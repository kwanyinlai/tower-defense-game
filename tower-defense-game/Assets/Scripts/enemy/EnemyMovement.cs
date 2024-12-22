using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;



public class EnemyMovement : MonoBehaviour
{
    public float aggroRange = 10.0f;
    public Transform target;
    public Transform enemy_target;
    private NavMeshAgent agent;
    private BattleSystem targetStats;
    public float range = 5.0f;
    public int dmg = 10;
    private float atkCooldown = 1.5f;
    private float atkTimer = 0f;
    public static LinkedList<GameObject> enemies = new LinkedList<GameObject>(); 
    public GameObject bulletPrefab;

 

     void Start()
    {
        enemies.AddLast(gameObject);
        agent = GetComponent<NavMeshAgent>();
        if (target != null)
        {
            targetStats = target.GetComponent<BattleSystem>(); 
        }
       
    }

    void Update()
    {
        
        enemy_target = GetClosestEnemyInRange();
        if (enemy_target != null)
        {
            targetStats = enemy_target.GetComponent<BattleSystem>(); 
            float distance = Vector3.Distance(transform.position, enemy_target.position);
            if (distance <= range){ 
                agent.isStopped = true;
                if (atkTimer <= 0f){ Attack(enemy_target);}}
            else{ 
                agent.isStopped = false;
                agent.SetDestination(enemy_target.position); 
                }
        }
        else if (target != null) {
            agent.SetDestination(target.position);
            float distance = Vector3.Distance(agent.transform.position, target.position);
            if (distance <= range && agent.velocity.magnitude <= 0.1f) 
            {
                agent.isStopped = true;
                if (atkTimer <= 0f){  Attack(target);}
            }
            else { agent.isStopped = false; }
        }
        if (atkTimer > 0f) { atkTimer -= Time.deltaTime; }
    }


    void Attack(Transform target)
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
        if (agent.isStopped && enemy_target!=null){
            return enemy_target;
        }
        GameObject closestEnemy = null;
        float closestDistance = aggroRange;
        if (PlayerMovement.troops.Count == 0 ){
            return null;
        }
        foreach (var enemy in PlayerMovement.troops)
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
