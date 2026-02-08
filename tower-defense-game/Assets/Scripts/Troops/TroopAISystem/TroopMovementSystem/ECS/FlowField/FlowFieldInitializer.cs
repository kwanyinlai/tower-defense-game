using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Pathfinding.ECS
{
    /// <summary>
    /// Bridge for flow field cache 
    /// </summary>
    public class FlowFieldCacheInitializer : MonoBehaviour
    {
        [Header("Flow Field Configuration")]
        [SerializeField] private int maxCachedFlowFields = 50;
        [SerializeField] private int flowFieldRadius = 30;
        
        private Entity cacheEntity;
        private EntityManager entityManager;
        private bool isInitialized = false;
        
        public static FlowFieldCacheInitializer Instance { get; private set; }
        
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        void Start()
        {
            World world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
            {
                Debug.LogError("ECS World not found");
                return;
            }
            
            entityManager = world.EntityManager;
            Invoke(nameof(InitializeCache), 0.2f);
            // wait for grid to initialise
        }
        
        void InitializeCache()
        {
            cacheEntity = entityManager.CreateEntity();
            entityManager.AddComponent<FlowFieldCacheTag>(cacheEntity);
            
            // create cache component with native containers
            FlowFieldCache cache = new FlowFieldCache
            {
                cache = new NativeHashMap<int2, BlobAssetReference<FlowFieldBlob>>(
                    maxCachedFlowFields, 
                    Allocator.Persistent
                ),
                lastAccessTime = new NativeHashMap<int2, float>(
                    maxCachedFlowFields, 
                    Allocator.Persistent
                ),
                maxCacheSize = maxCachedFlowFields,
                flowFieldRadius = flowFieldRadius
            };
            
            entityManager.AddComponentData(cacheEntity, cache);
            
            isInitialized = true;
        }
        
        public void InvalidateFlowFieldsInArea(Vector3 position, int2 size)
        {
            if (!isInitialized) return;
            
            Vector3Int gridPos = GridManager.WorldPosFromCoordinates(position);
            int2 centerGrid = new int2(gridPos.x, gridPos.z);
            
            int affectedRadius = Mathf.Max(size.x, size.y) / 2 + flowFieldRadius;
            
            FlowFieldCache cache = entityManager.GetComponentData<FlowFieldCache>(cacheEntity);
            NativeList<int2> keysToRemove = new NativeList<int2>(Allocator.Temp);
            
            NativeArray<int2> keys = cache.cache.GetKeyArray(Allocator.Temp);
            foreach (int2 key in keys)
            {
                float distance = math.distance(
                    new float2(centerGrid.x, centerGrid.y),
                    new float2(key.x, key.y)
                );
                
                if (distance <= affectedRadius)
                {
                    keysToRemove.Add(key);
                }
            }
            keys.Dispose();
            
            foreach (int2 key in keysToRemove)
            {
                if (cache.cache.TryGetValue(key, out BlobAssetReference<FlowFieldBlob> blobRef))
                {
                    blobRef.Dispose();
                    cache.cache.Remove(key);
                    cache.lastAccessTime.Remove(key);
                }
            }
            
            
            keysToRemove.Dispose();
            
            entityManager.SetComponentData(cacheEntity, cache);
        }
        
        public void ClearAllFlowFields()
        {
            if (!isInitialized) return;
            
            FlowFieldCache cache = entityManager.GetComponentData<FlowFieldCache>(cacheEntity);
            
            NativeArray<int2> keys = cache.cache.GetKeyArray(Allocator.Temp);
            foreach (int2 key in keys)
            {
                if (cache.cache.TryGetValue(key, out BlobAssetReference<FlowFieldBlob> blobRef))
                {
                    blobRef.Dispose();
                }
            }
            keys.Dispose();
            
            cache.cache.Clear();
            cache.lastAccessTime.Clear();
            
            entityManager.SetComponentData(cacheEntity, cache);
        }
        
        void OnDestroy()
        {
            if (isInitialized && entityManager != default && entityManager.Exists(cacheEntity))
            {
                FlowFieldCache cache = entityManager.GetComponentData<FlowFieldCache>(cacheEntity);
                
                // dispose blob assets
                NativeArray<int2> keys = cache.cache.GetKeyArray(Allocator.Temp);
                foreach (int2 key in keys)
                {
                    if (cache.cache.TryGetValue(key, out BlobAssetReference<FlowFieldBlob> blobRef))
                    {
                        blobRef.Dispose();
                    }
                }
                keys.Dispose();
                // clean cache
                cache.cache.Dispose();
                cache.lastAccessTime.Dispose();
            }
        }
    }
}