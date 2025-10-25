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

    [Header("Troop Attributes")]
    [SerializeField] protected string troopName;
    public string TroopName{ get { return troopName; } }
    [SerializeField] protected GameObject troopModel;
    public GameObject TroopModel{ get{ return troopModel; } set { troopModel = value; } } // why do we need this???
    [SerializeField] protected float maxSpeed = 3.5f;
    public float MaxSpeed { get{ return maxSpeed; } }

    [Header("Combat Attributes")]
    [SerializeField] protected float attackRange = 5.0f ;
    public float AttackRange{ get{ return attackRange; } set{ attackRange = value; } }
    [SerializeField] protected int dmg = 10;
    public int Damage
    {
        get { return dmg; }
        set { dmg = value; }
    }
    [SerializeField] protected float aggroRange = 10.0f;
    public float AggroRange { get{ return attackRange; } set{ attackRange = value; } } // range which troop engages enemy
    [SerializeField] protected Transform enemyTarget;
    public Transform EnemyTarget { get{ return enemyTarget; } set{ enemyTarget = value; } }

    [Header("Control Attributes")]

    protected bool isUnderSelection = false;
    public bool IsUnderSelection { get{ return isUnderSelection; } set{ isUnderSelection = value; } }


    // Combat Stats
    protected float atkCooldown { get; set; } = 1.5f;
    protected float atkTimer { get; set; } = 0f;

    // Combat Attributes
    protected CombatSystem enemyTargetCombatSystem
     { get; set; }
    protected HashSet<string> exceptionBulletList = new HashSet<string>{"Troop"};
    protected CombatSystem combatSystem { get; set; }

    // Movement Attributes
    
    // private LineRenderer pathIndicator;
    protected NavMeshAgent navMeshAgent;
    public abstract void Attack(Transform target);
    protected virtual void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        // pathIndicator = GetComponent<LineRenderer>();
        // pathIndicator.enabled = false;
        combatSystem = gameObject.GetComponent<CombatSystem>();
        navMeshAgent.speed = maxSpeed;
    }


    protected virtual void Update()
    {
        if (atkTimer > 0f) { atkTimer -= Time.deltaTime; }

        // Decide Whether to Engage In Combat
        DecideState();

       // Combat Mechanics
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
        if (troopState.InCombat)
        {
            FightEnemy();
        }
    }


    protected void MoveTowardsTarget(Transform target)
    {
        agent.isStopped = false;
        agent.speed = maxSpeed * (1 - combatSystem.GetEffectStrength("slow") + combatSystem.GetEffectStrength("haste")); // diff function
        agent.SetDestination(target.position);
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, path);

    }
        // pathIndicator.enabled = true;
        // pathIndicator.positionCount = path.corners.Length;
        // for (int i = 0; i < path.corners.Length; i++)
        // {
        //     pathIndicator.SetPosition(i, path.corners[i]);
        // }
        
    }
    protected void HandleMovement()
    {
        if (troopState == TroopState.Disengaged && enemyTarget != null)
        {
            troopState = TroopState.TargetDetected;
            MoveTowardsTarget(enemyTarget);
        }
    }

    protected abstract void FindAndSetAllTargets();

    protected void DecideState()
    {
        FindAndSetAllTargets();
        DecideCombatState();
    }

    protected void FightEnemy() {
        agent.speed = maxSpeed * (1 - combatSystem.GetEffectStrength("slow")); // TOOD: this should probably be put into a separate function
        // which is called by combatsystem
        FightEnemyInRange(); 
    }

    protected abstract void Disengaged(); // TODO: do we need this

    protected void DecideCombatState()
    {
        if (troopState != TroopState.Retreating)
        {
            if (distance <= attackRange /*&& agent.velocity.magnitude <= 0.1f*/)
            {
                agent.isStopped = true;
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

    protected void FightEnemyInRange()
    {
        enemyTargetCombatSystem = enemyTarget.GetComponent<CombatSystem>();
        float distance = Vector3.Distance(transform.position, enemyTarget.position);
        if (atkTimer <= 0f && agent.velocity.magnitude <= 0.05)
        {
            Attack(enemyTarget);
        }
        else
        {
            ReadjustPositioningBetweenAttack();
        }
    }
    
    protected void ReadjustPositioningBetweenAttack()
    {
        // have tolerance before moving
        agent.isStopped = false;
        agent.SetDestination(enemyTarget.position); 
    }


}
