using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using Pathfinding.ECS;


public class PathfindingTestGeneration : MonoBehaviour
{
    [Header("Pathfinding Mode")]
    [SerializeField] private PathfindingMode pathfindingMode = PathfindingMode.FlowField;
    
    [Header("Test Configuration")]
    [SerializeField] private int numberOfTestUnits = 10;
    [SerializeField] private bool spawnOnStart = true;
    
    [Header("Spawn Area")]
    [SerializeField] private Vector3 spawnAreaCenter = Vector3.zero;
    [SerializeField] private float spawnAreaRadius = 20f;
    
    [Header("Test Controls")]
    [SerializeField] private bool useRandomTargets = true;
    [SerializeField] private Vector3 fixedTarget = new Vector3(50, 0, 50);
    [SerializeField] private float targetAreaRadius = 30f;
    
    [Header("Unit Stats")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float stoppingDistance = 1.5f;
    [SerializeField] private float slowdownDistance = 3f;

    // Unit tracking
    private List<GameObject> FlowFields = new List<GameObject>();
    private List<GameObject> testUnits = new List<GameObject>();
    private List<TroopMovement> flowFieldMovement = new List<TroopMovement>();
    private List<NavMeshTroopMovement> navMeshMovement = new List<NavMeshTroopMovement>();
    private List<Entity> ecsMovement = new List<Entity>();
    
    private PathfindingMode currentMode;
    private EntityManager entityManager;

    void Start()
    {
        currentMode = pathfindingMode;
        
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        if (spawnOnStart)
        {
            SpawnTestUnits();
        }
    }

    void Update()
    {
        if (pathfindingMode != currentMode)
        {
            SwitchPathfindingMode(pathfindingMode);
        }
        
        if (Input.GetKeyDown(KeyCode.Space)) AssignNewTargets();
        if (Input.GetKeyDown(KeyCode.R)) ResetTest();
        if (Input.GetKeyDown(KeyCode.C)) ClearUnits();
        if (Input.GetKeyDown(KeyCode.T)) TogglePathfindingMode();

        // sync entities's LocalTransform to GameObject
        if (currentMode == PathfindingMode.ECSSystem)
        {
            for (int i = 0; i < ecsMovement.Count && i < testUnits.Count; i++)
            {
                if (testUnits[i] != null && entityManager.Exists(ecsMovement[i]))
                {
                    var transform = entityManager.GetComponentData<LocalTransform>(ecsMovement[i]);
                    testUnits[i].transform.SetPositionAndRotation(transform.Position, transform.Rotation);
                }
            }
        }
        else if (currentMode == PathfindingMode.FlowField)
        {
            foreach (var movement in flowFieldMovement)
            {
                if (movement != null && movement.enabled)
                {
                    movement.UpdateMovement(Time.deltaTime);
                }
            }
        }
        else if (currentMode == PathfindingMode.UnityNavMesh)
        {
            foreach (var movement in navMeshMovement)
            {
                if (movement != null && movement.enabled)
                {
                    movement.UpdateMovement(Time.deltaTime);
                }
            }
        }
    }

    #region Unit Spawning
    [ContextMenu("Spawn Test Units")]
    public void SpawnTestUnits()
    {
        for (int i = 0; i < numberOfTestUnits; i++)
        {
            SpawnTestUnit(i);
        }
    }

    private void SpawnTestUnit(int index)
    {
        Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * spawnAreaRadius;
        Vector3 position = spawnAreaCenter + new Vector3(randomOffset.x, 1f, randomOffset.y);

        if (currentMode == PathfindingMode.ECSSystem)
        {
            SpawnECSUnit(index, position);
        }
        else if (currentMode == PathfindingMode.FlowField)
        {
            SpawnFlowFieldUnit(index, position);
        }
        else if (currentMode == PathfindingMode.UnityNavMesh)
        {
            SpawnNavMeshUnit(index, position);
        }
    }

    private void SpawnECSUnit(int index, Vector3 position)
    {
        if (entityManager == default)
        {
            return;
        }

        Entity entity = entityManager.CreateEntity();
        entityManager.AddComponent<TroopTag>(entity);
        entityManager.AddComponentData(entity, new LocalTransform
        {
            Position = position,
            Rotation = quaternion.identity,
            Scale = 1f
        });
        
        entityManager.AddComponentData(entity, new MovementData
        {
            maxSpeed = maxSpeed,
            acceleration = acceleration,
            velocity = float2.zero
        });
        
        entityManager.AddComponentData(entity, new NavigationTarget
        {
            stoppingDistance = stoppingDistance,
            slowdownDistance = slowdownDistance,
            isMoving = 0,
            reachedDestination = 1,
            useFlowField = 0,
            targetPosition = float2.zero
        });
        
        entityManager.AddComponentData(entity, new WaypointProgress
        {
            currentIndex = 0,
            totalCount = 0
        });
        
        entityManager.AddBuffer<WaypointElement>(entity);
        
        ecsMovement.Add(entity);
        
        // create visual GameObject associated with given entity
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.transform.position = position;
        visual.transform.localScale = new Vector3(2f, 2f, 2f);
        
        
        visual.name = $"ECSTroop_{index}";
        testUnits.Add(visual);
        
    }

    private void SpawnFlowFieldUnit(int index, Vector3 position)
    {
        GameObject unit = GameObject.CreatePrimitive(PrimitiveType.Cube);
        unit.transform.position = position;
        unit.transform.localScale = new Vector3(2f, 2f, 2f);
    

        unit.name = $"FlowFieldTroop_{index}";

        TroopMovement movement = unit.GetComponent<TroopMovement>();
        if (movement == null)
            movement = unit.AddComponent<TroopMovement>();

        TroopStats stats = new TroopStats
        {
            troopName = $"FlowFieldTroop_{index}",
            faction = TroopFaction.Player,
            maxSpeed = maxSpeed,
            acceleration = acceleration
        };

        movement.Initialize(stats);
        flowFieldMovement.Add(movement);
        testUnits.Add(unit);
    }

    private void SpawnNavMeshUnit(int index, Vector3 position)
    {
        GameObject unit;

        unit = GameObject.CreatePrimitive(PrimitiveType.Cube);
        unit.transform.position = position;
        unit.transform.localScale = new Vector3(2f, 2f, 2f);

        unit.name = $"NavMesh_{index}";

        var movement = unit.AddComponent<NavMeshTroopMovement>();
        TroopStats stats = new TroopStats
        {
            troopName = $"NavMesh_{index}",
            faction = TroopFaction.Player,
            maxSpeed = maxSpeed,
            acceleration = acceleration
        };
        movement.Initialize(stats);
        navMeshMovement.Add(movement);
        testUnits.Add(unit);
    }

    #endregion

    #region Target Assigning

    [ContextMenu("Assign New Targets")]
    public void AssignNewTargets()
    {
        if (testUnits.Count == 0)
        {
            return;
        }

        if (currentMode == PathfindingMode.ECSSystem)
        {
            AssignECSTargets();
        }
        else if (currentMode == PathfindingMode.FlowField)
        {
            AssignFlowFieldTargets();
        }
        else if (currentMode == PathfindingMode.UnityNavMesh)
        {
            AssignNavMeshTargets();
        }
    }

    private void AssignECSTargets()
    {
        
        if (ecsMovement.Count == 0)
        {
            return;
        }

        int successCount = 0;
        
        for (int i = 0; i < ecsMovement.Count; i++)
        {
            Entity entity = ecsMovement[i];
            
            if (!entityManager.Exists(entity))
            {
                continue;
            }
            
            Vector3 target = GetRandomTarget();
            var transform = entityManager.GetComponentData<LocalTransform>(entity);
            Vector3 startPos = transform.Position;
            
            // Capture for closure
            int capturedIndex = i;
            
            PathfindingManager.Instance.RequestPath(
                startPos,
                target,
                result =>
                {
                    if (result.success && capturedIndex < ecsMovement.Count)
                    {
                        Entity e = ecsMovement[capturedIndex];
                        
                        if (!entityManager.Exists(e))
                            return;
                        
                        // Set navigation target
                        NavigationTarget nav = entityManager.GetComponentData<NavigationTarget>(e);
                        nav.targetPosition = new float2(result.targetPosition.x, result.targetPosition.z);
                        nav.isMoving = 1;
                        nav.reachedDestination = 0;
                        entityManager.SetComponentData(e, nav);
                        
                        // Set waypoints
                        DynamicBuffer<WaypointElement> waypointBuffer = entityManager.GetBuffer<WaypointElement>(e);
                        waypointBuffer.Clear();
                        foreach (var wp in result.waypoints)
                        {
                            waypointBuffer.Add(new WaypointElement { position = wp });
                        }
                        
                        // Reset progress
                        WaypointProgress progress = entityManager.GetComponentData<WaypointProgress>(e);
                        progress.currentIndex = 0;
                        progress.totalCount = result.waypoints.Count;
                        entityManager.SetComponentData(e, progress);

                        Vector3Int targetGrid = GridManager.WorldPosFromCoordinates(result.targetPosition);
                        CreateFlowFieldRequest(new int2(targetGrid.x, targetGrid.z));
                        
                    }
                }
            );
            
            successCount++;
        }
        
    }

    private void AssignFlowFieldTargets()
    {
        foreach (var movement in flowFieldMovement)
        {
            if (movement != null)
            {
                Vector3 target = GetRandomTarget();
                PathfindingManager.Instance.RequestPath(
                    movement.transform.position,
                    target,
                    result =>
                    {
                        if (result.success)
                        {
                            movement.SetTarget(result.targetPosition);
                        }
                    }
                );
            }
        }
    }

    private void AssignNavMeshTargets()
    {
        foreach (var movement in navMeshMovement)
        {
            if (movement != null)
            {
                Vector3 target = GetRandomTarget();
                movement.SetTarget(target);
            }
        }
    }

    #endregion
    private void CreateFlowFieldRequest(int2 targetGrid)
    {
        // Check if request already exists
        var query = entityManager.CreateEntityQuery(typeof(FlowFieldRequest));
        var requestEntities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
        
        foreach (var reqEntity in requestEntities)
        {
            var existingReq = entityManager.GetComponentData<FlowFieldRequest>(reqEntity);
            if (existingReq.targetGrid.x == targetGrid.x && existingReq.targetGrid.y == targetGrid.y)
            {
                // Already exists, don't create duplicate
                requestEntities.Dispose();
                return;
            }
        }
        requestEntities.Dispose();
        
        // Create new request entity
        Entity requestEntity = entityManager.CreateEntity();
        entityManager.AddComponentData(requestEntity, new FlowFieldRequest
        {
            targetGrid = targetGrid,
            isProcessing = 0  // Pending
        });
        
        Debug.Log($"✓ Created flow field request for grid {targetGrid}");
    }

    

    [ContextMenu("Stop All Units")]
    public void StopAllUnits()
    {
        if (currentMode == PathfindingMode.ECSSystem)
        {
            foreach (var entity in ecsMovement)
            {
                if (!entityManager.Exists(entity)) continue;
                
                var nav = entityManager.GetComponentData<NavigationTarget>(entity);
                nav.isMoving = 0;
                nav.reachedDestination = 1;
                entityManager.SetComponentData(entity, nav);
                
                var movement = entityManager.GetComponentData<MovementData>(entity);
                movement.velocity = float2.zero;
                entityManager.SetComponentData(entity, movement);
            }
        }
        else if (currentMode == PathfindingMode.FlowField)
        {
            foreach (var movement in flowFieldMovement)
            {
                if (movement != null) movement.StopMoving();
            }
        }
        else if (currentMode == PathfindingMode.UnityNavMesh)
        {
            foreach (var movement in navMeshMovement)
            {
                if (movement != null) movement.StopMoving();
            }
        }
    }

    [ContextMenu("Clear Units")]
    public void ClearUnits()
    {
        // Destroy ECS entities
        foreach (var entity in ecsMovement)
        {
            if (entityManager.Exists(entity))
            {
                entityManager.DestroyEntity(entity);
            }
        }
        
        // Destroy GameObjects
        foreach (var unit in testUnits)
        {
            if (unit != null) Destroy(unit);
        }
        
        testUnits.Clear();
        flowFieldMovement.Clear();
        navMeshMovement.Clear();
        ecsMovement.Clear();
    }

    [ContextMenu("Reset Test")]
    public void ResetTest()
    {
        ClearUnits();
        SpawnTestUnits();
    }

    [ContextMenu("Toggle Pathfinding Mode")]
    public void TogglePathfindingMode()
    {
        PathfindingMode newMode = currentMode == PathfindingMode.FlowField ? PathfindingMode.UnityNavMesh :
                                   currentMode == PathfindingMode.UnityNavMesh ? PathfindingMode.ECSSystem :
                                   PathfindingMode.FlowField;
        SwitchPathfindingMode(newMode);
    }

    private void SwitchPathfindingMode(PathfindingMode newMode)
    {
        ClearUnits();
        currentMode = newMode;
        pathfindingMode = newMode;
        SpawnTestUnits();
    }

    private Vector3 GetRandomTarget()
    {
        if (useRandomTargets)
        {
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * targetAreaRadius;
            return fixedTarget + new Vector3(randomOffset.x, 0, randomOffset.y);
        }
        return fixedTarget;
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 450, 700));
        GUILayout.Label("=== Pathfinding Test ===");
        GUILayout.Label($"Mode: {currentMode}");
        GUILayout.Label($"Units: {FlowFields.Count}");
        
