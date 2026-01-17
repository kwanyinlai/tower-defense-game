// TODO: need DOTS optimisation and multi target, flowfield reuse maybe, cached HPA*

using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

public class PathfindingManager : MonoBehaviour
{
    public static PathfindingManager Instance { get; private set; }

    #region Configuration

    [Header("Performance Settings")]
    [SerializeField] private int maxPathRequestsPerFrame = 5;
    [SerializeField] private int maxFlowFieldsInCache = 50;
    
    [Header("Flow Field Settings")]
    [SerializeField] private int flowFieldRadius = 30;
    [SerializeField] private float flowFieldActivationDistance = 30f;
    [SerializeField] private float flowFieldBlendRadius = 5f;
    [SerializeField] private float flowFieldSeedDistance = 25f; // max distance for sharing seed data

    #endregion
    
    private Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
    
    // TODO: use a LRU cache
    private Dictionary<int2, FlowField> flowFieldCache = new Dictionary<int2, FlowField>();
    
    // TODO: perhaps adapt this to be efficient with a quadtree; we can experiment with different
    // DS for efficiency
    private Dictionary<int2, List<int2>> tileCoverage = new Dictionary<int2, List<int2>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        ProcessPathRequests();
    }

    #region Path Requests

    public void RequestPath(Vector3 startPosition, Vector3 targetPosition, System.Action<PathResult> callback)
    {
        PathRequest request = new PathRequest
        {
            startPosition = startPosition,
            targetPosition = targetPosition,
            callback = callback,
            requestTime = Time.time
        };
        
        pathRequestQueue.Enqueue(request);
    }

    private void ProcessPathRequests()
    {
        int processed = 0;

        while (pathRequestQueue.Count > 0 && processed < maxPathRequestsPerFrame)
        {
            PathRequest request = pathRequestQueue.Dequeue();
            PathResult result = GeneratePath(request);
            request.callback?.Invoke(result);
            processed++;
        }
    }

    #endregion

    #region Path Generation

    private PathResult GeneratePath(PathRequest request)
    {
        Vector3Int startNodeCoordinates = GridManager.WorldPosFromCoordinates(request.startPosition);
        Vector3Int targetNodeCoordinates = GridManager.WorldPosFromCoordinates(request.targetPosition);
        GridNode startNode = GridManager.Instance.NodeFromGridCoordinate(startNodeCoordinates);
        GridNode targetNode = GridManager.Instance.NodeFromGridCoordinate(targetNodeCoordinates);

        // invalid path query
        if (startNode == null || targetNode == null)
        {
            return new PathResult { success = false };
        }

        // generate high level sector path
        List<GridSector> sectorPath = SectorManager.Instance.GenerateHighLevelSectorPath(
            startNode.gridSector, 
            targetNode.gridSector
        );

        // too close to generate a waypoint path
        if (sectorPath == null || sectorPath.Count == 0)
        {
            return new PathResult { success = false };
        }

        List<Vector3> waypoints = WaypointPathBuilder.GenerateWaypoints(
            sectorPath,
            startNodeCoordinates,
            targetNodeCoordinates
        );

        return new PathResult
        {
            success = true,
            waypoints = waypoints,
            sectorPath = sectorPath,
            startPosition = request.startPosition,
            targetPosition = request.targetPosition
        };
    }

    #endregion

    #region Flow Field Management

    public Vector3 GetFlowFieldDirection(Vector3 currentPosition, Vector3 targetPosition)
    {
        Vector3Int targetGridPos = GridManager.WorldPosFromCoordinates(targetPosition);
        int2 targetGrid = new int2(targetGridPos.x, targetGridPos.z);

        // use existing flow field
        if (flowFieldCache.TryGetValue(targetGrid, out FlowField exactMatch))
        {
            exactMatch.lastAccessed = Time.time;
            return exactMatch.GetDirectionAt(currentPosition);
        }

        // use seeding to speed up generating
        FlowField newField = CreateFlowFieldWithSeeding(targetGrid);
        return newField.GetDirectionAt(currentPosition);
    }


    private FlowField CreateFlowFieldWithSeeding(int2 targetGrid)
    {
        // seeding approach using existing flow fields to avoid repeat computations
        List<FlowField> seedFields = FindSeedFlowFields(targetGrid);

        FlowField newField;
        
        if (seedFields.Count > 0)
        {
            newField = FlowFieldBuilder.CreateRegionalWithSeed(targetGrid, flowFieldRadius, seedFields);
        }
        else
        {
            // create from scratch
            newField = FlowFieldBuilder.CreateRegional(targetGrid, flowFieldRadius);
        }
        // update access
        newField.lastAccessed = Time.time;

        // manage cache
        if (flowFieldCache.Count >= maxFlowFieldsInCache)
        {
            RemoveOldestFlowField();
        }

        
        flowFieldCache[targetGrid] = newField;

        // index coverage for future
        UpdateFlowFieldCoverage(targetGrid, newField);

        return newField;
    }

    private List<FlowField> FindSeedFlowFields(int2 targetGrid)
    {
        List<FlowField> seedFields = new List<FlowField>();

        foreach ((int2 goal, FlowField field) in flowFieldCache)
        {
            float distance = math.distance(
                new float2(goal.x, goal.y),
                new float2(targetGrid.x, targetGrid.y)
            );

            // seed if close enough
            if (distance <= flowFieldSeedDistance)
            {
                seedFields.Add(field);
                
                // hard cap on seeding, to maintain speed advantage to avoid excessive seeding
                if (seedFields.Count >= 3)
                    break;
            }
        }

        return seedFields;
    }


    private void UpdateFlowFieldCoverage(int2 goalPosition, FlowField flowField)
    {
        int minX = flowField.regionMin.x;
        int maxX = flowField.regionMax.x;
        int minY = flowField.regionMin.y;
        int maxY = flowField.regionMax.y;

        // low resolution for memory
        int step = 5;
        
        for (int x = minX; x < maxX; x += step)
        {
            for (int y = minY; y < maxY; y += step)
            {
                int2 tile = new int2(x, y);
                
                if (!tileCoverage.ContainsKey(tile))
                {
                    tileCoverage[tile] = new List<int2>();
                }
                
                tileCoverage[tile].Add(goalPosition);
            }
        }
    }

    private void RemoveFlowFieldCoverage(int2 goalPosition, FlowField flowField)
    {
        int minX = flowField.regionMin.x;
        int maxX = flowField.regionMax.x;
        int minY = flowField.regionMin.y;
        int maxY = flowField.regionMax.y;

        int step = 5;
        
        for (int x = minX; x < maxX; x += step)
        {
            for (int y = minY; y < maxY; y += step)
            {
                int2 tile = new int2(x, y);
                
                if (tileCoverage.TryGetValue(tile, out List<int2> goals))
                {
                    goals.Remove(goalPosition);
                    if (goals.Count == 0)
                    {
                        tileCoverage.Remove(tile);
                    }
                }
            }
        }
    }

    public bool ShouldUseFlowField(Vector3 currentPosition, Vector3 targetPosition)
    {
        return  Vector3.Distance(currentPosition, targetPosition) <= flowFieldActivationDistance;
    }

    private void RemoveOldestFlowField()
    {
        int2 oldestGoal = default;
        float oldestTime = float.MaxValue;
        FlowField oldestField = null;

        foreach ((int2 goal, FlowField flowField) in flowFieldCache)
        {
            if (flowField.lastAccessed < oldestTime)
            {
                oldestTime = flowField.lastAccessed;
                oldestGoal = goal;
                oldestField = flowField;
            }
        }

        if (oldestField != null)
        {
            RemoveFlowFieldCoverage(oldestGoal, oldestField);
            flowFieldCache.Remove(oldestGoal);
        }
    }

    public void ClearAllCache()
    {
        flowFieldCache.Clear();
        tileCoverage.Clear();
    }

    public void InvalidateFlowFieldsInArea(Vector3 position, int2 size)
    {
        Vector3Int gridPos = GridManager.WorldPosFromCoordinates(position);
        int2 centerGrid = new int2(gridPos.x, gridPos.z);

        int affectedRadius = Mathf.Max(size.x, size.y) / 2 + flowFieldRadius;

        List<int2> keysToRemove = new List<int2>();
        
        foreach ((int2 goal, FlowField field) in flowFieldCache)
        {
            float distance = math.distance(
                new float2(centerGrid.x, centerGrid.y),
                new float2(goal.x, goal.y)
            );

            if (distance <= affectedRadius)
            {
                keysToRemove.Add(goal);
            }
        }

        foreach (int2 key in keysToRemove)
        {
            if (flowFieldCache.TryGetValue(key, out FlowField field))
            {
                RemoveFlowFieldCoverage(key, field);
                flowFieldCache.Remove(key);
            }
        }
    }

    #endregion

    #region Debug Info

    public int GetCachedFlowFieldCount()
    {
        return flowFieldCache.Count;
    }

    public int GetQueuedPathRequests()
    {
        return pathRequestQueue.Count;
    }

    public int GetCoverageTileCount()
    {
        return tileCoverage.Count;
    }

    #endregion

    #region Getters
    public float FlowFieldActivationDistance => flowFieldActivationDistance;
    public float FlowFieldBlendRadius => flowFieldBlendRadius;
    #endregion
}

#region Custom Data Classes

public class PathRequest
{
    public Vector3 startPosition;
    public Vector3 targetPosition;
    public System.Action<PathResult> callback;
    public float requestTime;
}

public class PathResult
{
    public bool success;
    public List<Vector3> waypoints;
    public List<GridSector> sectorPath;
    public Vector3 startPosition;
    public Vector3 targetPosition;
}

#endregion