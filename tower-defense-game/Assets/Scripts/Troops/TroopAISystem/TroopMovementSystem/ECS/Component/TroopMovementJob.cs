using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Pathfinding.ECS
{
    /// <summary>
    /// Burst-compiled job that APPLIES the velocity computed by ORCASystem to position/rotation.
    ///
    /// ORCASystem handles:  direction → preferred velocity → ORCA adjustment → MovementData.velocity
    /// This job handles:    MovementData.velocity → position + rotation update
    ///
    /// For entities WITHOUT AvoidanceAgent, this job also computes direction + velocity
    /// as a fallback (same 3-tier logic as before).
    /// </summary>
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
    public partial struct TroopMovementJob : IJobEntity
    {
        public float deltaTime;
        
        private const float WAYPOINT_REACHED_DIST = 4f;
        private const float MIN_SPEED_MULTIPLIER = 0.2f;
        private const float FLOW_FIELD_BLEND_RADIUS = 5f;
        
        void Execute(
            ref MovementData movement,
            ref NavigationTarget nav,
            ref LocalTransform transform,
            in DynamicBuffer<WaypointElement> waypoints,
            ref WaypointProgress progress)
        {
            if (nav.isMoving == 0 || nav.reachedDestination == 1) return;
            
            float2 currentPos = new float2(transform.Position.x, transform.Position.z);
            float distSqr = math.distancesq(currentPos, nav.targetPosition);
            float stopDistSqr = nav.stoppingDistance * nav.stoppingDistance;
            
            if (distSqr < stopDistSqr)
            {
                nav.reachedDestination = 1;
                movement.velocity = float2.zero;
                return;
            }
            
            float distance = math.sqrt(distSqr);
            
            // Apply velocity to position (velocity was already set by ORCASystem)
            // For entities without AvoidanceAgent, ORCASystem won't touch them,
            // so we compute velocity here as fallback
            if (math.lengthsq(movement.velocity) < 1e-8f && nav.isMoving == 1)
            {
                // Fallback: compute direction + velocity for non-ORCA entities
                float2 direction;
                switch (nav.navigationMode)
                {
                    case 0: direction = math.normalizesafe(nav.targetPosition - currentPos); break;
                    case 1: direction = GetWaypointDirection(currentPos, nav.targetPosition, waypoints, ref progress); break;
                    case 2: direction = GetFlowFieldDirection(currentPos, nav, distance); break;
                    default: direction = math.normalizesafe(nav.targetPosition - currentPos); break;
                }

                float speedMul = 1f;
                if (distance < nav.slowdownDistance)
                    speedMul = math.clamp(distance / nav.slowdownDistance, MIN_SPEED_MULTIPLIER, 1f);

                float2 desiredVel = direction * movement.maxSpeed * speedMul;
                float lerpFactor = math.saturate(movement.acceleration * deltaTime);
                movement.velocity = math.lerp(movement.velocity, desiredVel, lerpFactor);
            }
            
            // Advance waypoint progress for entities using waypoints
            if (nav.navigationMode == 1)
            {
                AdvanceWaypoints(currentPos, waypoints, ref progress);
            }

            // Apply position
            float2 delta = movement.velocity * deltaTime;
            if (math.lengthsq(delta) > distance * distance)
            {
                delta = math.normalizesafe(delta) * distance;
            }
            
            transform.Position += new float3(delta.x, 0, delta.y);
            
            // Apply rotation
            if (math.lengthsq(movement.velocity) > 0.01f)
            {
                float angle = math.atan2(movement.velocity.y, movement.velocity.x);
                transform.Rotation = quaternion.Euler(0, angle - math.PI / 2, 0);
            }
        }
        
        /// <summary>
        /// Advance waypoint index past reached/overshot waypoints.
        /// </summary>
        void AdvanceWaypoints(
            float2 currentPos,
            in DynamicBuffer<WaypointElement> waypoints,
            ref WaypointProgress progress)
        {
            if (progress.totalCount == 0 || waypoints.Length == 0) return;
            
            float reachedSqr = WAYPOINT_REACHED_DIST * WAYPOINT_REACHED_DIST;
            
            while (progress.currentIndex < progress.totalCount && 
                   progress.currentIndex < waypoints.Length)
            {
                float3 wp3 = waypoints[progress.currentIndex].position;
                float2 wp = new float2(wp3.x, wp3.z);
                float distSqr = math.distancesq(currentPos, wp);
                
                if (distSqr < reachedSqr) { progress.currentIndex++; continue; }
                
                if (progress.currentIndex + 1 < waypoints.Length &&
                    progress.currentIndex + 1 < progress.totalCount)
                {
                    float3 nextWp3 = waypoints[progress.currentIndex + 1].position;
                    float2 nextWp = new float2(nextWp3.x, nextWp3.z);
                    if (math.distancesq(currentPos, nextWp) < math.distancesq(wp, nextWp))
                    { progress.currentIndex++; continue; }
                }
                break;
            }
        }
        
        /// <summary>
        /// Waypoint direction (fallback for non-ORCA entities).
        /// </summary>
        float2 GetWaypointDirection(
            float2 currentPos,
            float2 finalTarget,
            in DynamicBuffer<WaypointElement> waypoints,
            ref WaypointProgress progress)
        {
            if (progress.totalCount == 0 || waypoints.Length == 0)
                return math.normalizesafe(finalTarget - currentPos);
            
            float reachedSqr = WAYPOINT_REACHED_DIST * WAYPOINT_REACHED_DIST;
            
            while (progress.currentIndex < progress.totalCount && 
                   progress.currentIndex < waypoints.Length)
            {
                float3 wp3 = waypoints[progress.currentIndex].position;
                float2 wp = new float2(wp3.x, wp3.z);
                float distSqr = math.distancesq(currentPos, wp);
                
                if (distSqr < reachedSqr) { progress.currentIndex++; continue; }
                
                if (progress.currentIndex + 1 < waypoints.Length &&
                    progress.currentIndex + 1 < progress.totalCount)
                {
                    float3 nextWp3 = waypoints[progress.currentIndex + 1].position;
                    float2 nextWp = new float2(nextWp3.x, nextWp3.z);
                    if (math.distancesq(currentPos, nextWp) < math.distancesq(wp, nextWp))
                    { progress.currentIndex++; continue; }
                }
                break;
            }
            
            if (progress.currentIndex >= progress.totalCount || 
                progress.currentIndex >= waypoints.Length)
                return math.normalizesafe(finalTarget - currentPos);
            
            float3 targetWp3 = waypoints[progress.currentIndex].position;
            float2 targetWp = new float2(targetWp3.x, targetWp3.z);
            return math.normalizesafe(targetWp - currentPos);
        }
        
        /// <summary>
        /// Flow field direction with blend (fallback for non-ORCA entities).
        /// </summary>
        float2 GetFlowFieldDirection(float2 currentPos, NavigationTarget nav, float distance)
        {
            float2 direction = nav.flowFieldDirection;
            if (math.lengthsq(direction) < 0.001f)
                return math.normalizesafe(nav.targetPosition - currentPos);
            
            if (distance < FLOW_FIELD_BLEND_RADIUS)
            {
                float2 directDir = math.normalizesafe(nav.targetPosition - currentPos);
                float blendFactor = 1f - (distance / FLOW_FIELD_BLEND_RADIUS);
                direction = math.normalizesafe(math.lerp(direction, directDir, blendFactor));
            }
            return direction;
        }
    }
}