        if (currentMode == PathfindingMode.ECSSystem)
        {
            GUILayout.Label($"ECS Entities: {ecsMovement.Count}");
            
            int moving = 0;
            int existing = 0;
            int usingFlowField = 0;
            int usingWaypoints = 0;
            
            foreach (var entity in ecsMovement)
            {
                if (entityManager.Exists(entity))
                {
                    existing++;
                    var nav = entityManager.GetComponentData<NavigationTarget>(entity);
                    if (nav.isMoving == 1)
                    {
                        moving++;
                        if (nav.useFlowField == 1)
                            usingFlowField++;
                        else
                            usingWaypoints++;
                    }
                }
            }
            
            GUILayout.Label($"Existing: {existing}, Moving: {moving}");
            GUILayout.Label($"Using Waypoints: {usingWaypoints}");
            GUILayout.Label($"Using Flow Field: {usingFlowField}");
            
            // Show detailed info for first entity
            if (ecsMovement.Count > 0 && entityManager.Exists(ecsMovement[0]))
            {
                GUILayout.Label("");
                GUILayout.Label("--- First Entity Debug ---");
                
                var nav = entityManager.GetComponentData<NavigationTarget>(ecsMovement[0]);
                var transform = entityManager.GetComponentData<LocalTransform>(ecsMovement[0]);
                var progress = entityManager.GetComponentData<WaypointProgress>(ecsMovement[0]);
                var movement = entityManager.GetComponentData<MovementData>(ecsMovement[0]);
                
                Vector2 currentPos = new Vector2(transform.Position.x, transform.Position.z);
                Vector2 targetPos = new Vector2(nav.targetPosition.x, nav.targetPosition.y);
                float distToTarget = Vector2.Distance(currentPos, targetPos);
                
                GUILayout.Label($"Position: ({transform.Position.x:F1}, {transform.Position.z:F1})");
                GUILayout.Label($"Target: ({nav.targetPosition.x:F1}, {nav.targetPosition.y:F1})");
                GUILayout.Label($"Distance to Target: {distToTarget:F1}");
                GUILayout.Label($"Mode: {(nav.useFlowField == 1 ? "FLOW FIELD" : "WAYPOINTS")}");
                GUILayout.Label($"Waypoint: {progress.currentIndex}/{progress.totalCount}");
                GUILayout.Label($"Velocity: ({movement.velocity.x:F2}, {movement.velocity.y:F2})");
                GUILayout.Label($"Speed: {math.length(movement.velocity):F2}/{movement.maxSpeed:F2}");
                
                // Show when transition should happen
                float activationDist = PathfindingManager.Instance != null ? 
                    PathfindingManager.Instance.FlowFieldActivationDistance : 30f;
                GUILayout.Label($"Flow Field Activates at: {activationDist:F1}");
                
                if (distToTarget <= activationDist && nav.useFlowField == 0)
                {
                    GUILayout.Label("⚠ Should be using flow field!");
                }
            }
        }
        
