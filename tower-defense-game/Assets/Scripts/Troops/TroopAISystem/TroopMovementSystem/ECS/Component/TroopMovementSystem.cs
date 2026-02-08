using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;


namespace Pathfinding.ECS
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct TroopMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TroopTag>();
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