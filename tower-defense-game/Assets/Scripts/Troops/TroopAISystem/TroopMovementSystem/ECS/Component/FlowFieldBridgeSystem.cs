using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Pathfinding.ECS
{
    /// <summary>
    /// Bridge system: samples PathfindingManager's shared flow fields each frame
    /// and writes the direction into NavigationTarget.flowFieldDirection for entities
    /// using navigationMode == 2 (SharedFlowField).
    ///
    /// Runs on the main thread (needs MonoBehaviour access) before TroopMovementSystem.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TroopMovementSystem))]
    public partial class FlowFieldBridgeSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<TroopTag>();
        }

        protected override void OnUpdate()
        {
            if (PathfindingManager.Instance == null) return;

            Entities
                .WithoutBurst() // needs managed PathfindingManager access
                .WithAll<TroopTag>()
                .ForEach((ref NavigationTarget nav, in LocalTransform transform) =>
                {
                    // only sample for SharedFlowField mode
                    if (nav.navigationMode != 2 || nav.isMoving == 0) return;

                    float2 currentPos = new float2(transform.Position.x, transform.Position.z);
                    Vector3 worldPos = new Vector3(currentPos.x, 0, currentPos.y);
                    Vector3 targetPos = new Vector3(nav.targetPosition.x, 0, nav.targetPosition.y);

                    Vector3 dir = PathfindingManager.Instance.GetSharedFlowFieldDirection(worldPos, targetPos);
                    nav.flowFieldDirection = new float2(dir.x, dir.z);
                })
                .Run();
        }
    }
}