        GUILayout.Label("");
        GUILayout.Label("Controls:");
        GUILayout.Label("SPACE - Assign targets");
        GUILayout.Label("T - Toggle mode");
        GUILayout.Label("R - Reset");
        GUILayout.Label("C - Clear");
        
        GUILayout.EndArea();
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        DrawCircle(spawnAreaCenter, spawnAreaRadius, 32);
        
        Gizmos.color = Color.red;
        DrawCircle(fixedTarget, targetAreaRadius, 32);
        
        if (!Application.isPlaying || entityManager == default) return;
        
        if (currentMode == PathfindingMode.ECSSystem)
        {
            // Draw activation radius for flow fields
            float activationDist = PathfindingManager.Instance != null ? 
                PathfindingManager.Instance.FlowFieldActivationDistance : 30f;
            
            foreach (var entity in ecsMovement)
            {
                if (!entityManager.Exists(entity)) continue;
                
                var transform = entityManager.GetComponentData<LocalTransform>(entity);
                var nav = entityManager.GetComponentData<NavigationTarget>(entity);
                
                if (nav.isMoving == 1)
                {
                    Vector3 start = transform.Position;
                    Vector3 end = new Vector3(nav.targetPosition.x, start.y, nav.targetPosition.y);
                    
                    // Draw line to target
                    if (nav.useFlowField == 1)
                    {
                        Gizmos.color = Color.cyan; // Cyan = using flow field
                    }
                    else
                    {
                        Gizmos.color = Color.magenta; // Magenta = using waypoints
                    }
                    Gizmos.DrawLine(start, end);
                    
                    // Draw activation radius around target
                    Gizmos.color = new Color(1, 1, 0, 0.3f);
                    DrawCircle(end, activationDist, 16);
                    
                    // Draw velocity vector
                    var movement = entityManager.GetComponentData<MovementData>(entity);
                    if (math.lengthsq(movement.velocity) > 0.01f)
                    {
                        Gizmos.color = Color.green;
                        Vector3 velDir = new Vector3(movement.velocity.x, 0, movement.velocity.y);
                        Gizmos.DrawRay(start, velDir * 3f);
                    }
                    
                    // Draw waypoints if using waypoint navigation
                    if (nav.useFlowField == 0)
                    {
                        var waypoints = entityManager.GetBuffer<WaypointElement>(entity);
                        var progress = entityManager.GetComponentData<WaypointProgress>(entity);
                        
                        Gizmos.color = Color.yellow;
                        for (int i = progress.currentIndex; i < waypoints.Length && i < progress.totalCount; i++)
                        {
                            Vector3 wpPos = waypoints[i].position;
                            Gizmos.DrawWireSphere(wpPos, 1f);
                            
                            // Draw line to next waypoint
                            if (i < waypoints.Length - 1 && i + 1 < progress.totalCount)
                            {
                                Gizmos.DrawLine(wpPos, waypoints[i + 1].position);
                            }
                        }
                    }
                }
            }
        }
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

// ═══════════════════════════════════════════════════════════════════
// ENUMS
// ═══════════════════════════════════════════════════════════════════

public enum PathfindingMode
{
    FlowField,  // Your custom MonoBehaviour pathfinding
    UnityNavMesh,     // Unity's built-in NavMesh
    ECSSystem          // DOTS/ECS high-performance pathfinding
}

// ═══════════════════════════════════════════════════════════════════
// NAVMESH TROOP MOVEMENT (for comparison)
// ═══════════════════════════════════════════════════════════════════

public class NavMeshTroopMovement : MonoBehaviour
{
    private TroopStats stats;
    private NavMeshAgent agent;
    private Vector3 targetPosition;
    private bool isMoving = false;
    
    public void Initialize(TroopStats troopStats)
    {
        stats = troopStats;
        
        agent = gameObject.AddComponent<NavMeshAgent>();
        agent.speed = stats.maxSpeed;
        agent.acceleration = stats.acceleration;
        agent.angularSpeed = 360f;
        agent.stoppingDistance = 1.5f;
        agent.autoBraking = true;
    }
    
    public void SetTarget(Vector3 destination)
    {
        if (agent == null) return;
        
        targetPosition = destination;
        isMoving = true;
        agent.SetDestination(destination);
    }
    
    public void StopMoving()
    {
        if (agent == null) return;
        
        isMoving = false;
        agent.ResetPath();
    }
    
    public void UpdateMovement(float deltaTime)
    {
        if (!isMoving || agent == null) return;
        
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                StopMoving();
            }
        }
    }
    
    public bool IsMoving() => isMoving;
    public Vector3 GetCurrentDestination() => targetPosition;
    
    private void OnDrawGizmosSelected()
    {
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.cyan;
            Vector3[] corners = agent.path.corners;
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(targetPosition, 0.5f);
            
            Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
            Gizmos.DrawWireSphere(targetPosition, agent.stoppingDistance);
        }
    }
}