using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;


public abstract class TroopAI : MonoBehaviour
{

    [SerializeField] protected TroopState troopState; // disengaged, targetinrange, fighting enemy, retreating, undercontrol

    // maybe differentiating attacking troop and inanimate objects
    public enum TroopState
    {
        Disengaged,
        TargetDetected,
        InCombat,
        Retreating
    }
    [SerializeField] protected ITroopBehaviour troopBehaviour;
    [Header("Troop Attributes")]
    [SerializeField] protected string troopName;
    public string TroopName{ get { return troopName; } }
    [SerializeField] protected GameObject troopModel;
    public GameObject TroopModel{ get{ return troopModel; } set { troopModel = value; } } // why do we need this???
    [SerializeField] protected float maxSpeed = 3.5f;
    public float MaxSpeed { get { return maxSpeed; } }

    [Header("Control Attributes")]

    // Combat Stats
    [SerializeField] protected Transform enemyTarget;
    public Transform EnemyTarget { get { return enemyTarget; } set { enemyTarget = value; } }

    [SerializeField] protected float aggroRange = 10.0f;
    public float AggroRange { get { return aggroRange; } set { aggroRange = value; } } // range which troop engages enemy

    [SerializeField] protected float attackRange = 5.0f;
    public float AttackRange { get { return attackRange; } set { attackRange = value; } }

    // Combat Attributes
    protected TroopCombatSystem enemyTargetCombatSystem{ get; set; }
    protected HashSet<string> exceptionBulletList = new HashSet<string>{"Troop"}; // i have no clue what this is
    protected TroopCombatSystem selfCombatSystem { get; set; }

    // Movement Attributes
    
    // private LineRenderer pathIndicator;
    protected NavMeshAgent navMeshAgent;
    public virtual void Attack(Transform target)
    {
        // gotta rename Attack
        transform.LookAt(target);
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w); // essentially only allows the y axis to move
        
        enemyTargetCombatSystem = target.GetComponent<TroopCombatSystem>(); // interacts directly with the target rather than creating a projectile
        if (enemyTargetCombatSystem != null)
        {
            troopBehaviour.InteractWithTarget(selfCombatSystem, enemyTargetCombatSystem);
        }
        selfCombatSystem.ResetAttackCooldown();
    }
    protected virtual void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        // pathIndicator = GetComponent<LineRenderer>();
        // pathIndicator.enabled = false;
        selfCombatSystem = gameObject.GetComponent<TroopCombatSystem>();
        navMeshAgent.speed = maxSpeed;
        AddEntityToAliveList();
    }


    protected virtual void Update()
    {
        selfCombatSystem.DecrementAttackCoooldown(Time.deltaTime); // modularise

        // Decide Whether to Engage In Combat
        DecideState();

       // Handle states
        HandleState();
    }

    protected void HandleState()
    {
        HandleCombat();
        HandleMovement();
    }
    protected void HandleCombat()
    {
        // TODO: consider this
        if (troopState == TroopState.InCombat)
        {
            FightEnemy();
        }
    }

    protected void HandleMovement()
    {
        MoveTowardsTarget(enemyTarget); 
    }


    protected void MoveTowardsTarget(Transform target)
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = maxSpeed * (1 - selfCombatSystem.GetEffectStrength("slow") + selfCombatSystem.GetEffectStrength("haste")); // diff function
        navMeshAgent.SetDestination(target.position);
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, path);

    
        // pathIndicator.enabled = true;
        // pathIndicator.positionCount = path.corners.Length;
        // for (int i = 0; i < path.corners.Length; i++)
        // {
        //     pathIndicator.SetPosition(i, path.corners[i]);
        // }
        
    }
    protected void DecideMovementState()
    {
        if (troopState == TroopState.Disengaged && enemyTarget != null)
        {
            troopState = TroopState.TargetDetected;
        }
    }

    protected abstract void FindAndSetAllTargets();

    protected void DecideState()
    {
        FindAndSetAllTargets();
        DecideCombatState();
    }

    protected void FightEnemy() {
        // which is called by combatsystem
        enemyTargetCombatSystem = enemyTarget.GetComponent<TroopCombatSystem>();
        float distance = Vector3.Distance(transform.position, enemyTarget.position);
        if (selfCombatSystem.CanAttack() && navMeshAgent.velocity.magnitude <= 0.05)
        {
            Attack(enemyTarget);
        }
        else
        {
            ReadjustPositioningBetweenAttack();
        }
    }

    protected void DecideCombatState()
    {
        if (troopState != TroopState.Retreating)
        {
            float distance = Vector3.Distance(transform.position, enemyTarget.position);
            if (distance <= attackRange /*&& agent.velocity.magnitude <= 0.1f*/)
            {
                navMeshAgent.isStopped = true;
                troopState = TroopState.InCombat;
            }
            else
            {
                if (enemyTarget != null)
                {
                    troopState = TroopState.TargetDetected;
                }
                else
                {
                    troopState = TroopState.Disengaged;
                }

            }
        }
    }
    
    protected void ReadjustPositioningBetweenAttack()
    {
        // have tolerance before moving
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(enemyTarget.position); 
    }

    protected abstract void AddEntityToAliveList();
    public abstract void RemoveEntityFromAliveList();
    public abstract List<GameObject> GetEntityAliveList(); // TODO: separate get allies in same team and enemies

}