using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;
public abstract class EnemyAI : MonoBehaviour
{
    public static List<GameObject> enemies = new List<GameObject>(); 
    public float aggroRange = 10.0f;
    public GameObject baseTarget;
    public Transform troopTarget;
    public Transform barracksTarget;
    protected UnityEngine.AI.NavMeshAgent agent;
    protected CombatSystem targetStats;
    public float range = 5.0f;
    public int dmg = 10;
    protected float atkCooldown = 1.5f;
    public float atkTimer = 0f;
    public abstract void Attack(Transform target);
    
    void Start()
    {
        enemies.Add(gameObject);
    
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();


       
    }
 
    void Update()
    {

        if (baseTarget == null){
            Debug.Log("No base found!");
            baseTarget = GameObject.Find("base-manager").GetComponent<BaseManager>().GetBase();
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
            if (distance <= range)
            {
                agent.isStopped = true;
                if (atkTimer <= 0f)
                {
                    Attack(troopTarget);
                }
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(troopTarget.position);
            }
        }
        else if (baseTarget != null)
        {

            float distance = Vector3.Distance(agent.transform.position, baseTarget.transform.position);
            if (distance <= range)
            {
                agent.isStopped = true;
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
                agent.isStopped = false;
                agent.SetDestination(baseTarget.transform.position);
            }
        }
        else if (barracksTarget != null)
        {
            float distance = Vector3.Distance(agent.transform.position, barracksTarget.position);
            if (distance <= range)
            {
                agent.isStopped = true;
                if (atkTimer <= 0f)
                {
                    Attack(barracksTarget);
                    Debug.Log("attacking barracks");
                }
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(barracksTarget.position);
            }
        }
        else
        {
            agent.isStopped = true;
        }
        if (atkTimer > 0f) {
            atkTimer -= Time.deltaTime;
        } else
        {
            atkTimer = atkCooldown;
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
