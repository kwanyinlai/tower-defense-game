using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public Transform target;          
    private NavMeshAgent agent;      
    public float range = 5.0f;   
    private BattleSystem targetStats; 
    public GameObject bulletPrefab;   
    private float attackCooldown = 2.0f;  
    private float attackTimer = 0f;  

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (target != null)
        {
            targetStats = target.GetComponent<BattleSystem>(); 
        }
    }

    void Update()
    {
        if (target != null && targetStats != null)
        {

            agent.SetDestination(target.position);

            
            float distance = Vector3.Distance(agent.transform.position, target.position);
            Debug.Log("distance: " + distance);
            Debug.Log("range: " + range);

            if (distance <= range && agent.velocity.magnitude <= 0.1f) 
            {
                Debug.Log("im iin");

                agent.isStopped = true;

                
                if (attackTimer <= 0f)
                {
                    Attack();
                }
            }
            else
            {
                agent.isStopped = false;
                
            }

          
            attackTimer -= Time.deltaTime;
        }
    }


    void Attack(){

        Projectile();
        
        attackTimer = attackCooldown;
    }


    void Projectile()
    {
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            BulletScript bulletScript = bullet.GetComponent<BulletScript>();
            
            bulletScript.SetTarget(target);
            
        }
    }

}
