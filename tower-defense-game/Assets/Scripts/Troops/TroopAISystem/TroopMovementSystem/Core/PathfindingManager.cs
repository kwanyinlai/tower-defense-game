using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

/// <summary>
/// Simplified 3-tier pathfinding:
///   1. Direct steering   — short range (< directSteerDistance)
///   2. A* on node grid   — long range, per-agent
///   3. Shared flow field  — group commands to same destination
/// </summary>
public class PathfindingManager : MonoBehaviour
{
    public static PathfindingManager Instance { get; private set; }

    #region Configuration

    [Header("Performance Settings")]
    [SerializeField] private int maxPathRequestsPerFrame = 5;

    [Header("Navigation Thresholds")]
    [Tooltip("Below this distance, troops steer directly (no pathfinding)")]
    [SerializeField] private float directSteerDistance = 15f;

    [Tooltip("Number of troops sharing a destination before a flow field is generated")]
    [SerializeField] private int flowFieldGroupThreshold = 3;

    [Header("Flow Field Settings")]
    [Tooltip("Maximum shared flow fields kept alive at once")]
    [SerializeField] private int maxSharedFlowFields = 10;

    [Tooltip("How close two destinations must be (in grid tiles) to share a flow field")]
    [SerializeField] private float flowFieldMergeDistance = 3f;

    [SerializeField] private float flowFieldBlendRadius = 5f;

    [Header("Debug Visualization")]
    [SerializeField] private bool drawSharedFlowFieldGizmos = true;
    [SerializeField] private int debugSampleRadius = 10;
    [SerializeField] private float debugArrowLength = 0.6f;
    [SerializeField] private Color debugFlowArrowColor = Color.cyan;
    [SerializeField] private Color debugGoalColor = Color.red;

    #endregion

    private Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();

    // Shared flow fields keyed by destination grid tile
    private Dictionary<int2, FlowField> sharedFlowFields = new Dictionary<int2, FlowField>();

    // Track how many agents are using each shared flow field
    private Dictionary<int2, int> flowFieldRefCounts = new Dictionary<int2, int>();

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

    #region Navigation Mode Selection

    /// <summary>
    /// Determines the best navigation mode for a troop given its distance to target.
    /// <summary>
    /// Determines the best navigation mode for a single troop given its distance to target.
    /// Only chooses between DirectSteer and AStarWaypoints.
    /// SharedFlowField is only used via explicit group commands (SetTargetWithFlowField).
    /// </summary>
    public NavigationMode GetNavigationMode(Vector3 currentPosition, Vector3 targetPosition)
    {
        float distance = Vector3.Distance(currentPosition, targetPosition);

        if (distance <= directSteerDistance)
        {
            return NavigationMode.DirectSteer;
        }

        return NavigationMode.AStarWaypoints;
    }

    #endregion

    #region Path Requests (A* tier)

