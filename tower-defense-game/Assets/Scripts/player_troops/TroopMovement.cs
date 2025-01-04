using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class TroopMovement : MonoBehaviour
{
    public float aggroRange = 10.0f; // range which troop engages enemy
    public Transform enemyTarget;
    private BattleSystem targetStats;

    private NavMeshAgent agent;

    private Vector3 idleTransform;
    private Vector3 idleStartPos;
    public float idleRange = 10.0f;


    private bool inCombat = false;
    
    public float attackRange = 5.0f;
    public int dmg = 10;
    private float atkCooldown = 1.5f;
    private float atkTimer = 0f;
    public GameObject bulletPrefab; 


    public static List<GameObject> troops = new List<GameObject>();


    [SerializeField] private GameObject selectorCircle;
    public bool underSelection=false;


    public GameObject commandingPlayer;
    public GameObject waypoint; // maybe these shouldn't be public?
    


    public Vector3 getRandomPosition(Vector3 center, float idleRange)
    {
        Vector3 randomTransform;
        randomTransform.y = 0;
        randomTransform.x = Random.value;
        randomTransform.z = Random.value;
        randomTransform = randomTransform.normalized * Random.value * idleRange;
        return randomTransform;
    }


    void Start()
    {
        
        idleTransform = this.transform.position;
        idleStartPos = this.transform.position;
        agent = GetComponent<NavMeshAgent>();
        troops.Add(gameObject);
        DeactivateSelectingCircle();

    }


    void Update()
    {
        if (atkTimer > 0f) { atkTimer -= Time.deltaTime; }
        if(underSelection){
            ActivateSelectingCircle();
            
        }
        else{

            DeactivateSelectingCircle();
        }
        Transform copy = GetClosestEnemyInRange();
        if (copy != null)
        {
            inCombat = true;
            enemyTarget = copy;
        }
        else
        {   
            inCombat = false;  
            agent.isStopped=false; 
        }
       
        if (inCombat)
        {
            
            FightEnemyInRange();
        } 
        else {      
            if(underSelection){
               
                FollowCommander();
            }
            else{
                if(waypoint!=null){
                    GoToWaypoint();
                }
                else{
                    Idle();
                }

            }
            
            
            
        }


        
        
    }




    void Idle(){
        float dist = Mathf.Sqrt(Mathf.Pow(idleStartPos.x - transform.position.x, 2) + Mathf.Pow(idleStartPos.z - transform.position.z, 2));
        float dist2 = Mathf.Sqrt(Mathf.Pow(idleTransform.x - transform.position.x, 2) + Mathf.Pow(idleTransform.z - transform.position.z, 2));
        if (dist >= idleRange)
        {
            idleStartPos = transform.position;
        }
        if (dist >= idleRange || dist2 <= 3f)
        {
            idleTransform = idleStartPos + getRandomPosition(idleStartPos, idleRange);
            agent.SetDestination(idleTransform);
        }
    }

    void FollowCommander(){

        agent.SetDestination(commandingPlayer.transform.position); 


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
                bulletScript.SetTarget(enemyTarget);
            }
        }
        atkTimer = atkCooldown;
    }

  
    void FightEnemyInRange(){
        
        targetStats = enemyTarget.GetComponent<BattleSystem>(); 
        float distance = Vector3.Distance(transform.position, enemyTarget.position);
        if (distance <= attackRange && agent.velocity.magnitude <= 0.1f) {
            agent.isStopped = true;
            if (atkTimer <= 0f && agent.velocity.magnitude <= 0.05)  { 
                Attack(); 
            }
        }
        else{ 
            agent.isStopped = false;
            agent.SetDestination(enemyTarget.position); 
        }
    }


    Transform GetClosestEnemyInRange()
    {
        if (agent.isStopped && enemyTarget!=null){
            return enemyTarget;
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
    void GoToWaypoint(){
        if(Vector3.Distance(waypoint.transform.position, gameObject.transform.position) <= 5f){
            DeleteFromWaypoint();
        }
        else{
            agent.SetDestination(waypoint.transform.position); 
        }

        // indicator towards waypoint
        
    }

    public void DeleteFromWaypoint(){
        if (waypoint!=null){
            waypoint.GetComponent<Waypoint>().troopsBound.Remove(gameObject);
            waypoint = null;
        }
        
    }


    /*
    public static void inSelection(MeshCollider collider, Transform cam){

        selectedTroops.Clear();
        foreach (var troop in troops)
        {
            Ray ray = new Ray(troop.transform.position, cam.position - troop.transform.position);
            Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.red, 10f);

            RaycastHit hit;


            if (Physics.Raycast(ray, out hit, 1000f))
            {
                if (hit.collider == collider)
                {
                    selectedTroops.Add(troop);
                    troop.GetComponent<PlayerMovement>().ActivateSelectingCircle();
                }
            }
            else
            {
                Debug.Log(troop.name + " ray did not hit anything.");
            }
        }

    }
    */

    
    void ActivateSelectingCircle(){
        selectorCircle.transform.localScale= new Vector3(1,0.1f,1);
    }

    void DeactivateSelectingCircle(){
        selectorCircle.transform.localScale= new Vector3(0f,0f,0f);
    }
    

    

}


