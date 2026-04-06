using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using System.Collections.Generic;

/// <summary>
/// Singleton that manages ORCA agent-to-agent avoidance for MonoBehaviour troops.
///
/// Usage:
///   1. TroopMovement calls Register() in Initialize, Unregister() in OnDestroy.
///   2. Each frame, TroopManagement (or whoever drives movement) calls
///      ORCAManager.Instance.Prepare() once BEFORE any UpdateMovement() calls.
///   3. Inside TroopMovement.UpdateMovement(), call
///      ORCAManager.Instance.ComputeAvoidanceVelocity(id, preferredVel) to get adjusted velocity.
/// </summary>
public class ORCAManager : MonoBehaviour
{
    public static ORCAManager Instance { get; private set; }

    [Header("ORCA Settings")]
    [Tooltip("Collision radius per agent (world units).")]
    public float agentRadius = 0.9f;

    [Tooltip("How far ahead (seconds) agents plan avoidance. Higher = earlier avoidance.")]
    public float timeHorizon = 3f;

    [Tooltip("Maximum neighbor distance for ORCA queries.")]
    public float neighborDistance = 8f;

    [Tooltip("Max neighbors considered per agent.")]
    public int maxNeighbors = 10;

    // ---- internal state ----

    // Flat list of registered agents — id is the index.
    // We use a free-list for O(1) register/unregister.
    private List<AgentSlot> agents = new List<AgentSlot>();
    private Stack<int> freeSlots = new Stack<int>();

    // Spatial hash rebuilt every Prepare()
    private float cellSize;
    private Dictionary<int, List<int>> spatialHash = new Dictionary<int, List<int>>();
    private List<List<int>> cellListPool = new List<List<int>>();

    // Scratch buffer for neighbor queries 
    private List<int> neighborScratch = new List<int>(32);

    /// <summary>Fraction of normal radius used by stopped/stationary agents (0-1).</summary>
    private const float STATIONARY_RADIUS_SCALE = 0.3f;

    private struct AgentSlot
    {
        public bool   active;
        public bool   isStationary; // true if the agent has reached its destination / is not moving
        public float2 position;
        public float2 velocity;
        public float  radius;
        public float  maxSpeed;
    }

    // ----------------------------------------------------------------
    // Lifecycle
    // ----------------------------------------------------------------

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        cellSize = neighborDistance;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Auto-rebuild spatial hash each frame so callers don't need to remember to call Prepare().
    /// For manual/test scenarios, Prepare() can also be called explicitly.
    /// </summary>
    void Update()
    {
        Prepare();
    }

    // ----------------------------------------------------------------
    // Registration
    // ----------------------------------------------------------------

    /// <summary>Register an agent. Returns an id used for all future calls.</summary>
    public int Register(float maxSpeed, float customRadius = -1f)
    {
        float r = customRadius > 0f ? customRadius : agentRadius;
        var slot = new AgentSlot
        {
            active   = true,
            position = float2.zero,
            velocity = float2.zero,
            radius   = r,
            maxSpeed = maxSpeed
        };

        int id;
        if (freeSlots.Count > 0)
        {
            id = freeSlots.Pop();
            agents[id] = slot;
        }
        else
        {
            id = agents.Count;
            agents.Add(slot);
        }
        return id;
    }

    /// <summary>Unregister an agent.</summary>
    public void Unregister(int id)
    {
        if (id < 0 || id >= agents.Count) return;
        var slot = agents[id];
        slot.active = false;
        agents[id] = slot;
        freeSlots.Push(id);
    }

    /// <summary>Update an agent's position, velocity, and stationary status. Call each frame before Prepare().</summary>
    public void UpdateAgent(int id, float2 position, float2 velocity, bool isStationary = false)
    {
        if (id < 0 || id >= agents.Count) return;
        var slot = agents[id];
        slot.position     = position;
        slot.velocity     = velocity;
        slot.isStationary = isStationary;
        agents[id] = slot;
    }

    // ----------------------------------------------------------------
    // Per-frame preparation — rebuild spatial hash
    // ----------------------------------------------------------------