    public void RequestPath(Vector3 startPosition, Vector3 targetPosition, System.Action<PathResult> callback)
    {
        // short range — skip pathfinding entirely
        float distance = Vector3.Distance(startPosition, targetPosition);
        if (distance <= directSteerDistance)
        {
            callback?.Invoke(new PathResult
            {
                success = true,
                waypoints = new List<Vector3> { targetPosition },
                startPosition = startPosition,
                targetPosition = targetPosition,
                navigationMode = NavigationMode.DirectSteer
            });
            return;
        }

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

    private PathResult GeneratePath(PathRequest request)
    {
        // A* directly on the 200x200 grid
        List<Vector3> waypoints = GridPathfinder.FindPath(request.startPosition, request.targetPosition);

        if (waypoints == null || waypoints.Count == 0)
        {
            return new PathResult { success = false };
        }

        return new PathResult
        {
            success = true,
            waypoints = waypoints,
            startPosition = request.startPosition,
            targetPosition = request.targetPosition,
            navigationMode = NavigationMode.AStarWaypoints
        };
    }

    #endregion

    #region Shared Flow Fields (group command tier)

    /// <summary>
    /// Request a shared flow field for a group command destination.
    /// Multiple troops going to the same place share one field.
    /// Returns the flow field direction at the given position.
    /// </summary>
    public Vector3 GetSharedFlowFieldDirection(Vector3 currentPosition, Vector3 targetPosition)
    {
        int2 targetGrid = WorldToGrid(targetPosition);
        FlowField field = FindMatchingFlowField(targetGrid);

        if (field != null)
        {
            field.lastAccessed = Time.time;
            return field.GetDirectionAt(currentPosition);
        }

        // fallback to direct steering
        return (targetPosition - currentPosition).normalized;
    }

    /// <summary>
    /// Called when a group of troops is commanded to move to a shared destination.
    /// Creates a full-map flow field for that destination if one doesn't exist.
    /// </summary>
    public void RequestSharedFlowField(Vector3 targetPosition)
    {
        int2 targetGrid = WorldToGrid(targetPosition);

        if (FindMatchingFlowField(targetGrid) != null)
        {
            return; // already exists
        }

        // evict oldest if at capacity
        if (sharedFlowFields.Count >= maxSharedFlowFields)
        {
            RemoveOldestFlowField();
        }

        // full-map flow field (covers entire grid — cheap at 200x200)
        int radius = Mathf.Max(GridManager.GRID_WIDTH, GridManager.GRID_HEIGHT);
        FlowField newField = FlowFieldBuilder.CreateRegional(targetGrid, radius);
        newField.lastAccessed = Time.time;

        sharedFlowFields[targetGrid] = newField;
        flowFieldRefCounts[targetGrid] = 0;
    }

    /// <summary>
    /// Register a troop as using a shared flow field (for ref counting).
    /// </summary>
    public void RegisterFlowFieldUser(Vector3 targetPosition)
    {
        int2 targetGrid = WorldToGrid(targetPosition);
        int2? matchKey = FindMatchingFlowFieldKey(targetGrid);
        if (matchKey.HasValue)
        {
            flowFieldRefCounts[matchKey.Value]++;
        }
    }

    /// <summary>
    /// Unregister a troop from a shared flow field. Auto-cleanup when no users remain.
    /// </summary>
    public void UnregisterFlowFieldUser(Vector3 targetPosition)
    {
        int2 targetGrid = WorldToGrid(targetPosition);
        int2? matchKey = FindMatchingFlowFieldKey(targetGrid);
        if (matchKey.HasValue && flowFieldRefCounts.ContainsKey(matchKey.Value))
        {
            flowFieldRefCounts[matchKey.Value]--;
            if (flowFieldRefCounts[matchKey.Value] <= 0)
            {
                sharedFlowFields.Remove(matchKey.Value);
                flowFieldRefCounts.Remove(matchKey.Value);
            }
        }
    }

    /// <summary>
    /// Check if a shared flow field exists for this destination.
    /// </summary>
    public bool HasSharedFlowField(Vector3 targetPosition)
    {
        int2 targetGrid = WorldToGrid(targetPosition);
        return FindMatchingFlowField(targetGrid) != null;
    }

    private FlowField FindMatchingFlowField(int2 targetGrid)
    {
        // exact match first
        if (sharedFlowFields.TryGetValue(targetGrid, out FlowField exact))
            return exact;

        // fuzzy match — nearby destination shares the same field
        foreach (var kvp in sharedFlowFields)
        {
            float dist = math.distance(new float2(kvp.Key.x, kvp.Key.y), new float2(targetGrid.x, targetGrid.y));
            if (dist <= flowFieldMergeDistance)
                return kvp.Value;
        }

        return null;
    }

    private int2? FindMatchingFlowFieldKey(int2 targetGrid)
    {
        if (sharedFlowFields.ContainsKey(targetGrid))
            return targetGrid;

        foreach (var kvp in sharedFlowFields)
        {
            float dist = math.distance(new float2(kvp.Key.x, kvp.Key.y), new float2(targetGrid.x, targetGrid.y));
            if (dist <= flowFieldMergeDistance)
                return kvp.Key;
        }

        return null;
    }

    #endregion

    #region Cache Management

    public void InvalidateFlowFieldsInArea(Vector3 position, int2 size)
    {
        Vector3Int gridPos = GridManager.WorldPosFromCoordinates(position);
        int2 centerGrid = new int2(gridPos.x, gridPos.z);

        // any flow field whose goal is within the affected area needs regeneration
        List<int2> toRegenerate = new List<int2>();

        foreach (var kvp in sharedFlowFields)
        {
            // if the changed area overlaps with the flow field region, regenerate it
            FlowField field = kvp.Value;
            if (centerGrid.x + size.x >= field.regionMin.x && centerGrid.x - size.x <= field.regionMax.x &&
                centerGrid.y + size.y >= field.regionMin.y && centerGrid.y - size.y <= field.regionMax.y)
            {
                toRegenerate.Add(kvp.Key);
            }
        }

        foreach (int2 key in toRegenerate)
        {
            // regenerate in place
            int radius = Mathf.Max(GridManager.GRID_WIDTH, GridManager.GRID_HEIGHT);
            FlowField newField = FlowFieldBuilder.CreateRegional(key, radius);
            newField.lastAccessed = Time.time;
            sharedFlowFields[key] = newField;
        }
    }

    public void ClearAllCache()
    {
        sharedFlowFields.Clear();
        flowFieldRefCounts.Clear();
    }

    private void RemoveOldestFlowField()
    {
        int2 oldestKey = default;
        float oldestTime = float.MaxValue;

        foreach (var kvp in sharedFlowFields)
        {
            if (kvp.Value.lastAccessed < oldestTime)
            {
                oldestTime = kvp.Value.lastAccessed;
                oldestKey = kvp.Key;
            }
        }

        sharedFlowFields.Remove(oldestKey);
        flowFieldRefCounts.Remove(oldestKey);
    }

    #endregion

    #region Utility

    private int2 WorldToGrid(Vector3 worldPos)
    {
        Vector3Int gridPos = GridManager.WorldPosFromCoordinates(worldPos);
        return new int2(gridPos.x, gridPos.z);
    }

    private void OnDrawGizmos()
    {
        if (!drawSharedFlowFieldGizmos || !Application.isPlaying) return;
        if (GridManager.Instance == null || sharedFlowFields == null || sharedFlowFields.Count == 0) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 camPos = cam.transform.position;
        Vector3Int camGrid = GridManager.WorldPosFromCoordinates(camPos);
        float tileSize = GridManager.TILE_SIZE;

        foreach (var kvp in sharedFlowFields)
        {
            int2 targetGrid = kvp.Key;
            Vector3 targetWorld = GridManager.CoordinatesToWorldPos(new Vector3(targetGrid.x, 0, targetGrid.y));
            targetWorld.y = camPos.y;

            Gizmos.color = debugGoalColor;
            Gizmos.DrawWireSphere(targetWorld, tileSize * 0.6f);

            int minX = Mathf.Max(0, camGrid.x - debugSampleRadius);
            int maxX = Mathf.Min(GridManager.GRID_WIDTH - 1, camGrid.x + debugSampleRadius);
            int minZ = Mathf.Max(0, camGrid.z - debugSampleRadius);
            int maxZ = Mathf.Min(GridManager.GRID_HEIGHT - 1, camGrid.z + debugSampleRadius);

            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    Vector3 cellWorld = GridManager.CoordinatesToWorldPos(new Vector3(x, 0, z));
                    cellWorld.y = camPos.y + 0.15f;

                    Vector3 dir = GetSharedFlowFieldDirection(cellWorld, targetWorld);
                    if (dir.sqrMagnitude < 0.001f) continue;

                    Gizmos.color = debugFlowArrowColor;
                    DrawDebugArrow(cellWorld, new Vector3(dir.x, 0, dir.z) * debugArrowLength);
                }
            }
        }
    }

    private void DrawDebugArrow(Vector3 from, Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.0001f) return;

        Vector3 to = from + direction;
        Gizmos.DrawLine(from, to);

        float headSize = direction.magnitude * 0.3f;
        Vector3 dir = direction.normalized;
        Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;

        Gizmos.DrawLine(to, to - dir * headSize + right * headSize * 0.4f);
        Gizmos.DrawLine(to, to - dir * headSize - right * headSize * 0.4f);
    }

    #endregion

    #region Debug Info

    public int GetCachedFlowFieldCount() => sharedFlowFields.Count;
    public int GetQueuedPathRequests() => pathRequestQueue.Count;

    #endregion

    #region Getters
    public float DirectSteerDistance => directSteerDistance;
    public float FlowFieldBlendRadius => flowFieldBlendRadius;
    #endregion
}

#region Enums and Data Classes

public enum NavigationMode
{
    DirectSteer,        // short range — just move toward target
    AStarWaypoints,     // long range — follow A* waypoints
    SharedFlowField     // group command — sample shared flow field
}

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
    public Vector3 startPosition;
    public Vector3 targetPosition;
    public NavigationMode navigationMode;
}

#endregion