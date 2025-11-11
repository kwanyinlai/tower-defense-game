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
    public string TroopName { get { return troopName; } }
    [SerializeField] protected float maxSpeed = 3.5f;
    public float MaxSpeed { get { return maxSpeed; } }


    public Vector2 currVelocity = Vector2.zero;
    public float acceleration = 5f;

    [Header("Control Attributes")]

    // Combat Stats
    [SerializeField] protected Transform enemyTarget;
    public Transform EnemyTarget { get { return enemyTarget; } set { enemyTarget = value; } }

    [SerializeField] protected float aggroRange = 10.0f;
    public float AggroRange { get { return aggroRange; } set { aggroRange = value; } } // range which troop engages enemy

    // Combat Attributes
    protected TroopCombatSystem enemyTargetCombatSystem;
    public TroopCombatSystem EnemyTargetCombatSystem { get{ return enemyTargetCombatSystem; } }
    protected HashSet<string> exceptionBulletList = new HashSet<string> { "Troop" }; // i have no clue what this is
    protected TroopCombatSystem selfCombatSystem;
    public TroopCombatSystem SelfCombatSystem { get { return selfCombatSystem; } }

    // Movement Attributes
    private List<GridSector> highLevelPath;
    private GridNode localTargetNode;

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
        float speed = maxSpeed * (1 - selfCombatSystem.GetEffectStrength("slow") + selfCombatSystem.GetEffectStrength("haste")); 
        // TODO: probably put this in a function?
        if (enemyTarget != null)
        {
            MoveTowardsTarget(enemyTarget);
        }
       
    }


    protected void MoveTowardsTarget(Transform target)
    {
         
        GridManager gridManager = GridManager.Instance;

        GridNode currentNode = gridManager.NodeFromWorldPos(transform.position);

        if (highLevelPath == null)
        {
            highLevelPath = SectorManager.Instance.GenerateHighLevelSectorPath(
                currentNode.gridSector,
                localTargetNode.gridSector // TODO: maybe store as attribute in troopAI
            );
        }
       

        if (highLevelPath[0] == currentNode.gridSector)
        {
            highLevelPath.RemoveAt(0);
            if (highLevelPath.Count == 0)
            {
                localTargetNode = gridManager.NodeFromWorldPos(target.position);
            }
            else
            {
                if (highLevelPath.Count > 1)
                {
                    localTargetNode = currentNode.gridSector.GuessOptimalExitNode(
                        currentNode,
                        highLevelPath[0],
                        highLevelPath[1]
                    );
                }
                else
                {
                    localTargetNode = currentNode.gridSector.GuessOptimalExitNode(
                        currentNode,
                        highLevelPath[0]
                    );
                }
            }
        }

        Vector2 dirVector = currentNode.gridSector.QueryFlowField(currentNode, localTargetNode);

        // check whether the current sector is adjacent to the next target sector
        // if not, regenerate the path because we have veered off path
        if (!SectorManager.Instance.SectorAreNeighbours(currentNode.gridSector, highLevelPath[0]))
        {
            highLevelPath = SectorManager.Instance.GenerateHighLevelSectorPath(
                currentNode.gridSector,
                localTargetNode.gridSector // TODO: maybe store as attribute in troopAI
            );
            return;
        }

        // Steer


        Vector2 desiredVelocity = dirVector.normalized * maxSpeed;

        currVelocity = Vector2.MoveTowards(currVelocity, desiredVelocity, acceleration * Time.deltaTime);

        transform.position += (Vector3)(currVelocity * Time.deltaTime);

        if (currVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(currVelocity.y, currVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }


        

        // Vector2Int sectorLocalPos = new Vector2Int(
        //     gridCo
        // )
        
        // navMeshAgent.isStopped = false;
        // navMeshAgent.speed = maxSpeed * (1 - selfCombatSystem.GetEffectStrength("slow") + selfCombatSystem.GetEffectStrength("haste")); // diff function
        // navMeshAgent.SetDestination(target.position);
        // NavMeshPath path = new NavMeshPath();
        // NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, path);


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

    protected void FightEnemy()
    {
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
        if (troopState != TroopState.Retreating && enemyTarget != null)
        {
            float distance = Vector3.Distance(transform.position, enemyTarget.position);
            if (distance <= selfCombatSystem.AttackRange /*&& agent.velocity.magnitude <= 0.1f*/)
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

    public static List<GameObject> GetAllyEntitiesAliveList(GameObject referenceObject)
    {
        if (referenceObject.GetComponent<PlayerTroopAI>() != null)
        {
            return PlayerTroopAI.GetAllyEntitiesAliveList();
        }
        else
        {
            return EnemyTroopAI.GetAllyEntitiesAliveList();
        }
    }

    public static List<GameObject> GetEnemyEntitiesAliveList(GameObject referenceObject)
    {
        if (referenceObject.GetComponent<PlayerTroopAI>() != null)
        {
            return EnemyTroopAI.GetAllyEntitiesAliveList();
        }
        else
        {
            return PlayerTroopAI.GetAllyEntitiesAliveList();
        }
    }

}