    /// <summary>Call ONCE per frame before any ComputeAvoidanceVelocity calls.</summary>
    public void Prepare()
    {
        cellSize = math.max(neighborDistance, 1f);

        // recycle cell lists
        foreach (var kvp in spatialHash)
        {
            kvp.Value.Clear();
            cellListPool.Add(kvp.Value);
        }
        spatialHash.Clear();

        for (int i = 0; i < agents.Count; i++)
        {
            if (!agents[i].active) continue;
            int key = CellKey(agents[i].position);
            if (!spatialHash.TryGetValue(key, out var list))
            {
                list = cellListPool.Count > 0
                    ? cellListPool[cellListPool.Count - 1]
                    : new List<int>(8);
                if (cellListPool.Count > 0) cellListPool.RemoveAt(cellListPool.Count - 1);
                spatialHash[key] = list;
            }
            list.Add(i);
        }
    }

    // ----------------------------------------------------------------
    // Query
    // ----------------------------------------------------------------

    /// <summary>
    /// Compute an ORCA-adjusted velocity for agent <paramref name="id"/>.
    /// Near the goal, ORCA influence fades so troops can settle at their destination.
    /// </summary>
    /// <param name="id">Agent id from Register().</param>
    /// <param name="preferredVelocity">Desired velocity from pathfinding (direction * speed).</param>
    /// <param name="distanceToGoal">Current distance to the agent's goal position.</param>
    /// <param name="slowdownDistance">Distance at which the agent starts slowing down. ORCA fades over this range.</param>
    /// <returns>Adjusted velocity that avoids collisions with nearby agents.</returns>
    public float2 ComputeAvoidanceVelocity(int id, float2 preferredVelocity, float distanceToGoal = float.MaxValue, float slowdownDistance = 0f)
    {
        if (id < 0 || id >= agents.Count || !agents[id].active)
            return preferredVelocity;

        AgentSlot self = agents[id];

        // gather neighbors
        using var neighbors = new NativeList<ORCASolver.AgentData>(maxNeighbors, Allocator.Temp);

        FindNeighbors(id, self.position, neighbors);

        if (neighbors.Length == 0)
            return preferredVelocity;

        float2 orcaVelocity = ORCASolver.ComputeNewVelocity(
            preferredVelocity,
            self.position,
            self.velocity,
            self.radius,
            self.maxSpeed,
            neighbors.AsArray(),
            neighbors.Length,
            timeHorizon);

        // Goal-proximity dampening: linearly fade ORCA influence as we approach the target.
        // At slowdownDistance → full ORCA; at stoppingDistance (≈0) → zero ORCA.
        if (slowdownDistance > 0f && distanceToGoal < slowdownDistance)
        {
            float orcaFactor = math.saturate(distanceToGoal / slowdownDistance);
            return math.lerp(preferredVelocity, orcaVelocity, orcaFactor);
        }

        return orcaVelocity;
    }

    // ----------------------------------------------------------------
    // Spatial hash helpers
    // ----------------------------------------------------------------

    private void FindNeighbors(int selfId, float2 position, NativeList<ORCASolver.AgentData> result)
    {
        int cx = (int)math.floor(position.x / cellSize);
        int cz = (int)math.floor(position.y / cellSize);

        float distSq = neighborDistance * neighborDistance;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                int key = HashCell(cx + dx, cz + dz);
                if (!spatialHash.TryGetValue(key, out var cell)) continue;

                for (int i = 0; i < cell.Count; i++)
                {
                    int otherId = cell[i];
                    if (otherId == selfId) continue;
                    if (!agents[otherId].active) continue;

                    AgentSlot other = agents[otherId];
                    if (math.distancesq(position, other.position) > distSq) continue;

                    // Stationary (arrived) neighbors use a much smaller radius
                    // so moving agents can approach packed groups.
                    float effectiveRadius = other.isStationary
                        ? other.radius * STATIONARY_RADIUS_SCALE
                        : other.radius;

                    result.Add(new ORCASolver.AgentData
                    {
                        position = other.position,
                        velocity = other.velocity,
                        radius   = effectiveRadius,
                        maxSpeed = other.maxSpeed
                    });

                    if (result.Length >= maxNeighbors) return;
                }
            }
        }
    }

    int CellKey(float2 pos)
    {
        int cx = (int)math.floor(pos.x / cellSize);
        int cz = (int)math.floor(pos.y / cellSize);
        return HashCell(cx, cz);
    }

    static int HashCell(int x, int z)
    {
        // large prime spatial hash — low collisions for typical world sizes
        unchecked { return x * 73856093 ^ z * 19349663; }
    }
}
