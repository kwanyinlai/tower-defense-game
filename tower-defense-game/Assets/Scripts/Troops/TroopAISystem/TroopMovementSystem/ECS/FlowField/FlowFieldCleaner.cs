using Unity.Entities;
using Unity.Collections;

namespace Pathfinding.ECS
{
    /// <summary>
    /// Delete completed requests
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FlowFieldGenerationSystem))]
    public partial class FlowFieldRequestCleanupSystem : SystemBase
    {
        private double lastCleanupTime;
        private const double CLEANUP_INTERVAL = 1.0;
        
        protected override void OnCreate()
        {
            RequireForUpdate<FlowFieldRequest>();
        }
        
        protected override void OnUpdate()
        {
            double currentTime = SystemAPI.Time.ElapsedTime;
            
            if (currentTime - lastCleanupTime < CLEANUP_INTERVAL) return;
            
            lastCleanupTime = currentTime;
            
            // temporary memory alllocation
            var completedRequests = new NativeList<Entity>(Allocator.Temp);
            
            Entities
                .WithAll<FlowFieldRequest>()
                .ForEach((Entity entity, in FlowFieldRequest request) =>
                {
                    if (request.isProcessing == 2) // (isProcessing == 2) = complete
                    {
                        completedRequests.Add(entity);
                    }
                })
                .Run();
            
            foreach (Entity req in completedRequests)
            {
                EntityManager.DestroyEntity(req);
            }
            
            completedRequests.Dispose();
        }
    }
}