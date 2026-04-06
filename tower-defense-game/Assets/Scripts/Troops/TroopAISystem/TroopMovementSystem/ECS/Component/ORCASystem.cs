using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Pathfinding.ECS
{
    /// <summary>
    /// Computes ORCA agent-to-agent avoidance for all troop entities.
    ///
    /// Pipeline each frame:
    ///   1. ComputePreferredVelocityJob — determine each agent's desired velocity from its nav tier
    ///   2. Collect all agent positions/velocities into flat NativeArrays
    ///   3. Build a spatial hash for neighbor queries
    ///   4. ORCAJob — compute avoidance-adjusted velocity and write to MovementData
    ///
    /// Runs BEFORE TroopMovementSystem (which now only applies velocity → position).
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FlowFieldBridgeSystem))]
    [UpdateBefore(typeof(TroopMovementSystem))]
    public partial class ORCASystem : SystemBase
    {
        private EntityQuery troopQuery;

        // ORCA parameters (match ORCAManager defaults)
        private const float AGENT_RADIUS       = 0.9f;
        private const float TIME_HORIZON       = 3f;
        private const float NEIGHBOR_DISTANCE   = 8f;
        private const int   MAX_NEIGHBORS       = 10;
        private const float CELL_SIZE           = 8f; // matches neighborDistance
        private const float STATIONARY_RADIUS_SCALE = 0.3f; // shrink stopped agents so others can approach

        // direction computation constants (from TroopMovementJob)
        private const float WAYPOINT_REACHED_DIST = 4f;
        private const float MIN_SPEED_MULTIPLIER  = 0.2f;
        private const float FLOW_FIELD_BLEND_RADIUS = 5f;

        protected override void OnCreate()
        {
            troopQuery = GetEntityQuery(
                ComponentType.ReadWrite<MovementData>(),
                ComponentType.ReadOnly<NavigationTarget>(),
                ComponentType.ReadOnly<LocalTransform>(),
                ComponentType.ReadOnly<TroopTag>(),
                ComponentType.ReadOnly<WaypointProgress>()
            );
            RequireForUpdate(troopQuery);
        }

        protected override void OnUpdate()
        {
            int agentCount = troopQuery.CalculateEntityCount();
            if (agentCount == 0) return;

            float dt = SystemAPI.Time.DeltaTime;

            // ---- Phase 1: compute preferred velocity per agent ----
            // We do this inline with the collection pass to avoid scheduling a separate job.

            var positions  = new NativeArray<float2>(agentCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var velocities = new NativeArray<float2>(agentCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var radii      = new NativeArray<float>(agentCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var maxSpeeds  = new NativeArray<float>(agentCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var prefVels   = new NativeArray<float2>(agentCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var entities   = new NativeArray<Entity>(agentCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var isStationary = new NativeArray<bool>(agentCount, Allocator.TempJob, NativeArrayOptions.ClearMemory);
            var goalDistances = new NativeArray<float>(agentCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var slowdownDists = new NativeArray<float>(agentCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            // collect + compute preferred velocity on main thread
            int idx = 0;
            foreach (var (movement, nav, transform, wpProgress, wpBuffer, avoidance, entity) in
                SystemAPI.Query<
                    RefRO<MovementData>,
                    RefRO<NavigationTarget>,
                    RefRO<LocalTransform>,
                    RefRO<WaypointProgress>,
                    DynamicBuffer<WaypointElement>,
                    RefRO<AvoidanceAgent>>()
                .WithAll<TroopTag>()
                .WithEntityAccess())
            {
                float2 pos = new float2(transform.ValueRO.Position.x, transform.ValueRO.Position.z);
                positions[idx]  = pos;
                velocities[idx] = movement.ValueRO.velocity;
                radii[idx]      = avoidance.ValueRO.radius;
                maxSpeeds[idx]  = movement.ValueRO.maxSpeed;
                entities[idx]   = entity;

                // compute preferred velocity (same logic as old TroopMovementJob)
                float2 pv = float2.zero;
                bool stationary = true;
                if (nav.ValueRO.isMoving == 1 && nav.ValueRO.reachedDestination == 0)
                {
                    float distSq = math.distancesq(pos, nav.ValueRO.targetPosition);
                    if (distSq >= nav.ValueRO.stoppingDistance * nav.ValueRO.stoppingDistance)
                    {
                        float2 dir = ComputeDirection(
                            pos, nav.ValueRO, wpProgress.ValueRO, wpBuffer);

                        float distance = math.sqrt(distSq);
                        float speedMul = 1f;
                        if (distance < nav.ValueRO.slowdownDistance)
                        {
                            speedMul = math.clamp(distance / nav.ValueRO.slowdownDistance,
                                                  MIN_SPEED_MULTIPLIER, 1f);
                        }
                        pv = dir * movement.ValueRO.maxSpeed * speedMul;
                        stationary = false;
                    }
                }
                prefVels[idx] = pv;

                // Track stationary status — stationary agents get a reduced radius
                // so other moving agents can approach them at the destination.
                isStationary[idx] = stationary;
                float dist = math.distance(pos, nav.ValueRO.targetPosition);
                goalDistances[idx] = dist;
                slowdownDists[idx] = nav.ValueRO.slowdownDistance;

                // Shrink own radius if stationary
                if (stationary)
                {
                    radii[idx] = avoidance.ValueRO.radius * STATIONARY_RADIUS_SCALE;
                }
                idx++;
            }

            // If we collected fewer than expected (entities without AvoidanceAgent), trim
            int actualCount = idx;

            // ---- Phase 2: build spatial hash ----
            var spatialHash = new NativeParallelMultiHashMap<int, int>(actualCount * 2, Allocator.TempJob);
            for (int i = 0; i < actualCount; i++)
            {
                int key = HashCell(
                    (int)math.floor(positions[i].x / CELL_SIZE),
                    (int)math.floor(positions[i].y / CELL_SIZE));
                spatialHash.Add(key, i);
            }

            // ---- Phase 3: ORCA job ----
            var adjustedVels = new NativeArray<float2>(actualCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var orcaJob = new ORCABurstJob
            {
                positions     = positions,
                velocities    = velocities,
                radii         = radii,
                maxSpeeds     = maxSpeeds,
                prefVels      = prefVels,
                spatialHash   = spatialHash,
                cellSize      = CELL_SIZE,
                timeHorizon   = TIME_HORIZON,
                maxNeighbors  = MAX_NEIGHBORS,
                neighborDist  = NEIGHBOR_DISTANCE,
                adjustedVels  = adjustedVels,
                agentCount    = actualCount
            };

            // run ORCA in parallel
            var handle = orcaJob.Schedule(actualCount, 32, Dependency);
            handle.Complete();

            // ---- Phase 4: write back adjusted velocity with goal-proximity dampening ----
            for (int i = 0; i < actualCount; i++)
            {
                var movRW = SystemAPI.GetComponentRW<MovementData>(entities[i]);
                var navRW = SystemAPI.GetComponentRW<NavigationTarget>(entities[i]);

                float2 pos2 = positions[i];
                float distSq2 = math.distancesq(pos2, navRW.ValueRO.targetPosition);
                float stopDistSq = navRW.ValueRO.stoppingDistance * navRW.ValueRO.stoppingDistance;

                if (navRW.ValueRO.isMoving == 0 || navRW.ValueRO.reachedDestination == 1)
                    continue;

                if (distSq2 < stopDistSq)
                {
                    navRW.ValueRW.reachedDestination = 1;
                    movRW.ValueRW.velocity = float2.zero;
                    continue;
                }

                // Goal-proximity dampening: fade ORCA influence near destination
                // so troops in a group can actually reach their shared target.
                float2 orcaVel = adjustedVels[i];
                float2 prefVel = prefVels[i];
                float slowDist = slowdownDists[i];
                float goalDist = goalDistances[i];

                if (slowDist > 0f && goalDist < slowDist)
                {
                    float orcaFactor = math.saturate(goalDist / slowDist);
                    orcaVel = math.lerp(prefVel, orcaVel, orcaFactor);
                }

                // lerp toward ORCA-adjusted velocity
                float lerpFactor = math.saturate(movRW.ValueRO.acceleration * dt);
                movRW.ValueRW.velocity = math.lerp(movRW.ValueRO.velocity, orcaVel, lerpFactor);
            }

            // dispose temp arrays
            positions.Dispose();
            velocities.Dispose();
            radii.Dispose();
            maxSpeeds.Dispose();
            prefVels.Dispose();
            entities.Dispose();
            isStationary.Dispose();
            goalDistances.Dispose();
            slowdownDists.Dispose();
            spatialHash.Dispose();
            adjustedVels.Dispose();
        }

        // ----------------------------------------------------------------
        // Direction computation — mirrors old TroopMovementJob logic
        // ----------------------------------------------------------------
        static float2 ComputeDirection(
            float2 pos,
            NavigationTarget nav,
            WaypointProgress progress,
            DynamicBuffer<WaypointElement> waypoints)
        {
            switch (nav.navigationMode)
            {
                case 0: // DirectSteer
                    return math.normalizesafe(nav.targetPosition - pos);

                case 1: // AStarWaypoints
                    return GetWaypointDir(pos, nav.targetPosition, progress, waypoints);

                case 2: // SharedFlowField
                    return GetFlowFieldDir(pos, nav);

                default:
                    return math.normalizesafe(nav.targetPosition - pos);
            }
        }

        static float2 GetWaypointDir(
            float2 pos,
            float2 finalTarget,
            WaypointProgress progress,
            DynamicBuffer<WaypointElement> waypoints)
        {
            if (progress.totalCount == 0 || waypoints.Length == 0)
                return math.normalizesafe(finalTarget - pos);

            float reachedSqr = WAYPOINT_REACHED_DIST * WAYPOINT_REACHED_DIST;
            int ci = progress.currentIndex;

            while (ci < progress.totalCount && ci < waypoints.Length)
            {
                float2 wp = new float2(waypoints[ci].position.x, waypoints[ci].position.z);
                if (math.distancesq(pos, wp) < reachedSqr) { ci++; continue; }

                if (ci + 1 < waypoints.Length && ci + 1 < progress.totalCount)
                {
                    float2 nextWp = new float2(waypoints[ci + 1].position.x, waypoints[ci + 1].position.z);
                    if (math.distancesq(pos, nextWp) < math.distancesq(wp, nextWp)) { ci++; continue; }
                }
                break;
            }

            if (ci >= progress.totalCount || ci >= waypoints.Length)
                return math.normalizesafe(finalTarget - pos);

            float2 target = new float2(waypoints[ci].position.x, waypoints[ci].position.z);
            return math.normalizesafe(target - pos);
        }

        static float2 GetFlowFieldDir(float2 pos, NavigationTarget nav)
        {
            float2 dir = nav.flowFieldDirection;
            if (math.lengthsq(dir) < 0.001f)
                return math.normalizesafe(nav.targetPosition - pos);

            float dist = math.distance(pos, nav.targetPosition);
            if (dist < FLOW_FIELD_BLEND_RADIUS)
            {
                float2 direct = math.normalizesafe(nav.targetPosition - pos);
                float blend = 1f - (dist / FLOW_FIELD_BLEND_RADIUS);
                dir = math.normalizesafe(math.lerp(dir, direct, blend));
            }
            return dir;
        }

        static int HashCell(int x, int z)
        {
            unchecked { return x * 73856093 ^ z * 19349663; }
        }
    }

    // ----------------------------------------------------------------
    // Burst-compiled ORCA job — processes each agent independently
    // ----------------------------------------------------------------
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
    public struct ORCABurstJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float2> positions;
        [ReadOnly] public NativeArray<float2> velocities;
        [ReadOnly] public NativeArray<float>  radii;
        [ReadOnly] public NativeArray<float>  maxSpeeds;
        [ReadOnly] public NativeArray<float2> prefVels;
        [ReadOnly] public NativeParallelMultiHashMap<int, int> spatialHash;

        public float cellSize;
        public float timeHorizon;
        public int   maxNeighbors;
        public float neighborDist;
        public int   agentCount;

        [WriteOnly] public NativeArray<float2> adjustedVels;

        public void Execute(int index)
        {
            float2 pos   = positions[index];
            float2 vel   = velocities[index];
            float  radius = radii[index];
            float  maxSpd = maxSpeeds[index];
            float2 pref   = prefVels[index];

            // gather neighbors
            var neighbors = new NativeArray<ORCASolver.AgentData>(maxNeighbors, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            int neighborCount = 0;

            int cx = (int)math.floor(pos.x / cellSize);
            int cz = (int)math.floor(pos.y / cellSize);
            float distSq = neighborDist * neighborDist;

            for (int dx = -1; dx <= 1 && neighborCount < maxNeighbors; dx++)
            {
                for (int dz = -1; dz <= 1 && neighborCount < maxNeighbors; dz++)
                {
                    int key = HashCell(cx + dx, cz + dz);

                    if (spatialHash.TryGetFirstValue(key, out int otherId, out var it))
                    {
                        do
                        {
                            if (otherId == index) continue;
                            if (math.distancesq(pos, positions[otherId]) > distSq) continue;

                            neighbors[neighborCount++] = new ORCASolver.AgentData
                            {
                                position = positions[otherId],
                                velocity = velocities[otherId],
                                radius   = radii[otherId],
                                maxSpeed = maxSpeeds[otherId]
                            };

                            if (neighborCount >= maxNeighbors) break;
                        }
                        while (spatialHash.TryGetNextValue(out otherId, ref it));
                    }
                }
            }

            float2 result;
            if (neighborCount > 0)
            {
                result = ORCASolver.ComputeNewVelocity(
                    pref, pos, vel, radius, maxSpd,
                    neighbors, neighborCount, timeHorizon);
            }
            else
            {
                result = pref;
            }

            neighbors.Dispose();
            adjustedVels[index] = result;
        }

        static int HashCell(int x, int z)
        {
            unchecked { return x * 73856093 ^ z * 19349663; }
        }
    }
}
