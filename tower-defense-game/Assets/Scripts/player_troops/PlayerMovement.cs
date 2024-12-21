using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    public float aggroRange = 10.0f;
    public Transform target;
    private NavMeshAgent agent;
    private BattleSystem targetStats;
    public float range = 5.0f;
    public int dmg = 10;  
    private float atkCooldown = 1.5f;  
    private float atkTimer = 0f;      
    public static LinkedList<GameObject> troops = new LinkedList<GameObject>();   
    public GameObject bulletPrefab; 

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        troops.AddLast(gameObject);
    }

    void Update()
    {
        target = GetClosestEnemyInRange();
        if (target != null)
        {
            targetStats = target.GetComponent<BattleSystem>(); 
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance <= aggroRange) {
                agent.isStopped = true;
                if (atkTimer <= 0f)  { Attack(); }
            }
            else{ agent.SetDestination(target.position); }
        }
        if (atkTimer > 0f) { atkTimer -= Time.deltaTime; }
    }

    void Attack()
    {
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            BulletScript bulletScript = bullet.GetComponent<BulletScript>();
            if (bulletScript != null)
            {
                bulletScript.SetTarget(target);
            }
        }
        atkTimer = atkCooldown;  
    }

  

    Transform GetClosestEnemyInRange()
    {
        GameObject closestEnemy = null;
        float closestDistance = aggroRange;
        if (EnemyMovement.enemies.Count == 0 ){
            return null;
        }
        foreach (var enemy in EnemyMovement.enemies)
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






