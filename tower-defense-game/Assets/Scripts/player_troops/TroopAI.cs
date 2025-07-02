using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public abstract class TroopAI : MonoBehaviour
{
    public float aggroRange = 10.0f; // range which troop engages enemy
    public Transform enemyTarget;
    protected CombatSystem targetStats;

    protected NavMeshAgent agent;

    protected Vector3 idleTransform;
    protected Vector3 idleStartPos;
    public float idleRange = 10.0f;


    protected bool inCombat = false;
    public bool InCombat{
        get{ return inCombat; }
    }
    
    public float attackRange = 5.0f;
    public int dmg = 10;
    protected float atkCooldown = 1.5f;
    protected float atkTimer = 0f;
    protected HashSet<string> exceptionBulletList = new HashSet<string>{"Troop"};

    private LineRenderer pathIndicator;


    public static List<GameObject> troops = new List<GameObject>();


    public bool underSelection=false;


    public GameObject commandingPlayer;
    public GameObject waypoint; // maybe these shouldn't be public?
    public abstract void Attack();

    [SerializeField] private GameObject selectedCircle;
    


    public Vector3 GetRandomPosition(Vector3 center, float idleRange)
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
        HideCircle();
        pathIndicator = GetComponent<LineRenderer>();
        pathIndicator.enabled = false;

    }


    void Update()
    {
        if (atkTimer > 0f) { atkTimer -= Time.deltaTime; }
        if(underSelection){
            ;
        }
        else{
            HideCircle();
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

                ;
            }
            else{
                if(waypoint!=null){
                    GoToWaypoint();
                    HideCircle();
                    
                }
                else{
                    ;
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
            idleTransform = idleStartPos + GetRandomPosition(idleStartPos, idleRange);
            agent.SetDestination(idleTransform);
        }
    }


  
    void FightEnemyInRange(){
        
        targetStats = enemyTarget.GetComponent<CombatSystem>(); 
        float distance = Vector3.Distance(transform.position, enemyTarget.position);
        if (distance <= attackRange /*&& agent.velocity.magnitude <= 0.1f*/) {
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


    public Transform GetClosestEnemyInRange()
    {

        if (agent!=null && agent.isOnNavMesh && agent.isStopped && enemyTarget!=null){ // isStopped can't be called after dead but it is being called
            return enemyTarget;
        }
        GameObject closestEnemy = null;
        float closestDistance = aggroRange;
        if (EnemyAI.enemies.Count == 0 ){
            return null;
        }

        foreach (var enemy in EnemyAI.enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            
            if (distance <= aggroRange && distance < closestDistance)
            {
                closestEnemy = enemy;
                closestDistance = distance;
            }
        }
        
        if (closestEnemy == null) {
            return enemyTarget; 
        }

        return closestEnemy.transform;
    }

    void GoToWaypoint(){
        if(Vector3.Distance(waypoint.transform.position, gameObject.transform.position) <= 3f){
            DeleteFromWaypoint();
            Debug.Log("waypoint test");
            pathIndicator.enabled = false;
        }
        else{
            agent.SetDestination(waypoint.transform.position);
            NavMeshPath path = new NavMeshPath();
            NavMesh.CalculatePath(transform.position, waypoint.transform.position, NavMesh.AllAreas, path);
            pathIndicator.enabled = true;
            pathIndicator.positionCount = path.corners.Length;
            for (int i = 0; i < path.corners.Length; i++)
            {
                pathIndicator.SetPosition(i, path.corners[i]);
            }

        }

        // indicator towards waypoint
        
    }

    public void DeleteFromWaypoint()
    {
        
        if (waypoint!=null){
            waypoint.GetComponent<Waypoint>().troopsBound.Remove(gameObject);
            waypoint = null;
        }
        
    }

    public void ShowCircle()
    {
        selectedCircle.SetActive(true);
    }

    public void HideCircle()
    {
        selectedCircle.SetActive(false);
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


    public void Selected()
    {
        Debug.Log("hello");
    }

    
}
