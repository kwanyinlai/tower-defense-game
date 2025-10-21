using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;


public abstract class TroopAI : MonoBehaviour
{

    [SerializeField] protected string troopState; // disengaged, targetinrange, fighting enemy, stopped
    enum TroopState
    {
        Disengaged,
        TargetDetected,
        InCombat,
        Stopped,
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
    protected CombatSystem combatSystem { get;  set; }
    protected bool inCombat = false;
    public bool InCombat{
        get{ return inCombat; }
    }

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

    protected void HandleCombat()
    {
        // TODO: consider this
        if (inCombat)
        {
            FightEnemy();
        }
        else
        {
            Disengaged();
        } 
    }

    protected abstract void FindAndSetAllTargets();
    protected void ControlTroop() {
        if(IsUnderSelection){
            ShowCircle();
        }
        else{
            HideCircle();
        }
    }

    protected void DecideState()
    {
        // TODO: Implement default state machine for all troops

        FindAndSetAllTargets();

        DecideState();

        if (CheckInCombat())
        {
            inCombat = true;
        }
        else
        {
            inCombat = false;
        }
    }

    protected void FightEnemy() {
        agent.speed = maxSpeed * (1 - combatSystem.GetEffectStrength("slow"));
        FightEnemyInRange(); // why are we separating fight enemy with fight enemy in range
    }

    protected abstract void Disengaged();
  
    protected void FightEnemyInRange(){
        
        enemyTargetCombatSystem
         = enemyTarget.GetComponent<CombatSystem>(); 
        float distance = Vector3.Distance(transform.position, enemyTarget.position);
        if (distance <= attackRange /*&& agent.velocity.magnitude <= 0.1f*/) {
            agent.isStopped = true;
            if (atkTimer <= 0f && agent.velocity.magnitude <= 0.05)  { 
                Attack(enemyTarget); 
            }
        }
        else{ 
            agent.isStopped = false;
            agent.SetDestination(enemyTarget.position); 
        }
    }


   
    
}
