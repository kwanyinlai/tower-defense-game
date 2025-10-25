using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;


// some sort of anchor point which they attempt to get close to always but if there are enemies in the way, they choose against it

public interface IPlayer
{
    
}
public abstract class PlayerTroopAI : TroopAI
{

    protected static List<GameObject> allPlayerTroops = new List<GameObject>(); 
    public static List<GameObject> AllPlayerTroops { get; }

    [Header("Control Attributes")]

    protected bool isUnderSelection = false;
    public bool IsUnderSelection { get{ return isUnderSelection; } set{ isUnderSelection = value; } }

    [SerializeField] protected GameObject commandingPlayer;
    public GameObject CommandingPlayer { get{ return commandingPlayer; } set{ commandingPlayer = value; } }
    [SerializeField] protected GameObject waypoint;
    public GameObject Waypoint { get{ return waypoint; } set{ waypoint = value; } } 
    [SerializeField] protected GameObject selectedCircle; // TODO: change build menu isBuilding to more descriptive
    public GameObject SelectedCircle { get { return selectedCircle; } set { selectedCircle = value; } }
    

    // Movement Attributes
    private Vector3 anchor; // for the future, not currently in use
    
    // Resource Attributes
    protected Dictionary<string, int> sellResources = new Dictionary<string, int>();

    // Abstract Methods 
    protected abstract void IntializeSellResources();


    protected virtual void Start()
    {
        super.Start();

        IntializeSellResources();
        HideCircle();
    }


    protected virtual void Update()
    {
        super.Update();

        // Events when player is controlling troop
        ControlTroop();
    }

    protected void ControlTroop() {
        // For when the player is selecting the troop, but TODO: maybe move this out of AI logic
        if(IsUnderSelection){
            ShowCircle();
        }
        else{
            HideCircle();
        }
    }


    protected void GoToWaypoint()
    {
        if (Vector3.Distance(waypoint.transform.position, gameObject.transform.position) <= 3f)
        {
            DeleteFromWaypoint();
            // pathIndicator.enabled = false;
        }
        else
        {
            agent.SetDestination(waypoint.transform.position);
            NavMeshPath path = new NavMeshPath();
            NavMesh.CalculatePath(transform.position, waypoint.transform.position, NavMesh.AllAreas, path);
            // pathIndicator.enabled = true;
            // pathIndicator.positionCount = path.corners.Length;
            // for (int i = 0; i < path.corners.Length; i++)
            // {
            //     pathIndicator.SetPosition(i, path.corners[i]);
            // }

        }
        // indicator towards waypoint
    }
    
    protected void Disengaged() {
        // Disengaged Mechanics; move to waypoint
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

    public void DeleteFromWaypoint()
    {
        if (waypoint != null)
        {
            waypoint.GetComponent<Waypoint>().allPlayerTroopsBound.Remove(gameObject); // TODO: getter setter this
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
        ;
        // behaviour for when selected
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

        selectedallPlayerTroops.Clear();
        foreach (var troop in allPlayerTroops)
        {
            Ray ray = new Ray(troop.transform.position, cam.position - troop.transform.position);
            Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.red, 10f);

            RaycastHit hit;


            if (Physics.Raycast(ray, out hit, 1000f))
            {
                if (hit.collider == collider)
                {
                    selectedallPlayerTroops.Add(troop);
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


    protected override void FindAndSetAllTargets()
    {
        enemyTarget = GetClosestEnemyInRange();
    }

    public Transform GetClosestEnemyInRange()
    {
        if (agent != null && agent.isOnNavMesh && agent.isStopped && enemyTarget != null)
        { // isStopped can't be called after dead but it is being called
            return enemyTarget;
        }
        GameObject closestEnemy = null;
        float closestDistance = aggroRange;

        // TODO:
        if (EnemyAI.getListOfEnemies().Count == 0)
        {
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

        if (closestEnemy == null)
        {
            return enemyTarget;
        }

        return closestEnemy.transform;
    }

    protected override void AddEntityToAliveList()
    {
        allPlayerTroops.add(gameObject);
    }
    
    public override void RemoveEntityFromAliveList()
    {
        allPlayerTroops.Remove(gameObject);
    }


    
}
