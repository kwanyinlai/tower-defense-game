using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

public class TroopMovement : MonoBehaviour
{
    private TroopStats stats;
    private Vector2 currentVelocity = Vector2.zero;

    // Navigation/Pathfinding State
    private bool isMoving = false;
    private Vector3 targetPosition;
    private NavigationMode currentNavMode = NavigationMode.DirectSteer;
    private List<Vector3> waypoints;
    private int currentWaypointIndex = 0;
    private bool hasReachedDestination = false;
    private bool isUsingSharedFlowField = false;

    // Movement Configurations
    private float waypointReachedDistance = 4f;
    private float stoppingDistance = 1.5f;
    private float slowdownDistance = 3f;

    // Batch Update
    private bool positionCacheValid = false;
    private Vector3 cachedPosition;
    private float cachedDistance;

    // ORCA avoidance
    private int orcaAgentId = -1;

    void Start()
    {
    }

    private void OnDestroy()
    {
        if (isUsingSharedFlowField && PathfindingManager.Instance != null)
        {
            PathfindingManager.Instance.UnregisterFlowFieldUser(targetPosition);
        }
        if (orcaAgentId >= 0 && ORCAManager.Instance != null)
        {
            ORCAManager.Instance.Unregister(orcaAgentId);
            orcaAgentId = -1;
        }
    }

    public void Initialize(TroopStats troopStats)
    {
        stats = troopStats;
        if (ORCAManager.Instance != null)
        {
            orcaAgentId = ORCAManager.Instance.Register(stats.maxSpeed);
        }
    }

    public void SetTarget(Vector3 destination)
    {
        // early exit if already at destination
        float distanceSqr = (destination - transform.position).sqrMagnitude;
        if (distanceSqr < stoppingDistance * stoppingDistance)
        {
            return;
        }

        // clean up previous shared flow field usage
        if (isUsingSharedFlowField && PathfindingManager.Instance != null)
        {
            PathfindingManager.Instance.UnregisterFlowFieldUser(targetPosition);
            isUsingSharedFlowField = false;
        }

        targetPosition = destination;
        isMoving = true;
        hasReachedDestination = false;
        waypoints = null;
        positionCacheValid = false;

        // determine navigation mode
        currentNavMode = PathfindingManager.Instance.GetNavigationMode(transform.position, destination);

        switch (currentNavMode)
        {
            case NavigationMode.DirectSteer:
                // no pathfinding needed — just steer toward target
                break;

            case NavigationMode.SharedFlowField:
                // use existing shared flow field
                PathfindingManager.Instance.RegisterFlowFieldUser(destination);
                isUsingSharedFlowField = true;
                break;

            case NavigationMode.AStarWaypoints:
                // request A* path
                PathfindingManager.Instance.RequestPath(
                    transform.position,
                    destination,
                    OnPathReceived
                );
                break;
        }
    }

    /// <summary>
    /// Set target using a shared flow field (for group commands).
    /// Call PathfindingManager.RequestSharedFlowField() first before calling this on each troop.
    /// </summary>
    public void SetTargetWithFlowField(Vector3 destination)
    {
        // clean up previous
        if (isUsingSharedFlowField && PathfindingManager.Instance != null)
        {
            PathfindingManager.Instance.UnregisterFlowFieldUser(targetPosition);
        }

        targetPosition = destination;
        isMoving = true;
        hasReachedDestination = false;
        waypoints = null;
        positionCacheValid = false;
        currentNavMode = NavigationMode.SharedFlowField;
        isUsingSharedFlowField = true;

        PathfindingManager.Instance.RegisterFlowFieldUser(destination);
    }

    private void OnPathReceived(PathResult result)
    {
        if (!result.success)
        {
            Debug.LogWarning($"Failed to find path for {gameObject.name}");
            StopMoving();
            return;
        }

        waypoints = result.waypoints;
        currentWaypointIndex = 0;
    }

    public void StopMoving()
    {
        if (isUsingSharedFlowField && PathfindingManager.Instance != null)
        {
            PathfindingManager.Instance.UnregisterFlowFieldUser(targetPosition);
            isUsingSharedFlowField = false;
        }

        isMoving = false;
        hasReachedDestination = true;
        currentVelocity = Vector2.zero;
        waypoints = null;
    }

    public void UpdateMovement(float deltaTime)
    {
        if (!isMoving || hasReachedDestination) return;

        // cache position for batch updates
        cachedPosition = transform.position;
        cachedDistance = (targetPosition - cachedPosition).sqrMagnitude;
        positionCacheValid = true;

        // update ORCA agent data (position + velocity + stationary status)
        if (orcaAgentId >= 0 && ORCAManager.Instance != null)
        {
            ORCAManager.Instance.UpdateAgent(
                orcaAgentId,
                new Unity.Mathematics.float2(cachedPosition.x, cachedPosition.z),
                currentVelocity,
                hasReachedDestination || currentVelocity.sqrMagnitude < 0.01f);
        }

        Vector3 desiredDirection = GetDesiredDirection();

        if (desiredDirection == Vector3.zero)
        {
            StopMoving();
            return;
        }

        Vector2 direction2D = new Vector2(desiredDirection.x, desiredDirection.z);
        ApplyMovement(direction2D, deltaTime);
        positionCacheValid = false;
    }

