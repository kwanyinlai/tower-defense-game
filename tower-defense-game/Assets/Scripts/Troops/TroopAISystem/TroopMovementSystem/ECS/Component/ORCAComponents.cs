using Unity.Entities;
using Unity.Mathematics;

namespace Pathfinding.ECS
{
    /// <summary>
    /// Per-entity avoidance configuration.
    /// Add to every troop entity that should participate in ORCA.
    /// </summary>
    public struct AvoidanceAgent : IComponentData
    {
        /// <summary>Physical collision radius of this agent.</summary>
        public float radius;
    }

    /// <summary>
    /// Written each frame by ORCASystem with the desired velocity
    /// computed from the navigation tier (before ORCA adjustment).
    /// TroopMovementSystem reads this and applies it.
    /// </summary>
    public struct PreferredVelocity : IComponentData
    {
        public float2 value;
    }
}
