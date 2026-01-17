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
    private List<Vector3> waypoints;
    private int currentWaypointIndex = 0;
    private bool hasReachedDestination = false;
    
    // Movement Configurations
    private float waypointReachedDistance = 2f;
    private float stoppingDistance = 1.5f;
    private float slowdownDistance = 3f;

    // Batch Update
    private bool positionCacheValid = false;
    private Vector3 cachedPosition;
    private float cachedDistance;

    // Local Avoidance
    private ILocalAvoidance localAvoidance = null;

    void Start()
    {
    }

    private void OnDestroy()
    {
        ;
    }
    public void Initialize(TroopStats troopStats)
    {
        stats = troopStats;
    }

    public void SetTarget(Vector3 destination)
    {
        // early exit if reached to reduce redundant requests
        float distanceSqr = (destination - transform.position).sqrMagnitude;
        if (distanceSqr < stoppingDistance * stoppingDistance)
        {
            return; 
        }
        
        targetPosition = destination;
        isMoving = true;
        hasReachedDestination = false;
        waypoints = null;
        positionCacheValid = false;
        
        PathfindingManager.Instance.RequestPath(
            transform.position,
            destination,
            OnPathReceived
        );
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
        isMoving = false;
        hasReachedDestination = true;
        currentVelocity = Vector2.zero;
        waypoints = null;
    }

    public void UpdateMovement(float deltaTime)
    {
        if (!isMoving || hasReachedDestination) return;

         // cache position for batch updates - so calculation is from same position
        cachedPosition = transform.position;
        cachedDistance = (targetPosition - cachedPosition).sqrMagnitude;
        positionCacheValid = true;

        Vector3 desiredDirection = GetDesiredDirection();
        
        if (desiredDirection == Vector3.zero)
        {
            // destination reached
            StopMoving();
            return;
        }

        Vector2 direction2D = new Vector2(desiredDirection.x, desiredDirection.z);
        ApplyMovement(direction2D, deltaTime);
        positionCacheValid = false;
    }

    private Vector3 GetDesiredDirection()
    {
        float distanceToGoal = positionCacheValid ? 
            cachedDistance : 
            (targetPosition - transform.position).sqrMagnitude;

        
        // flag to determine reached destination (prevent oscillation)
        if (distanceToGoal < stoppingDistance)
        {
            hasReachedDestination = true;
            return Vector3.zero;
        }
        Vector3 direction;
        // decide nav mode
        if (PathfindingManager.Instance.ShouldUseFlowField(transform.position, targetPosition))
        {
            // regional flowfields
            direction = GetFlowFieldDirection(distanceToGoal);
        }
        else
        {
            direction = GetWaypointDirection();
        }

        if (localAvoidance != null)
        {
            direction = localAvoidance.GetAvoidanceDirection(transform.position, direction, stats.maxSpeed);
        }
        return direction;
    }

    private Vector3 GetWaypointDirection()
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            // no waypoints, fallback to move directly towards target
            return (targetPosition - transform.position).normalized;
        }

        Vector3 currentWaypoint = waypoints[currentWaypointIndex];
        float distanceToWaypoint = (new Vector2(cachedPosition.x, cachedPosition.z) - 
            new Vector2(currentWaypoint.x, currentWaypoint.z)).sqrMagnitude;
        float waypointReachedDistanceSqr = waypointReachedDistance * waypointReachedDistance;
        // squared because we're now using sqrMagnitude - so equiv scale 

        // move to next waypoint
        if (distanceToWaypoint < waypointReachedDistanceSqr)
        {
            currentWaypointIndex++;
            
            // all waypoints reached?
            if (currentWaypointIndex >= waypoints.Count)
            {
                // all waypoints done - head directly and hopefully trigger flowfield
                return (targetPosition - transform.position).normalized;
            }
            
            currentWaypoint = waypoints[currentWaypointIndex];
        }

        Vector3 direction = (currentWaypoint - transform.position).normalized;
        direction.y = 0;
        return direction;
    }

    private Vector3 GetFlowFieldDirection(float distanceToGoal)
    {
        Vector3 flowDirection = PathfindingManager.Instance.GetFlowFieldDirection(
            transform.position,
            targetPosition
        );

        // blend w/ direct vector when close enough
        float blendRadius = PathfindingManager.Instance.FlowFieldBlendRadius;
        
        if (distanceToGoal <= blendRadius)
        {
            Vector3 directDirection = (targetPosition - transform.position).normalized;
            directDirection.y = 0;
            
            float blendFactor = 1f - (distanceToGoal / blendRadius);
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

    #endregion

}