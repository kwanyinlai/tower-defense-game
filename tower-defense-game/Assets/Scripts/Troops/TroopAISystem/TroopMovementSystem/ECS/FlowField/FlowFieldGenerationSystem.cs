using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

using Grid.ECS;

namespace Pathfinding.ECS
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(FlowFieldSamplingSystem))]
    public partial class FlowFieldGenerationSystem : SystemBase
    {
        private EntityQuery gridQuery;
        private EntityQuery cacheQuery;
        
        private const int MAX_GENERATIONS_PER_FRAME = 2;
        
        protected override void OnCreate()
        {
            gridQuery = GetEntityQuery(typeof(GridDataTag), typeof(GridDataSingleton));
            cacheQuery = GetEntityQuery(typeof(FlowFieldCacheTag), typeof(FlowFieldCache));
            
            RequireForUpdate(gridQuery);
            RequireForUpdate(cacheQuery);
        }
        
        protected override void OnUpdate()
        {
            Entity gridEntity = gridQuery.GetSingletonEntity();
            GridDataSingleton gridData = EntityManager.GetComponentData<GridDataSingleton>(gridEntity);
            
            Entity cacheEntity = cacheQuery.GetSingletonEntity();
            FlowFieldCache cache = EntityManager.GetComponentData<FlowFieldCache>(cacheEntity);
            
            // rate limit flow field generations to avoid frame spikes
            int generatedThisFrame = 0;
            
            Entities
                .WithoutBurst() // blob builder + burst incompatibility
                // CreateFlowField -> CreateFlowFieldBlob -> BlobBuilder
                .ForEach((Entity entity, ref FlowFieldRequest request) =>
                {
                    if (request.isProcessing != 0) return; // 0 = not started, 1 = processing, 2 = complete
                    
                    if (cache.cache.ContainsKey(request.targetGrid))
                    {
                        cache.lastAccessTime[request.targetGrid] = (float)SystemAPI.Time.ElapsedTime;
                        request.isProcessing = 2;
                        return;
                    }
                    
                    // rate limiter
                    if (generatedThisFrame >= MAX_GENERATIONS_PER_FRAME)
                        return;
                    
                    // generate new flow field
                    request.isProcessing = 1;
                    
                    BlobAssetReference<FlowFieldBlob> blobRef = GenerateFlowField(
                        request.targetGrid, 
                        cache.flowFieldRadius, 
                        gridData
                    );
                    
                    generatedThisFrame++;
                    
                    // manage cache size
                    if (cache.cache.Count >= cache.maxCacheSize)
                    {
                        RemoveOldestFromCache(ref cache);
                    }
                    
                    // add to cache
                    cache.cache[request.targetGrid] = blobRef;
                    cache.lastAccessTime[request.targetGrid] = (float)SystemAPI.Time.ElapsedTime;
                    
                    request.isProcessing = 2;
                })
                .Run();
            
            // write back to cache to update access times
            EntityManager.SetComponentData(cacheEntity, cache);
        }
        
        private BlobAssetReference<FlowFieldBlob> GenerateFlowField(
            int2 goalPosition, 
            int radius, 
            GridDataSingleton gridData)
        {
            // flow field bounds
            int minX = math.max(0, goalPosition.x - radius);
            int maxX = math.min(gridData.width, goalPosition.x + radius);
            int minY = math.max(0, goalPosition.y - radius);
            int maxY = math.min(gridData.height, goalPosition.y + radius);
            
            int width = maxX - minX;
            int height = maxY - minY;
            
            // generate cost field
            NativeArray<float> costField = new NativeArray<float>(width * height, Allocator.Temp);
            GenerateCostField(goalPosition, minX, minY, width, height, gridData, costField);
            
            // generate flow field from cost field
            NativeArray<float2> flowField = new NativeArray<float2>(width * height, Allocator.Temp);
            GenerateFlowDirections(costField, width, height, flowField);
            
            // create blob
            BlobAssetReference<FlowFieldBlob> blobRef = CreateFlowFieldBlob(
                goalPosition, 
                new int2(minX, minY), 
                new int2(maxX, maxY),
                width, 
                height, 
                costField, 
                flowField
            );
            
            costField.Dispose();
            flowField.Dispose();
            
            return blobRef;
        }
        
        private void GenerateCostField(
            int2 goal, 
            int minX, 
            int minY, 
            int width, 
            int height,
            GridDataSingleton gridData,
            NativeArray<float> costField)
        {
            for (int i = 0; i < costField.Length; i++)
            {
                costField[i] = float.MaxValue;
            }
            
            int totalCells = width * height;
            NativeHashMap<int2, bool> visited = new NativeHashMap<int2, bool>(totalCells, Allocator.Temp);
            
            // openList linked with openCosts
            // no binary heap for simplicity - small radius
            // and lazy to implement in ECS manually
            NativeList<int2> openList = new NativeList<int2>(totalCells, Allocator.Temp);
            NativeList<float> openCosts = new NativeList<float>(totalCells, Allocator.Temp);
 
            int2 localGoal = goal - new int2(minX, minY);
            int goalIndex = localGoal.y * width + localGoal.x;
            costField[goalIndex] = 0;

            openList.Add(localGoal);
            openCosts.Add(0f);
            
            // 8-directional neighbors
            NativeArray<int2> neighbors = new NativeArray<int2>(8, Allocator.Temp);
            neighbors[0] = new int2(0, 1);   // N
            neighbors[1] = new int2(1, 0);   // E
            neighbors[2] = new int2(0, -1);  // S
            neighbors[3] = new int2(-1, 0);  // W
            neighbors[4] = new int2(1, 1);   // NE
            neighbors[5] = new int2(1, -1);  // SE
            neighbors[6] = new int2(-1, -1); // SW
            neighbors[7] = new int2(-1, 1);  // NW
            
            while (openList.Length > 0)
            {
                int bestIndex = 0;
                float bestCost = openCosts[0];
                
                for (int i = 1; i < openList.Length; i++)
                {
                    if (openCosts[i] < bestCost)
                    {
                        bestCost = openCosts[i];
                        bestIndex = i;
                    }
                }
                
                int2 current = openList[bestIndex];
                openList.RemoveAtSwapBack(bestIndex);
                openCosts.RemoveAtSwapBack(bestIndex);
                
                if (visited.ContainsKey(current)) continue;
                visited[current] = true;
                
                int currIndex = current.y * width + current.x;
                float currentCost = costField[currIndex];
                
                for (int i = 0; i < 8; i++)
                {
                    int2 neighbor = current + neighbors[i];

                    if (neighbor.x < 0 || neighbor.x >= width || neighbor.y < 0 || neighbor.y >= height)
                    {
                        continue;
                    }
                    
                    if (visited.ContainsKey(neighbor)) continue;
                    
                    // convert to global grid position for cost lookup
                    int2 globalNeighbor = neighbor + new int2(minX, minY);
            
                    if (!gridData.IsInBounds(globalNeighbor)) continue;
                    
                    float walkCost = gridData.GetWalkCost(globalNeighbor);
                    
                    if (walkCost >= float.MaxValue) continue;
                    
                    bool isDiagonal = neighbors[i].x != 0 && neighbors[i].y != 0;
                    float moveCost = isDiagonal ? 1.414f : 1.0f; // diagonal = sqrt(2) = 1.414
                    
                    float newCost = currentCost + moveCost * walkCost;
                    
                    int neighbourIndex = neighbor.y * width + neighbor.x;
                    
                    if (newCost < costField[neighbourIndex])
                    {
                        costField[neighbourIndex] = newCost;
                        openList.Add(neighbor);
                        openCosts.Add(newCost);
                    }
                }
            }
            
            visited.Dispose();
            openList.Dispose();
            openCosts.Dispose();
            neighbors.Dispose();
        }
        
        private void GenerateFlowDirections(
            NativeArray<float> costField, 
            int width, 
            int height,
            NativeArray<float2> flowField)
        {
            NativeArray<int2> neighbors = new NativeArray<int2>(8, Allocator.Temp);
            neighbors[0] = new int2(0, 1);   // N
            neighbors[1] = new int2(1, 0);   // E
            neighbors[2] = new int2(0, -1);  // S
            neighbors[3] = new int2(-1, 0);  // W
            neighbors[4] = new int2(1, 1);   // NE
            neighbors[5] = new int2(1, -1);  // SE
            neighbors[6] = new int2(-1, -1); // SW
            neighbors[7] = new int2(-1, 1);  // NW
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = y * width + x;
                    
                    if (costField[idx] >= float.MaxValue)
                    {
                        flowField[idx] = float2.zero;
                        continue;
                    }
                    
                    float lowestCost = float.MaxValue;
                    int2 bestDirection = new int2(0, 0);
                    
                    for (int i = 0; i < 8; i++)
                    {
                        int2 neighborPos = new int2(x, y) + neighbors[i];
                        
                        if (neighborPos.x < 0 || neighborPos.x >= width || 
                            neighborPos.y < 0 || neighborPos.y >= height)
                            continue;
                        
                        int neighbourIndex = neighborPos.y * width + neighborPos.x;
                        
                        if (costField[neighbourIndex] < lowestCost)
                        {
                            lowestCost = costField[neighbourIndex];
                            bestDirection = neighbors[i];
                        }
                    }
                    
                    if (bestDirection.x != 0 || bestDirection.y != 0)
                    {
                        float2 dir = new float2(bestDirection.x, bestDirection.y);
                        flowField[idx] = math.normalize(dir);
                    }
                    else
                    {
                        flowField[idx] = float2.zero;
                    }
                }
            }
            
            neighbors.Dispose();
        }
        
        private BlobAssetReference<FlowFieldBlob> CreateFlowFieldBlob(
            int2 goalPosition,
            int2 regionMin,
            int2 regionMax,
            int width,
            int height,
            NativeArray<float> costField,
            NativeArray<float2> flowField)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref FlowFieldBlob blob = ref builder.ConstructRoot<FlowFieldBlob>();
            
            blob.goalPosition = goalPosition;
            blob.regionMin = regionMin;
            blob.regionMax = regionMax;
            blob.width = width;
            blob.height = height;
            
            BlobArray<float> costsArray = builder.Allocate(ref blob.costs, costField.Length);
            for (int i = 0; i < costField.Length; i++)
            {
                costsArray[i] = costField[i];
            }
            
            BlobArray<float2> directionsArray = builder.Allocate(ref blob.directions, flowField.Length);
            for (int i = 0; i < flowField.Length; i++)
            {
                directionsArray[i] = flowField[i];
            }
            
            var blobRef = builder.CreateBlobAssetReference<FlowFieldBlob>(Allocator.Persistent);
            builder.Dispose();
            
            return blobRef;
        }
        
        private void RemoveOldestFromCache(ref FlowFieldCache cache)
        {
            float oldestTime = float.MaxValue;
            int2 oldestKey = default;
            bool found = false;
            
            var keys = cache.lastAccessTime.GetKeyArray(Allocator.Temp);
            foreach (var key in keys)
            {
                if (cache.lastAccessTime.TryGetValue(key, out float time))
                {
                    if (time < oldestTime)
                    {
                        oldestTime = time;
                        oldestKey = key;
                        found = true;
                    }
                }
            }
            keys.Dispose();
            
            if (found && cache.cache.TryGetValue(oldestKey, out var flowFieldBlobRef))
            {
                flowFieldBlobRef.Dispose();
                cache.cache.Remove(oldestKey);
                cache.lastAccessTime.Remove(oldestKey);
            }
        }
    }
}