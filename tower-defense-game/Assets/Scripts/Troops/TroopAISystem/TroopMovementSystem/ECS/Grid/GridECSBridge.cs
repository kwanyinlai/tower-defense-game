using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

namespace Grid.ECS
{
    /// <summary>
    /// Bridge for GridManager
    /// </summary>
    public class GridECSBridge : MonoBehaviour
    {
        private Entity gridDataEntity;
        private EntityManager entityManager;
        private bool isInitialized = false;
        
        void Start()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
            {
                Debug.LogError("ECS World not found");
                return;
            }
            
            entityManager = world.EntityManager;
            
            // try guarantee GridManager has initialised
            Invoke(nameof(InitializeECSGrid), 0.1f);
        }
        
        void InitializeECSGrid()
        {
            if (GridManager.Instance == null)
            {
                Debug.LogError("Uninitialised manager object: GridManager");
                return;
            }
            
            gridDataEntity = entityManager.CreateEntity();
            entityManager.AddComponent<GridDataTag>(gridDataEntity);
            
            int totalSize = GridManager.GRID_WIDTH * GridManager.GRID_HEIGHT;
            var walkCosts = new NativeArray<float>(totalSize, Allocator.Persistent);
            
            // get initial costs from GridManager
            SyncGridToECS(walkCosts);
            
            var gridData = new GridDataSingleton
            {
                walkCosts = walkCosts,
                width = GridManager.GRID_WIDTH,
                height = GridManager.GRID_HEIGHT,
                tileSize = GridManager.TILE_SIZE
            };
            
            entityManager.AddComponentData(gridDataEntity, gridData);
            
            isInitialized = true;
            Debug.Log("GridECSBridge initialized and grid data synced to ECS.");
        }
        
        public void SyncGridChanges()
        {
            if (!isInitialized) return;
            
            var gridData = entityManager.GetComponentData<GridDataSingleton>(gridDataEntity);
            SyncGridToECS(gridData.walkCosts);
        }
        
        private void SyncGridToECS(NativeArray<float> walkCosts)
        {
            GridNode[,] grid = GridManager.Instance.GetGrid();
            
            for (int x = 0; x < GridManager.GRID_WIDTH; x++)
            {
                for (int y = 0; y < GridManager.GRID_HEIGHT; y++)
                {
                    int index = x + y * GridManager.GRID_WIDTH;
                    walkCosts[index] = grid[x, y].walkCost;
                }
            }
        }
        
        void OnDestroy()
        {
            if (isInitialized && entityManager != default)
            {
                var gridData = entityManager.GetComponentData<GridDataSingleton>(gridDataEntity);
                if (gridData.walkCosts.IsCreated)
                {
                    gridData.walkCosts.Dispose();
                }
            }
        }
    }
}