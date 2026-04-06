using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Pathfinding.ECS;

/// <summary>
/// Click-to-select debug visualizer for ECS troops.
/// Left-click a troop to inspect its navigation state, waypoints, velocity.
/// When using SharedFlowField mode, draws the flow field overlay from PathfindingManager.
/// </summary>
public class FlowFieldDebugSelector : MonoBehaviour
{
    [Header("Selection")]
    [SerializeField] private KeyCode selectKey = KeyCode.Mouse0;
    [SerializeField] private float selectionRadius = 6f;
    [SerializeField] private LayerMask selectionMask = ~0;
    [SerializeField] private bool autoFallbackToFirstTroop = true;
    
    [Header("Flow Field Visualization")]
    [SerializeField] private bool showFlowFieldDirections = true;
    [SerializeField] private float arrowLength = 0.6f;
    [SerializeField] private Color flowFieldArrowColor = Color.cyan;
    [SerializeField] private Color flowFieldBoundsColor = new Color(0f, 1f, 1f, 0.4f);
    [SerializeField] private Color goalColor = Color.red;
    
    [Header("Waypoint Visualization")]
    [SerializeField] private Color waypointColor = Color.yellow;
    [SerializeField] private Color waypointLineColor = new Color(1f, 1f, 0f, 0.6f);
    
    [Header("Troop Info")]
    [SerializeField] private Color velocityColor = Color.green;
    [SerializeField] private Color directionColor = new Color(0f, 0.8f, 1f, 1f);
    [SerializeField] private Color targetLineColor = new Color(1f, 0.3f, 0.3f, 0.5f);
    
    // State
    private EntityManager entityManager;
    private Entity selectedEntity;
    private bool hasSelection = false;
    private int selectedIndex = -1;
    
