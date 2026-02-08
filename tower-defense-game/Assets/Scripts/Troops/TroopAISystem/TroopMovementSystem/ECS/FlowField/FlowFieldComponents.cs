using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Grid.ECS;

namespace Pathfinding.ECS
{
    /// <summary>
    /// blob root
    /// </summary>
    public struct FlowFieldBlob
    {
        public BlobArray<float2> directions;
        public BlobArray<float> costs;
        public int2 goalPosition;
        public int2 regionMin;
        public int2 regionMax;
        public int width;
        public int height;
        
        public int GetIndex(int2 localPos)
        {
            return localPos.y * width + localPos.x;
        }
        
        public float2 GetDirection(int2 localPos)
        {
            if (localPos.x < 0 || localPos.x >= width || localPos.y < 0 || localPos.y >= height)
            {
                return float2.zero;
            }
            else
            {
                return directions[GetIndex(localPos)];
            }
        }
    }

    public struct FlowFieldCache : IComponentData
    {
        public NativeHashMap<int2, BlobAssetReference<FlowFieldBlob>> cache;
        public int maxCacheSize;
        public int flowFieldRadius;
        
        // lru tracking
        public NativeHashMap<int2, float> lastAccessTime;
    }
    
    public struct FlowFieldCacheTag : IComponentData { }
    
    public struct FlowFieldRequest : IComponentData
    {
        public int2 targetGrid;
        public byte isProcessing;  // 0 = pending, 1 = processing, 2 = complete
    }
}