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
    /// Movement State — mirrors the 3-tier system from PathfindingManager.
    /// navigationMode: 0 = DirectSteer, 1 = AStarWaypoints, 2 = SharedFlowField
    /// </summary>
    public struct NavigationTarget : IComponentData
    {
        public float2 targetPosition;      // final destination
        public float2 flowFieldDirection;   // pre-sampled direction (written by bridge each frame when mode == 2)
        public float stoppingDistance;
        public float slowdownDistance;
        public byte isMoving;
        public byte reachedDestination;
        public byte navigationMode;        // 0 = DirectSteer, 1 = AStarWaypoints, 2 = SharedFlowField
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

