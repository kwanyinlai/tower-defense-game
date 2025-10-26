using UnityEngine;
using System.Collections.Generic;

// TODO:
// SOME SORT OF COLOR CODING MECHANIC FOR LOCAL CO-OP WITH THE UNIT SELECTION 

public class TroopManagment : MonoBehaviour
{
    [SerializeField] private GameObject selectorCircle;
    private List<GameObject> selectedTroops = new List<GameObject>();
    public List<GameObject> SelectedTroops{ get; }
    private float selectionRadius = 100f;
    private bool isPlacingWaypoint = false; // change flags to replace with something else
    [SerializeField] private GameObject waypoint;
    [SerializeField] private GameObject waypointOutline;
    private bool isManagingTroops = false;
    // private float menuTime; // for timing how long menu is up for before it closes
    [SerializeField] private LayerMask selectableLayer;
    private Player playerData;
    
    public bool IsManagingTroops {get; private set;}

    
    void Start()
    {
        isManagingTroops = false;
        playerData = GetComponent<Player>();
        HideWaypointOutline();
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && selectedTroops.Count > 1)
        {
            StopAndClearSelecting();
        }
        if (playerData.CurrentState == Player.PlayerStates.ControllingCharacter){

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!(Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) && selectedTroops.Count > 1)
                {
                    StopAndClearSelecting();
                }

                isManagingTroops = true;
            }
               
                
                
            

            if(isPlacingWaypoint){
                // timeMenu += time.deltaTime;
                // if (timer >= timeLimit){

                // }


                switch(true){

                    /*case bool _ when Input.GetKeyDown(KeyCode.Alpha1):
                        SetWaypoint();
                        ValidMenuSelection();
                        break;
                    case bool _ when Input.GetKeyDown(KeyCode.Alpha2):
                        // other command
                        ValidMenuSelection();
                        break;
                    case bool _ when Input.GetKeyDown(KeyCode.Alpha3):
                        // other command
                        ValidMenuSelection();
                        break;
                    case bool _ when Input.GetKeyDown(KeyCode.Alpha4):
                        // other command
                        ValidMenuSelection();
                        break;
                        */
                    case bool _ when Input.GetKeyDown(KeyCode.Return):
                        SetWaypoint();
                        ValidInput();
                        isPlacingWaypoint = false;
                        break;
                    case bool _ when Input.GetKeyDown(KeyCode.Escape):
                        ValidInput(); // do nothing just close menu
                        isPlacingWaypoint = false;
                        break;
                }
                
                
            }
            else{
                HideWaypointOutline();
            }

            if (isManagingTroops)
            {
                // SelectTroops();
                RaySelectAlliedTroops();
                isManagingTroops = false;
                
            }
        }

    }



    void ShowWaypointOutline(){
      
        waypointOutline.transform.localScale = new Vector3(1f, 1f, 1f);
        isPlacingWaypoint = true;
        

    }
    // TODO: make default reference in troopmmanagment directly call prefab, why is flag outline disappearing?
    void HideWaypointOutline(){
        waypointOutline.transform.localScale= new Vector3(0f,0f,0f);

    }

    void StopAndClearSelecting(){
        foreach(GameObject troop in selectedTroops){
            PlayerTroopAI troopAI = troop.GetComponent<PlayerTroopAI>();
            troopAI.IsUnderSelection=false;
            troopAI.CommandingPlayer=null;

        }
        isManagingTroops = false;
        selectedTroops.Clear();
    }

    void SelectTroops(){
                
        foreach (GameObject troop in PlayerTroopAI.AllPlayerTroops)
        {
            Vector3 playerPosition = transform.position;
            Vector3 troopPosition = troop.transform.position;
            playerPosition.y = 0f;
            troopPosition.y = 0f;
            
            PlayerTroopAI troopAI = troop.GetComponent<PlayerTroopAI>();
            Debug.Log("Troop AI isUnderSelection: " + troopAI.IsUnderSelection);
            if (!selectedTroops.Contains(troop) && troopAI.IsUnderSelection == false)
            {
                
                troopAI.IsUnderSelection = true;
                Debug.Log("Troop AI isUnderSelection: " + troopAI.IsUnderSelection);
                troopAI.CommandingPlayer = gameObject;
                troopAI.DeleteFromWaypoint();
                selectedTroops.Add(troop);
            }
        }

        Debug.Log("Selected Troops: " + selectedTroops.Count);
    }




    void ValidInput(){
        isPlacingWaypoint = false;
        StopAndClearSelecting();
        
    }


    void SetWaypoint(){
        GameObject deployedPoint = null;
        if (Waypoint.FindNearestWaypoint(transform.position + transform.rotation * new Vector3(0f,0f,3f)) == null){
            deployedPoint = Instantiate(waypoint, transform.position + transform.rotation * new Vector3(0f,0f,3f), transform.rotation);
        }
        else{
            deployedPoint = Waypoint.FindNearestWaypoint(transform.position + transform.rotation * new Vector3(0f,0f,3f));
        }
        
        foreach (GameObject troop in selectedTroops)
        {
           
            PlayerTroopAI troopAI = troop.GetComponent<PlayerTroopAI>();
            troopAI.CommandingPlayer = null;
            troopAI.DeleteFromWaypoint();
            troopAI.Waypoint = deployedPoint;
            deployedPoint.GetComponent<Waypoint>().troopsBound.Add(troop);
        }

        StopAndClearSelecting();
    }

    void RaySelectAlliedTroops()
    {
        RaycastHit hit;
        bool destroyedRay = false;
        Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
        if (Physics.Raycast(transform.position + new Vector3(0f, 2f, 0f), forward, out hit, 10000f, selectableLayer) && !destroyedRay)
        {
            PlayerTroopAI troop = hit.collider.GetComponent<PlayerTroopAI>();
            if (troop != null && !selectedTroops.Contains(troop.gameObject))
            {
                troop.IsUnderSelection = true;
                troop.CommandingPlayer = gameObject;
                selectedTroops.Add(troop.gameObject);
                troop.ShowCircle();
                destroyedRay = true;
                ShowWaypointOutline();
                return;
            }
        }
    }
}
