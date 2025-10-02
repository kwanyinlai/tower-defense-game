using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;


// some sort of anchor point which they attempt to get close to always but if there are enemies in the way, they choose against it


public abstract class TroopAI : MonoBehaviour
{

    public static List<GameObject> troops = new List<GameObject>();

    [Header("Troop Attributes")]
    [SerializeField] protected string troopName;
    [SerializeField] protected GameObject troopModel;
    [SerializeField] protected float maxSpeed = 3.5f;

    [Header("Combat Attributes")]
    [SerializeField] protected float attackRange = 5.0f ;
    [SerializeField] protected int dmg = 10;
    [SerializeField] protected float aggroRange = 10.0f; // range which troop engages enemy
    [SerializeField] protected Transform enemyTarget;

    [Header("Control Attributes")]
    protected bool isUnderSelection = false;

    public bool IsUnderSelection
    {
        get => isUnderSelection;
        set => isUnderSelection = value;
    }

    [SerializeField] protected GameObject commandingPlayer;
    [SerializeField] protected GameObject waypoint; // maybe these shouldn't be public?
    [SerializeField] protected GameObject selectedCircle;
    

    // PRIVATE AND PROTECTED ATTRIBUTES 

    // Combat Stats
    protected float atkCooldown = 1.5f;
    protected float atkTimer = 0f;

    // Combat Attributes
    protected CombatSystem targetStats;
    protected HashSet<string> exceptionBulletList = new HashSet<string>{"Troop"};
    protected CombatSystem combatSystem;
    protected bool inCombat = false;
    public bool InCombat{
        get{ return inCombat; }
    }

    // Movement Attributes
    private LineRenderer pathIndicator;
    protected NavMeshAgent agent;

    private Vector3 anchor;
    
    // Resource Attributes
    protected Dictionary<string, int> sellResources = new Dictionary<string, int>();

    // Abstract Methods 
    protected abstract void IntializeSellResources();
    public abstract void Attack(Transform target);


    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        troops.Add(gameObject);
        pathIndicator = GetComponent<LineRenderer>();
        pathIndicator.enabled = false;
        combatSystem = gameObject.GetComponent<CombatSystem>();
        agent.speed = maxSpeed;

        IntializeSellResources();
        HideCircle();

    }


    protected virtual void Update()
    {
        if (atkTimer > 0f) { atkTimer -= Time.deltaTime; }

        // Events when player is controlling troop
        ControlTroop();

        // Decide Whether to Engage In Combat
        DecideState();

       // Combat Mechanics
        HandleCombat();
    }

    protected void HandleCombat() {
        if (inCombat)
        {
            FightEnemy();    
        } 
        else 
        {
            Disengaged();
        }
    }

    protected void ControlTroop() {
        if(IsUnderSelection){
            ShowCircle();
        }
        else{
            HideCircle();
        }
    }

    protected void DecideState() {
        Transform copy = GetClosestEnemyInRange();
        if (copy != null)
        {
            // Start Combat & Find Target
            inCombat = true;
            enemyTarget = copy;
        }
        else
        {   
            // Stop Combat + Movement
            inCombat = false;  
            agent.isStopped=false; 
        }
    }

    protected void FightEnemy() {
        agent.speed = maxSpeed * (1 - combatSystem.GetEffectStrength("slow"));
        FightEnemyInRange();
    }

    protected void Disengaged() {
        // Disengaged Mechanics
        agent.speed = maxSpeed * (1 - combatSystem.GetEffectStrength("slow"));
        AntiClustering();
        if(IsUnderSelection){
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

    protected void AntiClustering() // simple algorithm to avoid clustering while idling
    {
        Vector3 awayDir = Vector3.zero;
        Collider[] collisions = Physics.OverlapSphere(transform.position, 1.5f);
        foreach (Collider collider in collisions)
        {
            if (collider.gameObject != gameObject && collider.CompareTag("Troop"))
            {
                awayDir = transform.position - collider.transform.position;
            }
            
        }
        transform.position += awayDir.normalized * agent.speed * Time.deltaTime;
    }


  
    protected void FightEnemyInRange(){
        
        targetStats = enemyTarget.GetComponent<CombatSystem>(); 
        float distance = Vector3.Distance(transform.position, enemyTarget.position);
        if (distance <= attackRange /*&& agent.velocity.magnitude <= 0.1f*/) {
            agent.isStopped = true;
            if (atkTimer <= 0f && agent.velocity.magnitude <= 0.05)  { 
                Attack(enemyTarget); 
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

    protected void GoToWaypoint(){
        if(Vector3.Distance(waypoint.transform.position, gameObject.transform.position) <= 3f){
            DeleteFromWaypoint();
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

    public void Selected()
    {
    }

    public void SetSellResources(Dictionary<string, int> resources)
    {
        sellResources = resources;
    }

    public Dictionary<string, int> GetSellResources()
    {
        return sellResources;
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

    /*
    public Vector3 GetRandomPosition(Vector3 center, float idleRange)
    {
        Vector3 randomTransform;
        randomTransform.y = 0;
        randomTransform.x = Random.value;
        randomTransform.z = Random.value;
        randomTransform = randomTransform.normalized * Random.value * idleRange;
        return randomTransform;
    }
    */
    /* protected void Idle(){
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
    */

    
}
