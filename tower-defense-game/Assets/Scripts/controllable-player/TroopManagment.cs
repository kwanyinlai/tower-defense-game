using UnityEngine;
using System.Collections.Generic;

// TODO:
// SOME SORT OF COLOR CODING MECHANIC FOR LOCAL CO-OP WITH THE UNIT SELECTION 

public class TroopManagment : MonoBehaviour
{
    [SerializeField] private GameObject selectorCircle;
    public List<GameObject> selectedTroops = new List<GameObject>();
    public float selectionRadius = 100f;
    private bool placingWaypoint = false;
    [SerializeField] private GameObject waypoint;
    [SerializeField] private GameObject outlineWaypoint;
    public bool managingTroops = false;
    // private float menuTime; // for timing how long menu is up for before it closes
    [SerializeField] private LayerMask selectableLayer;
    


    
    void Start()
    {
        managingTroops = false;
        HideWaypointOutline();
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && selectedTroops.Count > 1)
        {
            StopAndClearSelecting();
        }
        if (!gameObject.GetComponent<BuildMode>().isBuilding &&
            GetComponent<CharacterMovement>().IsControllable()){

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!(Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) && selectedTroops.Count > 1)
                {
                    StopAndClearSelecting();
                }

                managingTroops = true;
            }
               
                
                
            

            if(placingWaypoint){
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
                        placingWaypoint = false;
                        break;
                    case bool _ when Input.GetKeyDown(KeyCode.Escape):
                        ValidInput(); // do nothing just close menu
                        placingWaypoint = false;
                        break;
                }
                
                
            }
            else{
                HideWaypointOutline();
            }

            if (managingTroops)
            {
                // SelectTroops();
                RaySelect();
                managingTroops = false;
                
            }
        }

    }



    void ShowWaypointOutline(){
      
        outlineWaypoint.transform.localScale = new Vector3(1f, 1f, 1f);
        placingWaypoint = true;
        

    }

    void HideWaypointOutline(){
        outlineWaypoint.transform.localScale= new Vector3(0f,0f,0f);

    }

    void StopAndClearSelecting(){
        foreach(GameObject troop in selectedTroops){
            TroopAI troopScript = troop.GetComponent<TroopAI>();
            troopScript.underSelection=false;
            troopScript.commandingPlayer=null;

        }
        managingTroops = false;
        selectedTroops.Clear();
    }

    void SelectTroops(){
                
        foreach (GameObject troop in TroopAI.troops)
        {
            Vector3 playerPosition = transform.position;
            Vector3 troopPosition = troop.transform.position;
            playerPosition.y = 0f;
            troopPosition.y = 0f;

            TroopAI troopScript = troop.GetComponent<TroopAI>();
            if (Vector3.Distance(playerPosition, troopPosition) <= selectionRadius && 
                    !selectedTroops.Contains(troop) && 
                   troopScript.underSelection==false)
            {
                
                troopScript.underSelection=true;
                troopScript.commandingPlayer = gameObject;
                troopScript.DeleteFromWaypoint();
                selectedTroops.Add(troop);
            }
        }

        Debug.Log("Selected Troops: " + selectedTroops.Count);
    }




    void ValidInput(){
        placingWaypoint = false;
        StopAndClearSelecting();
        
    }


    void SetWaypoint(){
        GameObject deployedPoint = Instantiate(waypoint, transform.position + transform.rotation * new Vector3(0f,0f,3f), transform.rotation);
        foreach (GameObject troop in selectedTroops)
        {
           
            TroopAI troopScript = troop.GetComponent<TroopAI>();
            troopScript.commandingPlayer = null;
            troopScript.DeleteFromWaypoint();
            troopScript.waypoint = deployedPoint;
            deployedPoint.GetComponent<Waypoint>().troopsBound.Add(troop);
        }

        StopAndClearSelecting();
    }

    void RaySelect()
    {
        RaycastHit hit;
        bool destroyedRay = false;
        Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
        if (Physics.Raycast(transform.position + new Vector3(0f, 2f, 0f), forward, out hit, 10000f, selectableLayer) && !destroyedRay)
        {
            TroopAI troop = hit.collider.GetComponent<TroopAI>();
            if (troop != null && !selectedTroops.Contains(troop.gameObject))
            {
                troop.underSelection = true;
                troop.commandingPlayer = gameObject;
                selectedTroops.Add(troop.gameObject);
                troop.ShowCircle();
                destroyedRay = true;
                ShowWaypointOutline();
                return;
            }
        }
    }
}
