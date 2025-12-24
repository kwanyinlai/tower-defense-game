using UnityEngine;
using System.Collections.Generic;

public class TroopControllerManager : MonoBehaviour
{
    public enum ControlState {
        Selecting,
        Controlling,
        Nothing
    }

    private float timeElapsed;
    private float bubbleSize;
    private bool isGrowing;

    private const float BUBBLE_GROWTH_RATE = 15.0f;
    private const float BUBBLE_MAX_SCALE = 45.0f;
    private const float MAX_RADIUS = 45.0f;
    private const float TAP_THRESHOLD = 0.35f;
    private const float BUBBLE_INIT_SIZE = 1.0f;
    
    [SerializeField] private GameObject bubbleIndicator;
    [SerializeField] private TroopSelectorRadius troopSelectorRadius;
    
    private PlayerManager playerData;
    
    private List<GameObject> selectedTroops = new List<GameObject>();
    private Transform bubbleIndicatorTransform;
    private ControlState controlState = ControlState.Nothing;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bubbleIndicator.SetActive(false);
        bubbleIndicatorTransform = bubbleIndicator.transform; // bubbleIndicator.GetComponent<Transform>();
        timeElapsed = -1;
        playerData = GetComponent<PlayerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleInputs();

        if(timeElapsed != -1) {
            IncreaseSelectionRadius();
        }


    }

    // Handles any inputs
    void HandleInputs() {
        // Checks whether inputs are meant for starting/stopping selecting or stopping controlling (or statement is because PlayerManager's input checker could run before or after this)
        if(playerData.CurrentState == PlayerManager.PlayerStates.SelectingTroops || playerData.CurrentState == PlayerManager.PlayerStates.ControllingCharacter) {
            // Starts selecting
            if(Input.GetKeyDown(KeyCode.O)) {
                SetupSelecting();
            }

            // Stops selecting or controlling
            if(Input.GetKeyUp(KeyCode.Escape) && controlState != ControlState.Nothing) {
                StopControlling();
            }

            // Makes all troops selected to following flag 
            if(Input.GetKeyUp(KeyCode.Return)) {
                if(controlState == ControlState.Selecting) {
                    SetupControl();
                } else if(controlState == ControLState.Controlling) {
                    PlaceWaypoint();
                }
            }
        }
        
        // Checks wheter inputs are meant for selecting or controlling
        if(playerData.CurrentState == PlayerManager.PlayerStates.SelectingTroops) {
            // Proccess whether selection input is for wide spread (radius) or single
            if(Input.GetKeyUp(KeyCode.O)) {
                if(isGrowing) {
                    AddTroopsInRadius();
                } else {
                    AddClosestTroop();
                }
                timeElapsed = -1;
            }
        }
    }

    // Sets up selecting input
    void SetupSelecting() {
        controlState = ControlState.Selecting;
        timeElapsed = 0;
        bubbleSize = BUBBLE_INIT_SIZE;
        isGrowing = false;
    }

    // Sets up control by making all selected troops to follow flag
    void SetupControl() {
        controlState = ControlState.Controlling;
        if(selectedTroops.Count == 0) {
            // Cancels control mode if no troops selected (allows players to redo selecting without having to cancel)
            StopControlling();
        } else {
            // Sets all selected troops to follow and hides bubble indicator
            troopSelectorRadius.ClearTroopsInRadius();
            bubbleIndicatorTransform.localScale = new Vector3(BUBBLE_INIT_SIZE, 0.1f, BUBBLE_INIT_SIZE);
            bubbleIndicator.SetActive(false);
            foreach(GameObject troop in selectedTroops) {
                PlayerTroopAI troopAI = troop.GetComponent<PlayerTroopAI>();
                troopAI.SetupControl(gameObject, false);
            }
        }
    }

    // Selects the closest troops that's not already in the selected list
    void AddClosestTroop() {
        GameObject closestTroop = null;
        double closestDistance = 0.0;
        Vector3 playerPos = gameObject.transform.position;
        foreach(GameObject troop in PlayerTroopAI.AllPlayerTroops) {
            double currDistance = Vector3.Distance(troop.transform.position, playerPos);
            if((closestTroop == null || currDistance < closestDistance) && !selectedTroops.Contains(troop)) {
                closestTroop = troop;
                closestDistance = currDistance;
            }
        }

        if(closestTroop != null && closestDistance <= MAX_RADIUS) {
            PlayerTroopAI troopAI = closestTroop.GetComponent<PlayerTroopAI>();
            selectedTroops.Add(closestTroop);
            troopAI.ShowCircle();
        }

        // Edge-case in order for the player not to be stuck in selecting mode
        if(selectedTroops.Count == 0) {
            StopControlling();
        }
    }

    // Stops controlling all troops and returns them to default state
    void StopControlling() {
        controlState = ControlState.Nothing;
        // Returns all controlled troops to default state
        foreach(GameObject troop in selectedTroops) {
            PlayerTroopAI troopAI = troop.GetComponent<PlayerTroopAI>();
            troopAI.DisableControl();
        }
        // Hides bubble indicator
        if(bubbleIndicator.activeSelf == true){
            troopSelectorRadius.ClearTroopsInRadius();
            bubbleIndicatorTransform.localScale = new Vector3(BUBBLE_INIT_SIZE, 0.1f, BUBBLE_INIT_SIZE);
            bubbleIndicator.SetActive(false);
        }

        // selectedTroops.Clear();
        while(selectedTroops.Count > 0) {
            // Checks for if troop has been already killed
            if(selectedTroops[0] != null) {
                selectedTroops[0].GetComponent<PlayerTroopAI>().HideCircle();
            }
            selectedTroops.RemoveAt(0);
        }
    }

    // Adds all troops in the bubble indicator
    void AddTroopsInRadius() {
        foreach(GameObject troop in troopSelectorRadius.TroopsInRadius) {
            selectedTroops.Add(troop);
        }
    }

    // Decides when to increase the radius selector and if it is visible
    void IncreaseSelectionRadius() {
        timeElapsed += Time.deltaTime;
        if(isGrowing) {
            if(bubbleSize < BUBBLE_MAX_SCALE) {
                // Formula for bubble size growth
                bubbleSize = (timeElapsed - TAP_THRESHOLD) * BUBBLE_GROWTH_RATE;
                if(bubbleSize > BUBBLE_MAX_SCALE) {
                    bubbleSize = BUBBLE_MAX_SCALE;
                }
                bubbleIndicatorTransform.localScale = new Vector3(bubbleSize, 0.1f, bubbleSize);
            }
        } else if(timeElapsed > TAP_THRESHOLD) {
            // Signals code that the selection input is meant for wide area and reveals bubble indicator
            bubbleIndicator.SetActive(true);
            bubbleIndicatorTransform.localScale = new Vector3(bubbleSize, 0.1f, bubbleSize);
            troopSelectorRadius.ClearTroopsInRadius();
            isGrowing = true;
        }
    }

    void PlaceWaypoint() {
        // Call FindNearestWaypoint and if there's a waypoint that's close
            // Create popup that asks if want to merge, if yes, then just call AddWaypoint to closest waypoint
            // If no do the following
                // Create new waypoint object
                // Call PlaceWaypoint on object 
                // Clear selectedTroop list from this script
    }

    void PickupWaypoint(GameObject waypoint) {
        // Calls PickupWaypoint on object
        // Adds troop list returned from PickupWaypoint to this script
        // Call SetupControl(gameObject, true) on all troops
    }

    void WaypointInRadius() {
        // Calls FindNearestWaypoint on Waypoint and sees if there's a waypoint that's close
        // Check if waypoint is within radius
        // Make global variable that stores closest waypoint
            // If global variable is null, set new waypoint as value
            // If global variable is not equal to new waypoint, set new waypoint as new value and then revert material of old waypoint
        // Set closest waypoint to a new material to indicator highlighting
        // Have this function be called in radius
        // Have HandleInputs see check if closest waypoint is not null and if return key is pressed
            // If so, then call pickup waypoint  
    }
}