    // References
    private PathfindingTestGeneration testGen;
    private float nextFallbackAttemptTime;
    
    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        testGen = GetComponent<PathfindingTestGeneration>();
        if (testGen == null)
            testGen = FindObjectOfType<PathfindingTestGeneration>();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(selectKey))
        {
            TrySelectTroop();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearSelection();
        }
        
        if (hasSelection && !entityManager.Exists(selectedEntity))
        {
            ClearSelection();
        }

        // If click selection fails (no collider / input mismatch), keep debug usable.
        if (!hasSelection && autoFallbackToFirstTroop && Time.time >= nextFallbackAttemptTime)
        {
            TryFallbackSelect();
            nextFallbackAttemptTime = Time.time + 0.5f;
        }
    }

    private void TryFallbackSelect()
    {
        var ecsEntities = GetECSEntities();
        if (ecsEntities == null || ecsEntities.Count == 0) return;

        for (int i = 0; i < ecsEntities.Count; i++)
        {
            Entity entity = ecsEntities[i];
            if (!entityManager.Exists(entity)) continue;

            selectedEntity = entity;
            selectedIndex = i;
            hasSelection = true;
            return;
        }
    }
    
    private void TrySelectTroop()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, selectionMask))
        {
            SelectNearestEntity(hit.point);
            return;
        }
        
        Plane groundPlane = new Plane(Vector3.up, Vector3.up);
        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 worldPoint = ray.GetPoint(enter);
            SelectNearestEntity(worldPoint);
        }
    }
    
    private void SelectNearestEntity(Vector3 worldPoint)
    {
        if (testGen == null) return;
        
        var ecsEntities = GetECSEntities();
        if (ecsEntities == null || ecsEntities.Count == 0) return;
        
        float bestDistSqr = selectionRadius * selectionRadius;
        int bestIndex = -1;
        Entity bestEntity = Entity.Null;
        
        for (int i = 0; i < ecsEntities.Count; i++)
        {
            Entity entity = ecsEntities[i];
            if (!entityManager.Exists(entity)) continue;
            
            var transform = entityManager.GetComponentData<LocalTransform>(entity);
            float distSqr = math.distancesq(
                new float3(worldPoint.x, worldPoint.y, worldPoint.z),
                transform.Position
            );
            
            if (distSqr < bestDistSqr)
            {
                bestDistSqr = distSqr;
                bestIndex = i;
                bestEntity = entity;
            }
        }
        
        if (bestIndex >= 0)
        {
            selectedEntity = bestEntity;
            selectedIndex = bestIndex;
            hasSelection = true;
            Debug.Log($"[FlowFieldDebug] Selected entity #{bestIndex}");
        }
        else
        {
            ClearSelection();
        }
    }
    
    private void ClearSelection()
    {
        hasSelection = false;
        selectedIndex = -1;
        selectedEntity = Entity.Null;
    }
    
    private System.Collections.Generic.List<Entity> GetECSEntities()
    {
        var field = typeof(PathfindingTestGeneration).GetField("ecsMovement",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null && testGen != null)
        {
            return field.GetValue(testGen) as System.Collections.Generic.List<Entity>;
        }
        return null;
    }
    
    void OnGUI()
    {
        if (!hasSelection || !Application.isPlaying) return;
        if (!entityManager.Exists(selectedEntity)) return;
        
        var nav = entityManager.GetComponentData<NavigationTarget>(selectedEntity);
        var transform = entityManager.GetComponentData<LocalTransform>(selectedEntity);
        var movement = entityManager.GetComponentData<MovementData>(selectedEntity);
        var progress = entityManager.GetComponentData<WaypointProgress>(selectedEntity);
        
        float2 currentPos = new float2(transform.Position.x, transform.Position.z);
        float distToTarget = math.distance(currentPos, nav.targetPosition);
        float speed = math.length(movement.velocity);
        bool hasSharedField = PathfindingManager.Instance != null &&
            PathfindingManager.Instance.HasSharedFlowField(new Vector3(nav.targetPosition.x, 0, nav.targetPosition.y));
        
        string modeName = nav.navigationMode switch
        {
            0 => "DIRECT STEER",
            1 => "A* WAYPOINTS",
            2 => "SHARED FLOW FIELD",
            _ => "UNKNOWN"
        };
        
        GUILayout.BeginArea(new Rect(Screen.width - 360, 10, 350, 400));
        
        GUI.color = Color.white;
        GUILayout.Label("=== Selected Troop Debug ===");
        GUILayout.Label($"Entity Index: #{selectedIndex}");
        GUILayout.Label($"Position: ({transform.Position.x:F1}, {transform.Position.z:F1})");
        GUILayout.Label($"Target: ({nav.targetPosition.x:F1}, {nav.targetPosition.y:F1})");
        GUILayout.Label($"Distance: {distToTarget:F1}");
        GUILayout.Space(5);
        
        GUILayout.Label($"Nav Mode: {modeName}");
        if (nav.navigationMode == 2)
            GUILayout.Label($"FF Direction: ({nav.flowFieldDirection.x:F2}, {nav.flowFieldDirection.y:F2})");
        GUILayout.Label($"Shared FF Cached: {(hasSharedField ? "Yes" : "No")}");
        GUILayout.Space(5);
        
        GUILayout.Label($"Speed: {speed:F2} / {movement.maxSpeed:F2}");
        GUILayout.Label($"Velocity: ({movement.velocity.x:F2}, {movement.velocity.y:F2})");
        GUILayout.Label($"Waypoint: {progress.currentIndex} / {progress.totalCount}");
        GUILayout.Label($"Moving: {(nav.isMoving == 1 ? "Yes" : "No")}");
        GUILayout.Label($"Reached: {(nav.reachedDestination == 1 ? "Yes" : "No")}");
        GUILayout.Space(10);
        
        GUILayout.Label("[Click] Select  [Esc] Deselect");
        
        GUILayout.EndArea();
    }
    
    void OnDrawGizmos()
    {
        if (!hasSelection || !Application.isPlaying) return;
        if (entityManager == default || !entityManager.Exists(selectedEntity)) return;
        
        var nav = entityManager.GetComponentData<NavigationTarget>(selectedEntity);
        var transform = entityManager.GetComponentData<LocalTransform>(selectedEntity);
        var movement = entityManager.GetComponentData<MovementData>(selectedEntity);
        var progress = entityManager.GetComponentData<WaypointProgress>(selectedEntity);
        
        Vector3 troopPos = transform.Position;
        Vector3 targetPos = new Vector3(nav.targetPosition.x, troopPos.y, nav.targetPosition.y);
        
        // --- Selection ring ---
        Gizmos.color = Color.white;
        DrawCircle(troopPos, 1.5f, 16);
        
        // --- Line to target ---
        Gizmos.color = targetLineColor;
        Gizmos.DrawLine(troopPos, targetPos);
        Gizmos.color = goalColor;
        Gizmos.DrawWireSphere(targetPos, 0.8f);
        
        // --- Velocity vector ---
        if (math.lengthsq(movement.velocity) > 0.01f)
        {
            Gizmos.color = velocityColor;
            Vector3 velDir = new Vector3(movement.velocity.x, 0, movement.velocity.y);
            Gizmos.DrawRay(troopPos, velDir * 3f);
        }
        
        // --- Flow field direction arrow (mode 2) ---
        if (nav.navigationMode == 2 && math.lengthsq(nav.flowFieldDirection) > 0.001f)
        {
            Gizmos.color = directionColor;
            Vector3 ffDir = new Vector3(nav.flowFieldDirection.x, 0, nav.flowFieldDirection.y);
            DrawArrow(troopPos + Vector3.up * 0.5f, ffDir * 4f);
        }
        
        // --- Waypoints (mode 1) ---
        if (progress.totalCount > 0)
        {
            var waypoints = entityManager.GetBuffer<WaypointElement>(selectedEntity);
            
            for (int i = 0; i < waypoints.Length && i < progress.totalCount; i++)
            {
                Vector3 wpPos = waypoints[i].position;
                
                if (i < progress.currentIndex)
                {
                    Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                    Gizmos.DrawWireSphere(wpPos, 0.5f);
                }
                else if (i == progress.currentIndex)
                {
                    Gizmos.color = waypointColor;
                    Gizmos.DrawWireSphere(wpPos, 1f);
                    Gizmos.DrawLine(troopPos, wpPos);
                }
                else
                {
                    Gizmos.color = new Color(waypointColor.r, waypointColor.g, waypointColor.b, 0.5f);
                    Gizmos.DrawWireSphere(wpPos, 0.7f);
                }
                
                if (i < waypoints.Length - 1 && i + 1 < progress.totalCount && i >= progress.currentIndex)
                {
                    Gizmos.color = waypointLineColor;
                    Gizmos.DrawLine(wpPos, waypoints[i + 1].position);
                }
            }
        }
        
        // --- Flow field overlay ---
        // Render when selected troop is in SharedFlowField mode OR when a shared field
        // exists for the selected troop's target (useful after selection/fallback changes).
        bool hasSharedField = PathfindingManager.Instance != null &&
            PathfindingManager.Instance.HasSharedFlowField(new Vector3(nav.targetPosition.x, 0, nav.targetPosition.y));
        if (showFlowFieldDirections && (nav.navigationMode == 2 || hasSharedField))
        {
            DrawFlowFieldOverlay(nav, troopPos);
        }
    }
    
    /// <summary>
    /// Draws the shared flow field from PathfindingManager around the troop's position.
    /// Samples the flow field direction at each nearby grid cell.
    /// </summary>
    private void DrawFlowFieldOverlay(NavigationTarget nav, Vector3 troopPos)
    {
        if (PathfindingManager.Instance == null) return;
        if (!PathfindingManager.Instance.HasSharedFlowField(
            new Vector3(nav.targetPosition.x, 0, nav.targetPosition.y)))
            return;

        float tileSize = GridManager.TILE_SIZE;
        int visualRadius = 12;

        // convert troop pos to grid cell
        Vector3Int troopGrid = GridManager.WorldPosFromCoordinates(troopPos);

        Vector3 targetWorld = new Vector3(nav.targetPosition.x, troopPos.y, nav.targetPosition.y);
        Gizmos.color = goalColor;
        Gizmos.DrawWireSphere(targetWorld, tileSize * 0.6f);

        for (int gx = troopGrid.x - visualRadius; gx <= troopGrid.x + visualRadius; gx++)
        {
            for (int gz = troopGrid.z - visualRadius; gz <= troopGrid.z + visualRadius; gz++)
            {
                if (gx < 0 || gx >= GridManager.GRID_WIDTH || gz < 0 || gz >= GridManager.GRID_HEIGHT)
                    continue;

                // grid cell center in world space
                Vector3 cellWorld = GridManager.CoordinatesToWorldPos(new Vector3(gx, 0, gz));
                cellWorld.y = troopPos.y + 0.15f;

                // sample direction from PathfindingManager
                Vector3 dir = PathfindingManager.Instance.GetSharedFlowFieldDirection(
                    cellWorld,
                    new Vector3(nav.targetPosition.x, 0, nav.targetPosition.y)
                );

                if (dir.sqrMagnitude < 0.001f) continue;

                Gizmos.color = flowFieldArrowColor;
                DrawArrow(cellWorld, new Vector3(dir.x, 0, dir.z) * arrowLength);
            }
        }

        // highlight troop's current cell
        Vector3 currentCell = GridManager.CoordinatesToWorldPos(new Vector3(troopGrid.x, 0, troopGrid.z));
        currentCell.y = troopPos.y + 0.2f;
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(currentCell, new Vector3(tileSize, 0.1f, tileSize));
    }
    
    private void DrawArrow(Vector3 from, Vector3 direction)
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
    
    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );
            
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}
