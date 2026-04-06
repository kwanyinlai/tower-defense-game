using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

namespace Grid.ECS
{
    /// <summary>
    /// ECS component for GridManager data
    /// </summary>
    public struct GridDataSingleton : IComponentData
    {
        // flattened; index = x + y * width
        public NativeArray<float> walkCosts;
        
        public int width;
        public int height;
        public float tileSize;
        
        public int GetIndex(int x, int y)
        {
            return x + y * width;
        }
        
        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }
        
        public bool IsInBounds(int2 pos)
        {
            return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
        }
        
        public float GetWalkCost(int x, int y)
        {
            if (!IsInBounds(x, y)) return float.MaxValue;
            return walkCosts[GetIndex(x, y)];
        }
        
        public float GetWalkCost(int2 pos)
        {
            return GetWalkCost(pos.x, pos.y);
        }
    }
   
    public struct GridDataTag : IComponentData { }
}