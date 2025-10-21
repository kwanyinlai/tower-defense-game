using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;
public abstract class EnemyAI : TroopAI
{
    public static List<GameObject> enemies = new List<GameObject>();

    
    protected virtual void Start()
    {
        super.Start();
        enemies.Add(gameObject);
    }


    protected void MoveTowardsTarget(Transform target)
    { 
        agent.isStopped = false;
        agent.speed = maxSpeed * (1 - combatSystem.GetEffectStrength("slow") + combatSystem.GetEffectStrength("haste"));
        agent.SetDestination(target.position);
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, path);
        // pathIndicator.enabled = true;
        // pathIndicator.positionCount = path.corners.Length;

        // for (int i = 0; i < path.corners.Length; i++)
        // {
        //     pathIndicator.SetPosition(i, path.corners[i]);
        // }
    }

    protected void StopMoving() // what is this for
    {
        agent.isStopped = true;
        // pathIndicator.enabled = false;
    }


    protected virtual void Update()
    {
        super.Update();
        
        FindDefaultTargets();
        FindTargets();
    }

    protected void FindDefaultTargets()
    {
        // Find Targets
        if (baseTarget == null)
        {
            Debug.Log("No base found!");
            baseTarget = BaseManager.Instance.GetBase();
        }
        else
        {
            targetStats = baseTarget.GetComponent<CombatSystem>();
        }
    }

    protected void Disengaged()
    {
        ;
    }
    
    protected void FindTargets() {
        // Get Targets
        troopTarget = GetClosestEnemyInRange();
        barracksTarget = GetClosestBarracksInRange();
    }


    protected void HandleCombat() {
        if (baseTarget == null){
            Debug.Log("No base found!");
            baseTarget = BaseManager.Instance.GetBase();
        }
        else{
            targetStats = baseTarget.GetComponent<CombatSystem>(); 
        }
        
        troopTarget = GetClosestEnemyInRange();
        barracksTarget = GetClosestBarracksInRange();
        
        if (troopTarget != null)
        {
            targetStats = troopTarget.GetComponent<CombatSystem>();
            float distance = Vector3.Distance(transform.position, troopTarget.position);
            if (distance <= attackRange)
            {
                StopMoving();
                if (atkTimer <= 0f)
                {
                    Attack(troopTarget);
                }
            }
            else
            {
            MoveTowardsTarget(troopTarget);
            }
        }
        else if (baseTarget != null)
        {

            float distance = Vector3.Distance(agent.transform.position, baseTarget.transform.position);
            if (distance <= attackRange)
            {
                StopMoving();
                if (agent.velocity.magnitude <= 0.1f)
                {
                    if (atkTimer <= 0f)
                    {
                        Attack(baseTarget.transform);
                    }
                }

            }
            else
            {
                MoveTowardsTarget(baseTarget.transform);
            }
        }
        else if (barracksTarget != null)
        {
            float distance = Vector3.Distance(agent.transform.position, barracksTarget.position);
            if (distance <= attackRange)
            {
                StopMoving();
                if (atkTimer <= 0f)
                {
                    Attack(barracksTarget);
                    Debug.Log("attacking barracks");
                }
            }
            else
            {
                MoveTowardsTarget(barracksTarget);
            }
        }
        else
        {
            StopMoving();
        }
    }

    public Transform GetClosestEnemyInRange()
    {
        if (agent.isStopped && troopTarget != null)
        {
            return troopTarget;
        }
        GameObject closestEnemy = null;
        float closestDistance = aggroRange;
        if (TroopAI.troops.Count == 0)
        {
            return null;
        }
        foreach (var enemy in TroopAI.troops)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance <= aggroRange && distance < closestDistance)
            {
                closestEnemy = enemy;
                closestDistance = distance;
            }
        }
        if (closestEnemy == null) { return null; }
        return closestEnemy.transform;
    }

    
    public Transform GetClosestBarracksInRange()
    {
        if (barracksTarget != null)
        {
            return barracksTarget;
        }
        GameObject closestBarrack = null;
        float closestDistance = aggroRange;
        if (StructureBattleSystem.barracks.Count == 0)
        {
            return null;
        }
        foreach (var barrack in StructureBattleSystem.barracks)
        {
            float distance = Vector3.Distance(transform.position, barrack.transform.position);

            if (distance <= aggroRange && distance < closestDistance)
            {
                closestBarrack = barrack;
                closestDistance = distance;
            }
        }
        if (closestBarrack == null) { return null; }
        return closestBarrack.transform;
    }
}
