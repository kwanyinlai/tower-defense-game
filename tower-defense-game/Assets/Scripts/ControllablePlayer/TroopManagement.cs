using UnityEngine;
using System.Collections.Generic;

// TODO:
// SOME SORT OF COLOR CODING MECHANIC FOR LOCAL CO-OP WITH THE UNIT SELECTION 

public class TroopManagement : MonoBehaviour
{
    [SerializeField] private GameObject selectorCircle;
    private List<GameObject> selectedTroops = new List<GameObject>();
    public List<GameObject> SelectedTroops { get { return selectedTroops; } }
    private float selectionRadius = 100f;
    private bool isPlacingWaypoint = false; // change flags to replace with something else
    [SerializeField] private GameObject waypoint;
    [SerializeField] private GameObject waypointOutline;
    private bool isManagingTroops = false; // TODO: delete this

    // private float menuTime; // for timing how long menu is up for before it closes
    [SerializeField] private LayerMask selectableLayer;
    private PlayerManager playerData;
    
    public bool IsManagingTroops { get; private set; }

    
    void Start()
    {
        isManagingTroops = false;
        playerData = GetComponent<PlayerManager>();
        HideWaypointOutline();
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && selectedTroops.Count > 1)
        {
            StopAndClearSelecting();
        }
        
        if (playerData.CurrentState == PlayerManager.PlayerStates.ControllingCharacter)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!(Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) && selectedTroops.Count > 1)
                {
                    StopAndClearSelecting();
                }

                isManagingTroops = true;
            }

            if (isPlacingWaypoint)
            {
                switch(true)
                {
                    case bool _ when Input.GetKeyDown(KeyCode.Return):
                        SetWaypoint();
                        ValidInput();
                        isPlacingWaypoint = false;
                        break;
                    case bool _ when Input.GetKeyDown(KeyCode.Escape):
                        ValidInput();
                        isPlacingWaypoint = false;
                        break;
                }
            }
            else
            {
                HideWaypointOutline();
            }

            if (isManagingTroops)
            {
                RaySelectAlliedTroops();
                isManagingTroops = false;
            }
        }
    }

    void ShowWaypointOutline()
    {
        waypointOutline.transform.localScale = new Vector3(1f, 1f, 1f);
        isPlacingWaypoint = true;
    }
    // TODO: make default reference in troopmmanagment directly call prefab, why is flag outline disappearing?


    void HideWaypointOutline()
    {
        waypointOutline.transform.localScale = new Vector3(0f, 0f, 0f);
    }

    void StopAndClearSelecting()
    {
        isManagingTroops = false;
        selectedTroops.Clear();
    }

    void SelectTroops()
    {
        // use new FactionManager
        // List<TroopAI> allPlayerTroops = FactionManager.Instance.GetAlliesOf(TroopFaction.Player);
        
        // foreach (TroopAI troopController in allPlayerTroops)
        // {
        //     if (troopController == null) continue;
            
        //     GameObject troop = troopController.gameObject;
            
        //     if (!selectedTroops.Contains(troop))
        //     {
        //         selectedTroops.Add(troop);
        //     }
        // }

        // Debug.Log("Selected Troops: " + selectedTroops.Count);
    }

    void ValidInput()
    {
        isPlacingWaypoint = false;
        StopAndClearSelecting();
    }

    void SetWaypoint()
    {
        Vector3 waypointPosition = transform.position + transform.rotation * new Vector3(0f, 0f, 3f);
        GameObject deployedPoint = null;
        
        if (Waypoint.FindNearestWaypoint(waypointPosition) == null)
        {
            deployedPoint = Instantiate(waypoint, waypointPosition, transform.rotation);
        }
        else
        {
            deployedPoint = Waypoint.FindNearestWaypoint(waypointPosition);
        }
        
        Vector3 destination = deployedPoint.transform.position;

        // use shared flow field if commanding multiple troops to same location
        bool useFlowField = selectedTroops.Count >= 3;
        if (useFlowField && PathfindingManager.Instance != null)
        {
            PathfindingManager.Instance.RequestSharedFlowField(destination);
        }

        // Compute spread positions so troops don't all converge on the exact same point
        Vector3[] spreadPositions = DestinationSpreader.ComputeSpreadPositions(
            destination, selectedTroops.Count);
        int troopIndex = 0;

        foreach (GameObject troop in selectedTroops)
        {
            if (troop == null) { troopIndex++; continue; }
            
            TroopAI controller = troop.GetComponent<TroopAI>();
            if (controller != null)
            {
                Vector3 personalTarget = spreadPositions[troopIndex];
                if (useFlowField)
                    controller.CommandMoveToWithFlowField(personalTarget);
                else
                    controller.CommandMoveTo(personalTarget);
            }
            
            // TODO: remove this maybe? new controller means this might be redundant but we will see
            Waypoint waypointComponent = deployedPoint.GetComponent<Waypoint>();
            if (waypointComponent != null)
            {
                waypointComponent.troopsBound.Add(troop);
            }

            troopIndex++;
        }

        StopAndClearSelecting();
    }

    void RaySelectAlliedTroops()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + new Vector3(0f, 2f, 0f);
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        
        if (Physics.Raycast(rayOrigin, forward, out hit, 10000f, selectableLayer))
        {
            TroopAI troopController = hit.collider.GetComponent<TroopAI>();
            
            if (troopController != null && 
                troopController.GetFaction() == TroopFaction.Player && 
                !selectedTroops.Contains(troopController.gameObject))
            {
                selectedTroops.Add(troopController.gameObject);
                ShowWaypointOutline();
                return;
            }
        }
    }

    public void DeselectTroop(GameObject troop)
    {
        if (selectedTroops.Contains(troop))
        {
            selectedTroops.Remove(troop);
        }
    }
}