using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;
using System.Collections.Generic;

/// <summary>
/// Test script for pathfinding system - spawns test units and gives them movement commands
/// Supports both custom pathfinding and Unity NavMesh for comparison
/// Attach to an empty GameObject in your scene
/// </summary>
public class TestPathfinding : MonoBehaviour
{
    [Header("Pathfinding Mode")]
    [SerializeField] private PathfindingMode pathfindingMode = PathfindingMode.CustomFlowField;
    
    [Header("Test Configuration")]
    [SerializeField] private int numberOfTestUnits = 10;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private GameObject testUnitPrefab; // Optional: assign a prefab, or will create cubes
    
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
    
    private List<GameObject> testUnits = new List<GameObject>();
    private List<TroopMovement> customMovements = new List<TroopMovement>();
    private List<NavMeshTroopMovement> navMeshMovements = new List<NavMeshTroopMovement>();
    
    private PathfindingMode currentMode;

    void Start()
    {
        currentMode = pathfindingMode;
        
        if (spawnOnStart)
        {
            SpawnTestUnits();
        }
    }

    void Update()
    {
        // Check if mode changed
        if (pathfindingMode != currentMode)
        {
            SwitchPathfindingMode(pathfindingMode);
        }
        
        // Keyboard controls for testing
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AssignNewTargets();
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            StopAllUnits();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetTest();
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearUnits();
        }
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            TogglePathfindingMode();
        }

        // Update all troop movements based on current mode
        if (currentMode == PathfindingMode.CustomFlowField)
        {
            foreach (TroopMovement movement in customMovements)
            {
                if (movement != null && movement.enabled)
                {
                    movement.UpdateMovement(Time.deltaTime);
                }
            }
        }
        else
        {
            foreach (NavMeshTroopMovement movement in navMeshMovements)
            {
                if (movement != null && movement.enabled)
                {
                    movement.UpdateMovement(Time.deltaTime);
                }
            }
        }
    }

    [ContextMenu("Spawn Test Units")]
    public void SpawnTestUnits()
    {
        for (int i = 0; i < numberOfTestUnits; i++)
        {
            SpawnTestUnit(i);
        }
        
        Debug.Log($"Spawned {numberOfTestUnits} test units using {currentMode} pathfinding. Press SPACE to assign targets.");
    }

    private void SpawnTestUnit(int index)
    {
        // Create unit GameObject
        GameObject unit;
        
        if (testUnitPrefab != null)
        {
            unit = Instantiate(testUnitPrefab);
        }
        else
        {
            // Create a simple cube as test unit
            unit = GameObject.CreatePrimitive(PrimitiveType.Cube);
            unit.transform.localScale = new Vector3(2f, 2f, 2f);
            
            // Add a colored material
            Renderer renderer = unit.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = GetColorForIndex(index);
            renderer.material = mat;
        }
        
        unit.name = $"TestUnit_{index}_{currentMode}";
        
        // Random position in spawn area
        Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * spawnAreaRadius;
        Vector3 spawnPosition = spawnAreaCenter + new Vector3(randomOffset.x, 1f, randomOffset.y);
        unit.transform.position = spawnPosition;
        
        // Create stats
        TroopStats stats = new TroopStats
        {
            troopName = $"TestUnit_{index}",
            faction = TroopFaction.Player,
            maxSpeed = maxSpeed,
            acceleration = acceleration
        };
        
        // Add appropriate movement component based on mode
        if (currentMode == PathfindingMode.CustomFlowField)
        {
            TroopMovement movement = unit.AddComponent<TroopMovement>();
            movement.Initialize(stats);
            customMovements.Add(movement);
        }
        else
        {
            NavMeshTroopMovement movement = unit.AddComponent<NavMeshTroopMovement>();
            movement.Initialize(stats);
            navMeshMovements.Add(movement);
        }
        
        testUnits.Add(unit);
        if (FactionManager.Instance != null)
        {
            FactionManager.Instance.RegisterTroop(unit.transform, TroopFaction.Player);
        }

    }

    [ContextMenu("Toggle Pathfinding Mode")]
    public void TogglePathfindingMode()
    {
        PathfindingMode newMode = currentMode == PathfindingMode.CustomFlowField 
            ? PathfindingMode.UnityNavMesh 
            : PathfindingMode.CustomFlowField;
        
        SwitchPathfindingMode(newMode);
    }

    private void SwitchPathfindingMode(PathfindingMode newMode)
    {
        Debug.Log($"Switching from {currentMode} to {newMode}");
        
        // Store current positions
        List<Vector3> positions = new List<Vector3>();
        foreach (GameObject unit in testUnits)
        {
            if (unit != null)
            {
                positions.Add(unit.transform.position);
            }
        }
        
        // Clear existing units
        ClearUnits();
        
        // Update mode
        currentMode = newMode;
        pathfindingMode = newMode;
        
        // Respawn with new mode
        SpawnTestUnits();
        
        // Restore positions
        for (int i = 0; i < Mathf.Min(positions.Count, testUnits.Count); i++)
        {
            if (testUnits[i] != null)
            {
                testUnits[i].transform.position = positions[i];
            }
        }
    }

    [ContextMenu("Assign New Targets")]
    public void AssignNewTargets()
    {
        if (testUnits.Count == 0)
        {
            Debug.LogWarning("No test units spawned yet!");
            return;
        }

        if (currentMode == PathfindingMode.CustomFlowField)
        {
            foreach (TroopMovement movement in customMovements)
            {
                if (movement != null)
                {
                    Vector3 target = GetRandomTarget();
                    movement.SetTarget(target);
                }
            }
        }
        else
        {
            foreach (NavMeshTroopMovement movement in navMeshMovements)
            {
                if (movement != null)
                {
                    Vector3 target = GetRandomTarget();
                    movement.SetTarget(target);
                }
            }
        }
        
        Debug.Log($"Assigned new targets to {testUnits.Count} units.");
    }

    [ContextMenu("Assign Same Target")]
    public void AssignSameTarget()
    {
        if (testUnits.Count == 0)
        {
            Debug.LogWarning("No test units spawned yet!");
            return;
        }

        Vector3 target = useRandomTargets ? GetRandomTarget() : fixedTarget;
        
        if (currentMode == PathfindingMode.CustomFlowField)
        {
            foreach (TroopMovement movement in customMovements)
            {
                if (movement != null)
                {
                    movement.SetTarget(target);
                }
            }
        }
        else
        {
            foreach (NavMeshTroopMovement movement in navMeshMovements)
            {
                if (movement != null)
                {
                    movement.SetTarget(target);
                }
            }
        }
        
        Debug.Log($"All {testUnits.Count} units moving to {target}");
    }

    [ContextMenu("Stop All Units")]
    public void StopAllUnits()
    {
        if (currentMode == PathfindingMode.CustomFlowField)
        {
            foreach (TroopMovement movement in customMovements)
            {
                if (movement != null)
                {
                    movement.StopMoving();
                }
            }
        }
        else
        {
            foreach (NavMeshTroopMovement movement in navMeshMovements)
            {
                if (movement != null)
                {
                    movement.StopMoving();
                }
            }
        }
        
        Debug.Log("Stopped all units.");
    }

    [ContextMenu("Clear Units")]
    public void ClearUnits()
    {
        foreach (GameObject unit in testUnits)
        {
            if (unit != null)
            {
                Destroy(unit);
            }
        }
        
        testUnits.Clear();
        customMovements.Clear();
        navMeshMovements.Clear();
        
        Debug.Log("Cleared all test units.");
    }

    [ContextMenu("Reset Test")]
    public void ResetTest()
    {
        ClearUnits();
        SpawnTestUnits();
        AssignNewTargets();
    }

    private Vector3 GetRandomTarget()
    {
        if (useRandomTargets)
        {
            // Random position on the grid
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * targetAreaRadius;
            return fixedTarget + new Vector3(randomOffset.x, 0, randomOffset.y);
        }
        else
        {
            return fixedTarget;
        }
    }

    private Color GetColorForIndex(int index)
    {
        // Generate distinct colors for each unit
        float hue = (index * 0.618033988749895f) % 1f; // Golden ratio for nice distribution
        return Color.HSVToRGB(hue, 0.8f, 0.9f);
    }

    void OnDrawGizmos()
    {
        // Draw spawn area
        Gizmos.color = Color.green;
        DrawCircle(spawnAreaCenter, spawnAreaRadius, 32);
        
        // Draw target area
        Gizmos.color = Color.red;
        DrawCircle(fixedTarget, targetAreaRadius, 32);
        
        // Draw line from each unit to its target
        if (Application.isPlaying && testUnits != null)
        {
            if (currentMode == PathfindingMode.CustomFlowField)
            {
                foreach (TroopMovement movement in customMovements)
                {
                    if (movement != null && movement.IsMoving())
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(movement.transform.position, movement.GetCurrentDestination());
                    }
                }
            }
            else
            {
                foreach (NavMeshTroopMovement movement in navMeshMovements)
                {
                    if (movement != null && movement.IsMoving())
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawLine(movement.transform.position, movement.GetCurrentDestination());
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

    void OnGUI()
    {
        // Display instructions and debug info
        GUILayout.BeginArea(new Rect(10, 10, 400, 400));
        GUILayout.Label("=== Pathfinding Test Controls ===");
        GUILayout.Label($"Mode: {currentMode}");
        GUILayout.Label($"Units: {testUnits.Count}");
        GUILayout.Label("");
        GUILayout.Label("SPACE - Assign random targets");
        GUILayout.Label("T - Toggle pathfinding mode");
        GUILayout.Label("S - Stop all units");
        GUILayout.Label("R - Reset test");
        GUILayout.Label("C - Clear units");
        GUILayout.Label("");
        
        if (currentMode == PathfindingMode.CustomFlowField)
        {
            if (PathfindingManager.Instance != null)
            {
                GUILayout.Label("=== Custom Pathfinding Stats ===");
                GUILayout.Label($"Cached Flow Fields: {PathfindingManager.Instance.GetCachedFlowFieldCount()}");
                GUILayout.Label($"Queued Requests: {PathfindingManager.Instance.GetQueuedPathRequests()}");
                GUILayout.Label($"Coverage Tiles: {PathfindingManager.Instance.GetCoverageTileCount()}");
            }
        }
        else
        {
            GUILayout.Label("=== Unity NavMesh Stats ===");
            GUILayout.Label("Using Unity's built-in NavMesh");
            GUILayout.Label("No custom pathfinding overhead");
        }
        
        GUILayout.EndArea();
    }
}

public enum PathfindingMode
{
    CustomFlowField,
    UnityNavMesh
}

/// <summary>
/// NavMesh-based movement for comparison with custom pathfinding
/// </summary>
public class NavMeshTroopMovement : MonoBehaviour
{
    private TroopStats stats;
    private NavMeshAgent agent;
    private Vector3 targetPosition;
    private bool isMoving = false;
    
    public void Initialize(TroopStats troopStats)
    {
        stats = troopStats;
        
        // Add and configure NavMeshAgent
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
        
        // Check if reached destination
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
            // Draw NavMesh path
            Gizmos.color = Color.cyan;
            Vector3[] corners = agent.path.corners;
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }
            
            // Draw target
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(targetPosition, 0.5f);
            
            // Draw stopping distance
            Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
            Gizmos.DrawWireSphere(targetPosition, agent.stoppingDistance);
        }
    }
}