using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Pathfinding.ECS
{
    [BurstCompile] // haha need for speed
    [UpdateInGroup(typeof(SimulationSystemGroup))] // group for gameplay logic?? not really sure actually
    public partial struct TroopMovementSystem : ISystem // partial necessary for DOTS, injecting extra code??
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TroopTag>(); // checking for tag!
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            
            new TroopMovementJob
            {
                deltaTime = deltaTime
            }.ScheduleParallel();
        }
    }
}