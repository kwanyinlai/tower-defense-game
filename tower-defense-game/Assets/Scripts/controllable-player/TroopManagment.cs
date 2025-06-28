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
        if (!gameObject.GetComponent<BuildMode>().isBuilding &&
            GetComponent<CharacterMovement>().IsControllable()){
            
            if(Input.GetKey(KeyCode.Space)){
                managingTroops = true;
            }
            else{
                ShowWaypointOutline();
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
                        break;
                    case bool _ when Input.GetKeyDown(KeyCode.Space):
                        placingWaypoint = false; // close menu but keep selecting
                        break;
                    case bool _ when Input.GetKeyDown(KeyCode.Escape):
                        ValidInput(); // do nothing just close menu
                        break;
                }
                
                
            }
            else{
                HideWaypointOutline();
            }

            if(managingTroops){
                // SelectTroops();
                RaySelect();
            }
        }

    }



    void ShowWaypointOutline(){
        if (selectedTroops.Count > 0)
        {
            outlineWaypoint.transform.localScale = new Vector3(1f, 1f, 1f);
            placingWaypoint = true;
        }
        else
        {
            managingTroops = false;
        }
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
        selectedTroops.Clear();
        managingTroops = false;
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
            troopScript.waypoint = deployedPoint;
            deployedPoint.GetComponent<Waypoint>().troopsBound.Add(troop);
        }
    }

    void RaySelect()
    {
        RaycastHit hit;
        Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
        Debug.DrawRay(transform.position + new Vector3(0f, 2f, 0f), forward, Color.red, 1000.0f);
        if (Physics.Raycast(transform.position + new Vector3(0f, 2f, 0f), forward, out hit, 10000f, selectableLayer))
        {
            TroopAI troop = hit.collider.GetComponent<TroopAI>();
            Debug.Log("hit something");
            if (troop != null)
            {
                troop.underSelection = true;
                troop.commandingPlayer = gameObject;
                troop.DeleteFromWaypoint();
                troop.Selected();
                selectedTroops.Add(troop.gameObject);
            }
        }
    }
}
