using UnityEngine;
using System.Collections.Generic;


// TODO:
// SOME SORT OF COLOR CODING MECHANIC FOR LOCAL CO-OP WITH THE UNIT SELECTION 

public class TroopManagment : MonoBehaviour
{
    [SerializeField] private GameObject selectorCircle;
    public List<GameObject> selected = new List<GameObject>();
    private bool selecting;
    public float selectionRadius = 8f;
    [SerializeField] private bool menuOpen = false;
    [SerializeField] private GameObject waypoint;
    [SerializeField] private GameObject outlineWaypoint;
    public bool inProgress = false;
    // private float menuTime; // for timing how long menu is up for before it closes
    


    
    void Start()
    {
        selecting = false;
        HideWaypointOutline();
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameObject.GetComponent<BuildMode>().isBuilding){
            
        
            if(Input.GetKey(KeyCode.Space)){
                selecting = true;
                inProgress = true;
                ShowSelectingCircle();
            }
            else{
                selecting=false;
                HideSelectingCircle();
                ShowWaypointOutline();
            }
            if(menuOpen){
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
                        Debug.Log("test");
                        SetWaypoint();
                        ValidMenuSelection();
                        break;
                    case bool _ when Input.GetKeyDown(KeyCode.Space):
                        menuOpen = false; // close menu but keep selecting
                        selecting = true;
                        break;
                    case bool _ when Input.GetKeyDown(KeyCode.Escape):
                        menuOpen = false; // cancel selecting and menu if escape
                        StopAndClearSelecting();
                        break;
                }
                
                
            }
            else{
                HideWaypointOutline();
            }

            if(selecting){
                SelectTroops();
            }
        }

    }



    void ShowWaypointOutline(){
        if(selected.Count>0){
            outlineWaypoint.transform.localScale= new Vector3(1f,1f,1f);
            menuOpen = true;
        }
    }

    void HideWaypointOutline(){
        outlineWaypoint.transform.localScale= new Vector3(0f,0f,0f);
    }

    void StopAndClearSelecting(){
        selecting = false;
        foreach(GameObject troop in selected){
            TroopMovement troopScript = troop.GetComponent<TroopMovement>();
            troopScript.underSelection=false;
            troopScript.commandingPlayer=null;

        }
        selected.Clear();
        inProgress = false;
    }

    void SelectTroops(){
                
        foreach (GameObject troop in TroopMovement.troops)
        {
            Vector3 playerPosition = transform.position;
            Vector3 troopPosition = troop.transform.position;
            playerPosition.y = 0f;
            troopPosition.y = 0f;

            TroopMovement troopScript = troop.GetComponent<TroopMovement>();
            if (Vector3.Distance(playerPosition, troopPosition) <= selectionRadius && 
                    !selected.Contains(troop) && 
                   troopScript.underSelection==false)
            {
                
                troopScript.underSelection=true;
                troopScript.commandingPlayer = gameObject;
                troopScript.DeleteFromWaypoint();
                selected.Add(troop);
            }
        }

        Debug.Log("Selected Troops: " + selected.Count);
    }

    void ShowSelectingCircle(){
        selectorCircle.transform.localScale = new Vector3(15f,0.0002f,15f);
    }

    
    void HideSelectingCircle(){
        selectorCircle.transform.localScale = new Vector3(0,0,0);
    }

    void ValidMenuSelection(){
        menuOpen = false;
        StopAndClearSelecting();
        
    }


    void SetWaypoint(){
        GameObject deployedPoint = Instantiate(waypoint, transform.position + transform.rotation * new Vector3(0f,0f,-3f), transform.rotation);
        foreach (GameObject troop in selected)
        {
            TroopMovement troopScript = troop.GetComponent<TroopMovement>();
            troopScript.commandingPlayer = null;
            troopScript.waypoint = deployedPoint;
            deployedPoint.GetComponent<Waypoint>().troopsBound.Add(troop);
        }
    }
}
