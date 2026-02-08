using Unity.Entities;
using Unity.Mathematics;

namespace Pathfinding.ECS
{
    /// <summary>
    /// Movement-Position Data
    /// aligned to 16 bytes
    /// </summary>
    public struct MovementData : IComponentData
    {
        public float2 velocity; // 8 bytes
        public float maxSpeed; // 4 bytes
        public float acceleration; // 4 bytes
    }
    
    /// <summary>
    /// Movement State
    /// </summary>
    public struct NavigationTarget : IComponentData
    {
        public float2 targetPosition; // position of navigation target
        public float2 flowFieldDirection; // direction from flow field (if used)
        public float stoppingDistance; // distance at which to stop moving towards target
        public float slowdownDistance; // distance at which to start slowing down
        public byte isMoving; // whether the troop is currently moving towards a target
        public byte reachedDestination; // whether the troop has reached its destination
        public byte useFlowField; // whether to use flow field navigation or otherwise
        // compiler auto-align?
    }
        
    /// <summary>
    /// Waypoint Progress
    /// </summary>
    public struct WaypointProgress : IComponentData
    {
        public int currentIndex; // waypoint index
        public int totalCount; // number of waypoints
    }
    
    // estimating typical path length <8
    public struct WaypointElement : IBufferElementData
    {
        public float3 position;
    }
    
    /// <summary>
    /// Tag component
    /// </summary>
    public struct TroopTag : IComponentData { }
}

