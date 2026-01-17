using UnityEngine;

// delegator class
public class TroopAI : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private TroopStats stats;
    [SerializeField] private TroopFaction faction;

    [Header("Debug")]
    [SerializeField] private TroopState currentState;
    [SerializeField] private Transform currentTarget;

    // Troop Core Systems
    private TroopMovement movementSystem;
    private TroopCombat combatSystem;
    private TroopTargetSelector targetingSystem;
    private TroopStateMachine stateMachineSystem;


    private void Awake()
    {
        InitializeSystems();
    }

    private void Start()
    {
        RegisterWithFaction();
    }

    private void Update()
    {
        UpdateState();
        UpdateBehavior();
        UpdateCombat();
    }

    private void OnDestroy()
    {
        UnregisterFromFaction();
    }


    #region Initialization

    private void InitializeSystems()
    {
        movementSystem = GetComponent<TroopMovement>() ?? gameObject.AddComponent<TroopMovement>();
        combatSystem = GetComponent<TroopCombat>() ?? gameObject.AddComponent<TroopCombat>();
        targetingSystem = GetComponent<TroopTargetSelector>() ?? gameObject.AddComponent<TroopTargetSelector>();
        stateMachineSystem = new TroopStateMachine();

        movementSystem.Initialize(stats);
        combatSystem.Initialize(stats);
        targetingSystem.Initialize(stats.aggroRange, faction);
    }

    private void RegisterWithFaction()
    {
        FactionManager.Instance.RegisterTroop(transform, faction);
    }

    private void UnregisterFromFaction()
    {
        FactionManager.Instance.UnregisterTroop(transform, faction);
    }

    #endregion

    #region State Management

    private void UpdateState()
    {
        currentTarget = targetingSystem.GetBestTarget(transform.position);

        TroopState newState = stateMachineSystem.DetermineState(
            currentState,
            currentTarget,
            combatSystem,
            movementSystem,
            stats
        );

        if (newState != currentState)
        {
            // new action based on new state
            OnStateChanged(currentState, newState);
            currentState = newState;
        }
    }

    private void OnStateChanged(TroopState oldState, TroopState newState)
    {
        switch (oldState)
        {
            case TroopState.MovingToTarget:
                movementSystem.StopMoving();
                break;
            case TroopState.InCombat:
                combatSystem.ExitCombat();
                break;
        }

        switch (newState)
        {
            case TroopState.Idle:
                movementSystem.StopMoving();
                break;
            case TroopState.MovingToTarget:
                if (currentTarget != null)
                {
                    movementSystem.SetTarget(currentTarget.position);
                }
                break;
            case TroopState.InCombat:
                movementSystem.StopMoving();
                combatSystem.EnterCombat(currentTarget);
                break;
        }
    }

    #endregion

    #region Behavior Update

    private void UpdateBehavior()
    {
        switch (currentState)
        {
            case TroopState.Idle:
                HandleIdleState();
                break;

            case TroopState.MovingToTarget:
                HandleMovingState();
                break;

            case TroopState.InCombat:
                HandleCombatState();
                break;

            case TroopState.Retreating:
                HandleRetreatingState();
                break;
        }
    }

    private void UpdateCombat()
    {
        combatSystem.TickCombat(); 
    }

    private void HandleIdleState()
    {
        // idle behaviour
    }

    private void HandleMovingState()
    {
        if (currentTarget == null)
        {
            movementSystem.StopMoving();
            return;
        }

        // update destination only if moved enough
        if (Vector3.Distance(movementSystem.GetCurrentDestination(), currentTarget.position) > 5f)
        {
            movementSystem.SetTarget(currentTarget.position);
        }

        movementSystem.UpdateMovement(Time.deltaTime);
    }

    private void HandleCombatState()
    {
        if (currentTarget == null)
        {
            return;
        }

        // check reposition
        float distance = Vector3.Distance(transform.position, currentTarget.position);
        
        if (distance > stats.attackRange + 1f)
        {
            // chase
            movementSystem.SetTarget(currentTarget.position);
            movementSystem.UpdateMovement(Time.deltaTime);
        }
        else if (distance < stats.attackRange - 0.5f)
        {
            // backup
            Vector3 retreatDirection = (transform.position - currentTarget.position).normalized;
            Vector3 retreatPosition = transform.position + retreatDirection * 0.5f;
            movementSystem.SetTarget(retreatPosition);
            movementSystem.UpdateMovement(Time.deltaTime);
        }
        else
        {
            // attack
            movementSystem.StopMoving();
            combatSystem.TryAttack(currentTarget, transform);
        }
    }

    private void HandleRetreatingState()
    {
        // TODO: retreat behaviour
    }

    #endregion

    #region Public Interface

    public TroopStats GetStats() => stats;
    public TroopFaction GetFaction() => faction;
    public TroopState GetCurrentState() => currentState;
    public Transform GetCurrentTarget() => currentTarget;


    public void CommandAttack(Transform target)
    {
        // override and attack - maybe debug but for ohter mechanics too
        currentTarget = target;
        currentState = TroopState.MovingToTarget;
        movementSystem.SetTarget(target.position);
    }


    public void CommandMoveTo(Vector3 position)
    {
        // override and move - maybe debug but for other mechanics
        currentTarget = null;
        currentState = TroopState.MovingToTarget;
        movementSystem.SetTarget(position);
    }


    public void OnDeath()
    {
        currentState = TroopState.Dead;
        movementSystem.StopMoving();
        UnregisterFromFaction();
        // death effects, animation other clean up needed
    }

    #endregion
}