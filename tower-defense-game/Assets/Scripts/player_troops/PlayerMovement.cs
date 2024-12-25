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
    public static List<GameObject> selectedTroops = new List<GameObject>();   
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
            if (distance <= range && agent.velocity.magnitude <= 0.1f) {
                agent.isStopped = true;
                if (atkTimer <= 0f && agent.velocity.magnitude <= 0.05)  { Attack(); }
            }
            else{ 
                 agent.isStopped = false;
                agent.SetDestination(target.position); 
            }
        }
        if (atkTimer > 0f) { atkTimer -= Time.deltaTime; }

        if (selectedTroops.Count!=0){
            // selected troop script goes here
            return;
    


        }
    }

    void Attack()
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
                bulletScript.SetTarget(target);
            }
        }
        atkTimer = atkCooldown;
    }

  

    Transform GetClosestEnemyInRange()
    {
        if (agent.isStopped && target!=null){
            return target;
        }
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

    public static void inSelection(MeshCollider collider, Transform cam){

        selectedTroops.Clear();
        Debug.Log("Nums troops: "+troops.Count);
        foreach (var troop in troops)
        {
            Ray ray = new Ray(troop.transform.position, cam.position - troop.transform.position);
            Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.red, 10f);

            RaycastHit hit;


            if (Physics.Raycast(ray, out hit, 1000f))
            {
                if (hit.collider == collider)
                {
                    Debug.Log("Mesh collider hit");
                    selectedTroops.Add(troop);
                }
            }
            else
            {
                Debug.Log(troop.name + " ray did not hit anything.");
            }
        }

    }

}






