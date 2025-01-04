using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;



public class EnemyMovement : MonoBehaviour
{
    public float aggroRange = 10.0f;
    public Transform baseTarget;
    public Transform troopTarget;
    private NavMeshAgent agent;
    private BattleSystem targetStats;
    public float range = 5.0f;
    public int dmg = 10;
    private float atkCooldown = 1.5f;
    public float atkTimer = 0f;
    public static List<GameObject> enemies = new List<GameObject>(); 
    public GameObject bulletPrefab;


 

     void Start()
    {
        enemies.Add(gameObject);
    
        agent = GetComponent<NavMeshAgent>();


        if (baseTarget != null)
        {
            targetStats = baseTarget.GetComponent<BattleSystem>(); 
        }
       
    }

    void Update()
    {

        troopTarget = GetClosestEnemyInRange();
        if (troopTarget != null)
        {
            targetStats = troopTarget.GetComponent<BattleSystem>(); 
            float distance = Vector3.Distance(transform.position, troopTarget.position);
            if (distance <= range){ 
                agent.isStopped = true;
                if (atkTimer <= 0f){
                    Attack(troopTarget);
                }
            }
            else{ 
                agent.isStopped = false;
                agent.SetDestination(troopTarget.position); 
            }
        }
        else if (baseTarget != null) {
            
            float distance = Vector3.Distance(agent.transform.position, baseTarget.position);
            if (distance <= range) 
            {
                agent.isStopped = true;
                if( agent.velocity.magnitude <= 0.1f){
                    if (atkTimer <= 0f){
                        Attack(baseTarget);
                    }
                }
               
            }
            else { 
                agent.isStopped = false; 
                agent.SetDestination(baseTarget.position);
            }
        }
        if (atkTimer > 0f) {
            atkTimer -= Time.deltaTime;
        } else
        {
            atkTimer = atkCooldown;
        }
    }


    void Attack(Transform bulletTarget)
    {
        
        Vector3 bulletPos = transform.position;
        Collider collider = this.GetComponent<Collider>();
        bulletPos.y += collider.bounds.size.y * (0.75f);
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletPos, transform.rotation);
            BulletScript bulletScript = bullet.GetComponent<BulletScript>();
            if (bulletScript != null)
            {
                bulletScript.SetTarget(bulletTarget);
            }
        }
        atkTimer = atkCooldown;
    }


    Transform GetClosestEnemyInRange()
    {
        if (agent.isStopped && troopTarget != null)
        {
            return troopTarget;
        }
        GameObject closestEnemy = null;
        float closestDistance = aggroRange;
        if (TroopMovement.troops.Count == 0)
        {
            return null;
        }
        foreach (var enemy in TroopMovement.troops)
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



}
