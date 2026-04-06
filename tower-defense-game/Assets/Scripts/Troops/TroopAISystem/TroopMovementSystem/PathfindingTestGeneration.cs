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
    [SerializeField] private PathfindingMode pathfindingMode = PathfindingMode.Custom3Tier;
    
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
    
    [Header("Group Command Test")]
    [Tooltip("When true, all units get the same target (tests shared flow field tier)")]
    [SerializeField] private bool assignGroupTarget = false;
    
    [Header("Unit Stats")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float stoppingDistance = 1.5f;
    [SerializeField] private float slowdownDistance = 3f;

    // Unit tracking
    private List<GameObject> testUnits = new List<GameObject>();
    private List<TroopMovement> customMovement = new List<TroopMovement>();
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
        if (Input.GetKeyDown(KeyCode.G)) AssignGroupTarget();   // shared flow field test
        if (Input.GetKeyDown(KeyCode.R)) ResetTest();
        if (Input.GetKeyDown(KeyCode.C)) ClearUnits();
        if (Input.GetKeyDown(KeyCode.T)) TogglePathfindingMode();

        // sync ECS entity LocalTransform → visual GameObject
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
        else if (currentMode == PathfindingMode.Custom3Tier)
        {
            if (ORCAManager.Instance != null)
                ORCAManager.Instance.Prepare();

            foreach (var movement in customMovement)
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
        else if (currentMode == PathfindingMode.Custom3Tier)
        {
            SpawnCustomUnit(index, position);
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
            navigationMode = 0,  // DirectSteer
            targetPosition = float2.zero
        });
        
        entityManager.AddComponentData(entity, new WaypointProgress
        {
            currentIndex = 0,
            totalCount = 0
        });
        
        entityManager.AddBuffer<WaypointElement>(entity);
        
        entityManager.AddComponentData(entity, new AvoidanceAgent
        {
            radius = 0.9f
        });
        
        ecsMovement.Add(entity);
        
        // create visual GameObject associated with given entity
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.transform.position = position;
        visual.transform.localScale = new Vector3(2f, 2f, 2f);
        
        
        visual.name = $"ECSTroop_{index}";
        testUnits.Add(visual);
        
    }

    private void SpawnCustomUnit(int index, Vector3 position)
    {
        GameObject unit = GameObject.CreatePrimitive(PrimitiveType.Cube);
        unit.transform.position = position;
        unit.transform.localScale = new Vector3(2f, 2f, 2f);
    
        unit.name = $"CustomTroop_{index}";

        TroopMovement movement = unit.GetComponent<TroopMovement>();
        if (movement == null)
            movement = unit.AddComponent<TroopMovement>();

        TroopStats stats = new TroopStats
        {
            troopName = $"CustomTroop_{index}",
            faction = TroopFaction.Player,
            maxSpeed = maxSpeed,
            acceleration = acceleration
        };

        movement.Initialize(stats);
        customMovement.Add(movement);
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
        if (testUnits.Count == 0) return;

        if (currentMode == PathfindingMode.ECSSystem)
        {
            AssignECSTargets();
        }
        else if (currentMode == PathfindingMode.Custom3Tier)
        {
            AssignCustomTargets();
        }
        else if (currentMode == PathfindingMode.UnityNavMesh)
        {
            AssignNavMeshTargets();
        }
    }

    /// <summary>
    /// Assign all units the same target — exercises shared flow field tier.
    /// Press G at runtime.
    /// </summary>
    [ContextMenu("Assign Group Target (Flow Field)")]
    public void AssignGroupTarget()
    {
        if (testUnits.Count == 0) return;

        Vector3 target = GetTarget();

        if (currentMode == PathfindingMode.Custom3Tier)
        {
            // create shared flow field, then send all troops to it
            PathfindingManager.Instance.RequestSharedFlowField(target);

            foreach (var movement in customMovement)
            {
                if (movement != null)
                {
                    movement.SetTargetWithFlowField(target);
                }
            }

            Debug.Log($"[Test] Group command → {customMovement.Count} troops using shared flow field to {target}");
        }
        else if (currentMode == PathfindingMode.ECSSystem)
        {
            AssignECSTargetsShared(target);
        }
        else if (currentMode == PathfindingMode.UnityNavMesh)
        {
            foreach (var movement in navMeshMovement)
            {
                if (movement != null) movement.SetTarget(target);
            }
        }
    }

    private void AssignECSTargets()
    {
        if (ecsMovement.Count == 0) return;

        for (int i = 0; i < ecsMovement.Count; i++)
        {
            Entity entity = ecsMovement[i];
            if (!entityManager.Exists(entity)) continue;
            
            Vector3 target = GetTarget();
            var transform = entityManager.GetComponentData<LocalTransform>(entity);
            int capturedIndex = i;
            
            // use PathfindingManager's 3-tier decision
            NavigationMode mode = PathfindingManager.Instance.GetNavigationMode(transform.Position, target);
            
            if (mode == NavigationMode.DirectSteer)
            {
                // direct steer — no path needed
                NavigationTarget nav = entityManager.GetComponentData<NavigationTarget>(entity);
                nav.targetPosition = new float2(target.x, target.z);
                nav.isMoving = 1;
                nav.reachedDestination = 0;
                nav.navigationMode = 0; // DirectSteer
                entityManager.SetComponentData(entity, nav);
                
                WaypointProgress progress = entityManager.GetComponentData<WaypointProgress>(entity);
                progress.currentIndex = 0;
                progress.totalCount = 0;
                entityManager.SetComponentData(entity, progress);
            }
            else
            {
                // A* waypoints
                PathfindingManager.Instance.RequestPath(
                    transform.Position,
                    target,
                    result =>
                    {
                        if (result.success && capturedIndex < ecsMovement.Count)
                        {
                            Entity e = ecsMovement[capturedIndex];
                            if (!entityManager.Exists(e)) return;
                            
                            NavigationTarget nav = entityManager.GetComponentData<NavigationTarget>(e);
                            nav.targetPosition = new float2(result.targetPosition.x, result.targetPosition.z);
                            nav.isMoving = 1;
                            nav.reachedDestination = 0;
                            nav.navigationMode = 1; // AStarWaypoints
                            entityManager.SetComponentData(e, nav);
                            
                            DynamicBuffer<WaypointElement> waypointBuffer = entityManager.GetBuffer<WaypointElement>(e);
                            waypointBuffer.Clear();
                            foreach (var wp in result.waypoints)
                            {
                                waypointBuffer.Add(new WaypointElement { position = wp });
                            }
                            
                            WaypointProgress progress = entityManager.GetComponentData<WaypointProgress>(e);
                            progress.currentIndex = 0;
                            progress.totalCount = result.waypoints.Count;
                            entityManager.SetComponentData(e, progress);
                        }
                    }
                );
            }
        }
    }

    private void AssignECSTargetsShared(Vector3 target)
    {
        // create shared flow field via PathfindingManager (3-tier system)
        PathfindingManager.Instance.RequestSharedFlowField(target);

        foreach (var entity in ecsMovement)
        {
            if (!entityManager.Exists(entity)) continue;

            NavigationTarget nav = entityManager.GetComponentData<NavigationTarget>(entity);
            nav.targetPosition = new float2(target.x, target.z);
            nav.isMoving = 1;
            nav.reachedDestination = 0;
            nav.navigationMode = 2; // SharedFlowField
            entityManager.SetComponentData(entity, nav);
        }
    }

    /// <summary>
    /// Individual targets — each troop picks its own nav mode via SetTarget()
    /// (DirectSteer if close, A* if far).
    /// </summary>
    private void AssignCustomTargets()
    {
        foreach (var movement in customMovement)
        {
            if (movement != null)
            {
                Vector3 target = GetTarget();
                movement.SetTarget(target);
            }
        }
    }

    private void AssignNavMeshTargets()
    {
        foreach (var movement in navMeshMovement)
        {
            if (movement != null)
            {
                Vector3 target = GetTarget();
                movement.SetTarget(target);
            }
        }
    }

    #endregion

    

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
                nav.navigationMode = 0;
                entityManager.SetComponentData(entity, nav);
                
                var movement = entityManager.GetComponentData<MovementData>(entity);
                movement.velocity = float2.zero;
                entityManager.SetComponentData(entity, movement);
            }
        }
        else if (currentMode == PathfindingMode.Custom3Tier)
        {
            foreach (var movement in customMovement)
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
        customMovement.Clear();
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
        PathfindingMode newMode = currentMode == PathfindingMode.Custom3Tier ? PathfindingMode.UnityNavMesh :
                                   currentMode == PathfindingMode.UnityNavMesh ? PathfindingMode.ECSSystem :
                                   PathfindingMode.Custom3Tier;
        SwitchPathfindingMode(newMode);
    }

    private void SwitchPathfindingMode(PathfindingMode newMode)
    {
        ClearUnits();
        currentMode = newMode;
        pathfindingMode = newMode;
        SpawnTestUnits();
    }

    private Vector3 GetTarget()
    {
        if (useRandomTargets && !assignGroupTarget)
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
        GUILayout.Label($"Units: {testUnits.Count}");
        
        if (currentMode == PathfindingMode.Custom3Tier)
        {
            int moving = 0;
            int directSteer = 0;
            int astar = 0;
            int flowField = 0;

            foreach (var movement in customMovement)
            {
                if (movement != null && movement.IsMoving())
                {
                    moving++;
                    switch (movement.GetNavigationMode())
                    {
                        case NavigationMode.DirectSteer: directSteer++; break;
                        case NavigationMode.AStarWaypoints: astar++; break;
                        case NavigationMode.SharedFlowField: flowField++; break;
                    }
                }
            }

            GUILayout.Label($"Moving: {moving} / {customMovement.Count}");
            GUILayout.Label($"  Direct Steer: {directSteer}");
            GUILayout.Label($"  A* Waypoints: {astar}");
            GUILayout.Label($"  Shared Flow Field: {flowField}");

            if (PathfindingManager.Instance != null)
            {
                GUILayout.Space(5);
                GUILayout.Label($"Direct Steer Distance: {PathfindingManager.Instance.DirectSteerDistance:F1}");
                GUILayout.Label($"Cached Flow Fields: {PathfindingManager.Instance.GetCachedFlowFieldCount()}");
                GUILayout.Label($"Queued Path Requests: {PathfindingManager.Instance.GetQueuedPathRequests()}");
            }
        }
        else if (currentMode == PathfindingMode.ECSSystem)
        {
            GUILayout.Label($"ECS Entities: {ecsMovement.Count}");
            
            int moving = 0;
            int existing = 0;
            int directSteerECS = 0;
            int astarECS = 0;
            int flowFieldECS = 0;
            
            foreach (var entity in ecsMovement)
            {
                if (entityManager.Exists(entity))
                {
                    existing++;
                    var nav = entityManager.GetComponentData<NavigationTarget>(entity);
                    if (nav.isMoving == 1)
                    {
                        moving++;
                        switch (nav.navigationMode)
                        {
                            case 0: directSteerECS++; break;
                            case 1: astarECS++; break;
                            case 2: flowFieldECS++; break;
                        }
                    }
                }
            }
            
            GUILayout.Label($"Existing: {existing}, Moving: {moving}");
            GUILayout.Label($"  Direct Steer: {directSteerECS}");
            GUILayout.Label($"  A* Waypoints: {astarECS}");
            GUILayout.Label($"  Shared Flow Field: {flowFieldECS}");
            
            // detailed info for first entity
            if (ecsMovement.Count > 0 && entityManager.Exists(ecsMovement[0]))
            {
                GUILayout.Label("");
                GUILayout.Label("--- First Entity Debug ---");
                
                var nav = entityManager.GetComponentData<NavigationTarget>(ecsMovement[0]);
                var ecsTransform = entityManager.GetComponentData<LocalTransform>(ecsMovement[0]);
                var progress = entityManager.GetComponentData<WaypointProgress>(ecsMovement[0]);
                var movementData = entityManager.GetComponentData<MovementData>(ecsMovement[0]);
                
                Vector2 currentPos = new Vector2(ecsTransform.Position.x, ecsTransform.Position.z);
                Vector2 targetPos = new Vector2(nav.targetPosition.x, nav.targetPosition.y);
                float distToTarget = Vector2.Distance(currentPos, targetPos);
                
                GUILayout.Label($"Position: ({ecsTransform.Position.x:F1}, {ecsTransform.Position.z:F1})");
                GUILayout.Label($"Target: ({nav.targetPosition.x:F1}, {nav.targetPosition.y:F1})");
                GUILayout.Label($"Distance: {distToTarget:F1}");
                string modeName = nav.navigationMode switch
                {
                    0 => "DIRECT STEER",
                    1 => "A* WAYPOINTS",
                    2 => "SHARED FLOW FIELD",
                    _ => "UNKNOWN"
                };
                GUILayout.Label($"Mode: {modeName}");
                GUILayout.Label($"Waypoint: {progress.currentIndex}/{progress.totalCount}");
                GUILayout.Label($"Speed: {math.length(movementData.velocity):F2}/{movementData.maxSpeed:F2}");
            }
        }
        
        GUILayout.Label("");
        GUILayout.Label("Controls:");
        GUILayout.Label("SPACE - Assign individual targets");
        GUILayout.Label("G - Group target (shared flow field)");
        GUILayout.Label("T - Toggle mode");
        GUILayout.Label("R - Reset");
        GUILayout.Label("C - Clear");
        
        GUILayout.EndArea();
    }
    


}

// ═══════════════════════════════════════════════════════════════════
// ENUMS
// ═══════════════════════════════════════════════════════════════════

public enum PathfindingMode
{
    Custom3Tier,       // 3-tier: DirectSteer / A* / SharedFlowField
    UnityNavMesh,      // Unity's built-in NavMesh (comparison baseline)
    ECSSystem          // DOTS/ECS pathfinding pipeline
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
    
 
}