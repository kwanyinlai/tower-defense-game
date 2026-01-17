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
    /// aligned to 24 bytes
    /// </summary>
    public struct NavigationTarget : IComponentData
    {
        public float2 targetPosition; // 8 bytes, final pos
        public float stoppingDistance; // 4 bytes
        public float slowdownDistance; // 4 bytes
        public byte isMoving;
        public byte reachedDestination;
        public byte useFlowField;
        // 19 bytes -> aligned to 24
    }
    
    /// <summary>
    /// Waypoint Progress
    /// aligned to 8 bytes
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