using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Grid.ECS;

namespace Pathfinding.ECS
{
    /// <summary>
    /// Finds the best cached flow field that covers the troop's current position
    /// and writes the direction to NavigationTarget.flowFieldDirection.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TroopMovementSystem))]
    public partial class FlowFieldSamplingSystem : SystemBase
    {
        private EntityQuery cacheQuery;
        private EntityQuery gridQuery;
        
        protected override void OnCreate()
        {
            cacheQuery = GetEntityQuery(typeof(FlowFieldCacheTag), typeof(FlowFieldCache));
            gridQuery = GetEntityQuery(typeof(GridDataTag), typeof(GridDataSingleton));
            
            RequireForUpdate(cacheQuery);
            RequireForUpdate(gridQuery);
            RequireForUpdate<TroopTag>();
        }
        
        protected override void OnUpdate()
        {
            Entity cacheEntity = cacheQuery.GetSingletonEntity();
            FlowFieldCache cache = EntityManager.GetComponentData<FlowFieldCache>(cacheEntity);
            
            Entity gridEntity = gridQuery.GetSingletonEntity();
            GridDataSingleton gridData = EntityManager.GetComponentData<GridDataSingleton>(gridEntity);
            
            float currentTime = (float)SystemAPI.Time.ElapsedTime;
            
            NativeArray<int2> cachedKeys = cache.cache.GetKeyArray(Allocator.Temp);
            
            Entities
                .WithoutBurst()
                .WithAll<TroopTag>()
                .ForEach((ref NavigationTarget nav, in LocalTransform transform) =>
                {
                    if (nav.isMoving == 0) return;
                    
                    float2 currentPos = new float2(transform.Position.x, transform.Position.z);
                    int2 currentGrid = WorldToGrid(currentPos, gridData);
                    
                    // choose flow field that has goal closest to our target and covers our current position
                    float bestScore = float.MaxValue;
                    float2 bestDirection = float2.zero;
                    int2 bestKey = default;
                    bool found = false;
                    
                    for (int i = 0; i < cachedKeys.Length; i++)
                    {
                        int2 key = cachedKeys[i];
                        if (!cache.cache.TryGetValue(key, out BlobAssetReference<FlowFieldBlob> flowFieldBlob))
                            continue;
                        
                        ref FlowFieldBlob flowField = ref flowFieldBlob.Value;
                        
                        // covered
                        int2 localPos = currentGrid - flowField.regionMin;
                        if (localPos.x < 0 || localPos.x >= flowField.width ||
                            localPos.y < 0 || localPos.y >= flowField.height)
                            continue;
                        
                        float2 dir = flowField.GetDirection(localPos);
                        if (math.lengthsq(dir) < 0.01f) continue;
                        
                        // score for goal closer to our target
                        float2 goalWorld = new float2(
                            (key.x - gridData.width / 2f) * gridData.tileSize,
                            (key.y - gridData.height / 2f) * gridData.tileSize
                        );
                        float score = math.distancesq(goalWorld, nav.targetPosition);
                        
                        if (score < bestScore)
                        {
                            bestScore = score;
                            bestDirection = dir;
                            bestKey = key;
                            found = true;
                        }
                    }
                    
                    if (found)
                    {
                        nav.useFlowField = 1;
                        nav.flowFieldDirection = bestDirection;
                        cache.lastAccessTime[bestKey] = currentTime;
                    }
                    else
                    {
                        nav.useFlowField = 0;
                    }
                })
                .Run();
            
            cachedKeys.Dispose();
            
            // updated access times for cache
            EntityManager.SetComponentData(cacheEntity, cache);
        }
        
        private int2 WorldToGrid(float2 worldPos, GridDataSingleton gridData)
        {
            int x = (int)math.floor(worldPos.x / gridData.tileSize) + gridData.width / 2;
            int y = (int)math.floor(worldPos.y / gridData.tileSize) + gridData.height / 2;
            return new int2(x, y);
        }
    }
}