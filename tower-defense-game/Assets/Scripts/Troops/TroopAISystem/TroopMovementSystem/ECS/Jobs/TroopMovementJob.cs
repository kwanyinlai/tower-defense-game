using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Pathfinding.ECS
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
    public partial struct TroopMovementJob : IJobEntity
    {
        public float deltaTime;
        private const float WAYPOINT_REACHED_DIST = 2f;
        private const float MIN_SPEED_MULTIPLIER = 0.2f;
        
        void Execute(
            ref MovementData movement,
            ref NavigationTarget nav,
            ref LocalTransform transform,
            in DynamicBuffer<WaypointElement> waypoints,
            ref WaypointProgress progress)
        {
            // the logic is pretty much identical to the code in PathfindingManager.cs
            if (nav.isMoving == 0 || nav.reachedDestination == 1)
                return;
            
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
            
            float2 direction = CalculateDirection(
                currentPos,
                nav.targetPosition,
                waypoints,
                ref progress
            );
            
            UpdateVelocityAndPosition(
                ref movement,
                ref transform,
                direction,
                distance,
                nav.slowdownDistance,
                deltaTime
            );
        }
        
        float2 CalculateDirection(
            float2 currentPos,
            float2 finalTarget,
            in DynamicBuffer<WaypointElement> waypoints,
            ref WaypointProgress progress)
        {
            // no waypoints - straight to target
            if (progress.totalCount == 0 || waypoints.Length == 0)
            {
                return math.normalizesafe(finalTarget - currentPos);
            }
            
            // IMPORTANT: validate index before accessing buffer
            if (progress.currentIndex >= waypoints.Length || progress.currentIndex >= progress.totalCount)
            {
                return math.normalizesafe(finalTarget - currentPos);
            }
            
            float3 waypointPos3D = waypoints[progress.currentIndex].position;
            float2 waypointPos = new float2(waypointPos3D.x, waypointPos3D.z);
            
            float waypointDistanceSqr = math.distancesq(currentPos, waypointPos);
            const float waypointReachedDistanceSqr = WAYPOINT_REACHED_DIST * WAYPOINT_REACHED_DIST;
            
            if (waypointDistanceSqr < waypointReachedDistanceSqr)
            {
                progress.currentIndex++;
                
                // last waypoint check
                if (progress.currentIndex >= progress.totalCount || progress.currentIndex >= waypoints.Length)
                {
                    return math.normalizesafe(finalTarget - currentPos);
                }
                
                waypointPos3D = waypoints[progress.currentIndex].position;
                waypointPos = new float2(waypointPos3D.x, waypointPos3D.z);
            }
            
            // move curr
            return math.normalizesafe(waypointPos - currentPos);
        }
        
        void UpdateVelocityAndPosition(
            ref MovementData movement,
            ref LocalTransform transform,
            float2 direction,
            float distanceToGoal,
            float slowdownDistance,
            float deltaTime)
        {
            float speedMultiplier = 1f;
            if (distanceToGoal < slowdownDistance)
            {
                speedMultiplier = math.clamp(
                    distanceToGoal / slowdownDistance,
                    MIN_SPEED_MULTIPLIER,
                    1f
                );
            }
            
            // Calculate desired velocity
            float2 desiredVelocity = direction * movement.maxSpeed * speedMultiplier;
            
            // Smoothly lerp current velocity to desired velocity
            float lerpFactor = math.saturate(movement.acceleration * deltaTime);
            movement.velocity = math.lerp(movement.velocity, desiredVelocity, lerpFactor);
            
            // Calculate movement delta
            float2 delta = movement.velocity * deltaTime;
            
            // Prevent overshooting the goal
            if (math.lengthsq(delta) > distanceToGoal * distanceToGoal)
            {
                delta = math.normalizesafe(delta) * distanceToGoal;
            }
            
            // Update position
            transform.Position += new float3(delta.x, 0, delta.y);
            
            // Update rotation to face movement direction
            if (math.lengthsq(movement.velocity) > 0.01f)
            {
                float angle = math.atan2(movement.velocity.y, movement.velocity.x);
                // Subtract PI/2 to align with Unity's forward = +Z axis
                transform.Rotation = quaternion.Euler(0, angle - math.PI / 2, 0);
            }
        }
    }
}