    private Vector3 GetDesiredDirection()
    {
        float distanceSqr = positionCacheValid
            ? cachedDistance
            : (targetPosition - transform.position).sqrMagnitude;

        // reached destination check (prevent oscillation)
        if (distanceSqr < stoppingDistance * stoppingDistance)
        {
            hasReachedDestination = true;
            return Vector3.zero;
        }

        Vector3 direction;

        switch (currentNavMode)
        {
            case NavigationMode.DirectSteer:
                direction = GetDirectSteerDirection();
                break;

            case NavigationMode.SharedFlowField:
                direction = GetSharedFlowFieldDirection(distanceSqr);
                break;

            case NavigationMode.AStarWaypoints:
                direction = GetWaypointDirection();
                // once close enough, switch to direct steer (waypoints done or close to target)
                float distSteerSqr = PathfindingManager.Instance.DirectSteerDistance;
                distSteerSqr *= distSteerSqr;
                if (distanceSqr < distSteerSqr)
                {
                    currentNavMode = NavigationMode.DirectSteer;
                }
                break;

            default:
                direction = GetDirectSteerDirection();
                break;
        }

        return direction;
    }

    private Vector3 GetDirectSteerDirection()
    {
        Vector3 direction = (targetPosition - transform.position);
        direction.y = 0;
        return direction.normalized;
    }

    private Vector3 GetWaypointDirection()
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            return GetDirectSteerDirection();
        }

        Vector2 pos2D = new Vector2(cachedPosition.x, cachedPosition.z);
        float reachedSqr = waypointReachedDistance * waypointReachedDistance;

        // advance past any waypoints we are close enough to (handles overshooting)
        while (currentWaypointIndex < waypoints.Count)
        {
            Vector3 wp = waypoints[currentWaypointIndex];
            Vector2 wp2D = new Vector2(wp.x, wp.z);
            float distSqr = (pos2D - wp2D).sqrMagnitude;

            if (distSqr < reachedSqr)
            {
                currentWaypointIndex++;
                continue;
            }

            // skip-ahead: if the next waypoint is closer than the current one,
            // and moving toward the next one doesn't take us backwards, skip current
            if (currentWaypointIndex + 1 < waypoints.Count)
            {
                Vector3 nextWp = waypoints[currentWaypointIndex + 1];
                Vector2 nextWp2D = new Vector2(nextWp.x, nextWp.z);
                float distToNextSqr = (pos2D - nextWp2D).sqrMagnitude;
                float wpToNextSqr = (wp2D - nextWp2D).sqrMagnitude;

                // if we're past the current waypoint (closer to next than current-to-next distance)
                // this means we've overshot
                if (distToNextSqr < wpToNextSqr)
                {
                    currentWaypointIndex++;
                    continue;
                }
            }

            break;
        }

        // all waypoints traversed — direct steer to final target
        if (currentWaypointIndex >= waypoints.Count)
        {
            return GetDirectSteerDirection();
        }

        Vector3 currentWaypoint = waypoints[currentWaypointIndex];
        Vector3 direction = (currentWaypoint - cachedPosition);
        direction.y = 0;
        return direction.normalized;
    }

    private Vector3 GetSharedFlowFieldDirection(float distanceSqr)
    {
        if (PathfindingManager.Instance == null)
            return GetDirectSteerDirection();

        Vector3 flowDirection = PathfindingManager.Instance.GetSharedFlowFieldDirection(
            transform.position,
            targetPosition
        );

        // blend with direct steering when close enough
        float blendRadius = PathfindingManager.Instance.FlowFieldBlendRadius;
        float blendRadiusSqr = blendRadius * blendRadius;

        if (distanceSqr <= blendRadiusSqr)
        {
            Vector3 directDirection = GetDirectSteerDirection();
            float distance = Mathf.Sqrt(distanceSqr);
            float blendFactor = 1f - (distance / blendRadius);
            return Vector3.Lerp(flowDirection, directDirection, blendFactor).normalized;
        }

        return flowDirection;
    }

    private void ApplyMovement(Vector2 direction, float deltaTime)
    {
        float distanceToGoal = Vector3.Distance(transform.position, targetPosition);

        // slowdown near target
        float speedMultiplier = 1f;
        if (distanceToGoal < slowdownDistance)
        {
            speedMultiplier = Mathf.Clamp01(distanceToGoal / slowdownDistance);
            speedMultiplier = Mathf.Max(speedMultiplier, 0.2f); // min 0.2 of full speed
        }

        Vector2 desiredVelocity = direction.normalized * stats.maxSpeed * speedMultiplier;

        // ORCA: adjust desired velocity to avoid nearby agents
        // Near the goal, ORCA influence fades so troops can settle at their destination.
        if (orcaAgentId >= 0 && ORCAManager.Instance != null)
        {
            Unity.Mathematics.float2 adjusted = ORCAManager.Instance.ComputeAvoidanceVelocity(
                orcaAgentId,
                desiredVelocity,
                distanceToGoal,
                slowdownDistance);
            desiredVelocity = new Vector2(adjusted.x, adjusted.y);
        }

        currentVelocity = Vector2.MoveTowards(
            currentVelocity,
            desiredVelocity,
            stats.acceleration * deltaTime
        );

        // translational movement
        Vector3 movement = new Vector3(currentVelocity.x, 0, currentVelocity.y) * deltaTime;

        // overshooting checks
        float movementMagnitude = movement.magnitude;
        if (movementMagnitude > distanceToGoal)
        {
            movement = movement.normalized * distanceToGoal;
        }

        transform.position += movement;

        // rotational movement
        if (currentVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, angle - 90f, 0f);
        }
    }

    #region Getters

    public bool IsMoving() => isMoving;
    public Vector3 GetCurrentDestination() => targetPosition;
    public Vector2 GetCurrentVelocity() => currentVelocity;
    public float GetCurrentSpeed() => currentVelocity.magnitude;
    public NavigationMode GetNavigationMode() => currentNavMode;

    #endregion

}