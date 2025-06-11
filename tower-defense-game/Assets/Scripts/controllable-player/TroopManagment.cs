using UnityEngine;
using System.Collections.Generic;


// TODO:
// SOME SORT OF COLOR CODING MECHANIC FOR LOCAL CO-OP WITH THE UNIT SELECTION 

public class TroopManagment : MonoBehaviour
{
    [SerializeField] private GameObject selectorCircle;
    public List<GameObject> selectedTroops = new List<GameObject>();
    private bool isSelecting;
    public float selectionRadius = 8f;
    private bool placingWaypoint = false;
    [SerializeField] private GameObject waypoint;
    [SerializeField] private GameObject outlineWaypoint;
    public bool managingTroops = false;
    // private float menuTime; // for timing how long menu is up for before it closes
    


    
    void Start()
    {
        isSelecting = false;
        HideWaypointOutline();
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameObject.GetComponent<BuildMode>().isBuilding){
            
        
            if(Input.GetKey(KeyCode.Space)){
                isSelecting = true;
                managingTroops = true;
                ShowSelectingCircle();
            }
            else{
                isSelecting=false;
                HideSelectingCircle();
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
                        isSelecting = true;
                        break;
                    case bool _ when Input.GetKeyDown(KeyCode.Escape):
                        ValidInput(); // do nothing just close menu
                        break;
                }
                
                
            }
            else{
                HideWaypointOutline();
            }

            if(isSelecting){
                SelectTroops();
            }
        }

    }



    void ShowWaypointOutline(){
        if(selectedTroops.Count>0){
            outlineWaypoint.transform.localScale= new Vector3(1f,1f,1f);
            placingWaypoint = true;
        }
    }

    void HideWaypointOutline(){
        outlineWaypoint.transform.localScale= new Vector3(0f,0f,0f);

    }

    void StopAndClearSelecting(){
        isSelecting = false;
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

    
    void ShowSelectingCircle(){
        selectorCircle.transform.localScale = new Vector3(15f,0.0002f,15f);
    }

    
    void HideSelectingCircle(){
        selectorCircle.transform.localScale = new Vector3(0,0,0);
    }


    void ValidInput(){
        placingWaypoint = false;
        StopAndClearSelecting();
        
    }


    void SetWaypoint(){
        GameObject deployedPoint = Instantiate(waypoint, transform.position + transform.rotation * new Vector3(0f,0f,-3f), transform.rotation);
        foreach (GameObject troop in selectedTroops)
        {
            TroopAI troopScript = troop.GetComponent<TroopAI>();
            troopScript.commandingPlayer = null;
            troopScript.waypoint = deployedPoint;
            deployedPoint.GetComponent<Waypoint>().troopsBound.Add(troop);
        }
    }
}
