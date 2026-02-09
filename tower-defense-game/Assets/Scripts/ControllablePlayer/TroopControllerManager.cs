using UnityEngine;
using System.Collections.Generic;

public class TroopControllerManager : MonoBehaviour
{
    public enum ControlState {
        Selecting, // Selecting troops
        Controlling, // Controlling troops
        Nothing // Not managing any troops
    }

    private float timeElapsed;
    private float bubbleSize;
    private bool isGrowing;

    private const float BUBBLE_GROWTH_RATE = 15.0f;
    private const float BUBBLE_MAX_SCALE = 45.0f;
    private const float MAX_RADIUS = 45.0f;
    private const float TAP_THRESHOLD = 0.35f;
    private const float BUBBLE_INIT_SIZE = 1.0f;

    private const float MAX_PICKUP_DISTANCE = 5.0f;
    
    [SerializeField] private GameObject bubbleIndicator;
    [SerializeField] private TroopSelectorRadius troopSelectorRadius;

    [SerializeField] private GameObject waypointOutline;
    [SerializeField] private GameObject waypointPrefab;
    
    private PlayerManager playerData;
    private GameObject closestWaypoint;
    
    private List<GameObject> selectedTroops = new List<GameObject>();
    private List<GameObject> controlledTroops = new List<GameObject>();
    private Transform bubbleIndicatorTransform;
    private ControlState controlState = ControlState.Nothing;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bubbleIndicator.SetActive(false);
        waypointOutline.SetActive(false);
        bubbleIndicatorTransform = bubbleIndicator.transform; // bubbleIndicator.GetComponent<Transform>();
        timeElapsed = -1;
        playerData = GetComponent<PlayerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        WaypointInRadius();
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
                if(controlState == ControlState.Selecting) {
                    StopSetup();
                } else {
                    StopControlling();
                }
            }

            // Makes all troops selected to following flag 
            if(Input.GetKeyUp(KeyCode.Return)) {
                if(controlState == ControlState.Selecting) {
                    SetupControl(); // Confirms selection of troops 
                } else if(controlState == ControlState.Controlling && closestWaypoint != null) {
                    MergePrompt(); // Gives control of all selecting troops to closest flag
                } else if(closestWaypoint != null) {
                    PickupWaypoint(closestWaypoint); // Picksup flag since not controlling any troops @ moment
                } else if(controlState == ControlState.Controlling) {
                    PlaceWaypoint(); // Creates new flag and gives control to that flag
                }
            }
        }
        
        // Checks wheter inputs are meant for selecting or controlling
        if(playerData.CurrentState == PlayerManager.PlayerStates.SelectingTroops) {
            // Proccess whether selection input is for wide spread (radius) or single
            if(Input.GetKeyUp(KeyCode.O)) {
                // Checks through boolean if the O was a tap or hold (aka adding one troop vs. a list of them)
                if(!isGrowing){
                    AddClosestTroop();
                }
                timeElapsed = -1;
            }
        }
    }

    // Sets up selecting input state
    void SetupSelecting() {
        controlState = ControlState.Selecting;
        timeElapsed = 0;
        bubbleSize = BUBBLE_INIT_SIZE;
        isGrowing = false;
    }

    // Sets up control by making all selected troops to follow flag
    void SetupControl() {
        // Add all troops in circle
        controlState = ControlState.Controlling;
        if(bubbleIndicator.activeSelf) {
            AddTroopsInRadius();
        }
        if(controlledTroops.Count == 0 && selectedTroops.Count == 0) {
            // Cancels control mode if no troops selected (allows players to redo selecting without having to cancel)
            StopControlling();
        } else {
            // Resets bubble indicator 
            troopSelectorRadius.ClearTroopsInRadius();
            bubbleIndicatorTransform.localScale = new Vector3(BUBBLE_INIT_SIZE, 0.1f, BUBBLE_INIT_SIZE);
            bubbleIndicator.SetActive(false);
            // Gives control of troops to player
            foreach(GameObject troop in selectedTroops) {
                if(troop != null) {
                    PlayerTroopAI troopAI = troop.GetComponent<PlayerTroopAI>();
                    troopAI.SetupControl(gameObject, false);
                }
            }
            controlledTroops.AddRange(selectedTroops);
            selectedTroops.Clear();
            // Turn on flag
            waypointOutline.SetActive(true);
        }
    }

    // Selects the closest troops that's not already in the selected list
    void AddClosestTroop() {
        GameObject closestTroop = null;
        double closestDistance = 0.0;
        Vector3 playerPos = gameObject.transform.position;
        // Finds closest troop
        foreach(GameObject troop in PlayerTroopAI.AllPlayerTroops) {
            double currDistance = Vector3.Distance(troop.transform.position, playerPos);
            if((closestTroop == null || currDistance < closestDistance) && !controlledTroops.Contains(troop) && !selectedTroops.Contains(troop) && !troopSelectorRadius.TroopsInRadius.Contains(troop) && !troop.GetComponent<PlayerTroopAI>().IsUnderSelection) {
                closestTroop = troop;
                closestDistance = currDistance;
            }
        }

        // Checks if closest troop is eligable
        if(closestTroop != null && closestDistance <= MAX_RADIUS) {
            PlayerTroopAI troopAI = closestTroop.GetComponent<PlayerTroopAI>();
            selectedTroops.Add(closestTroop);
            troopAI.ShowCircle();
        }

        // Edge-case in order for the player not to be stuck in selecting mode
        if(selectedTroops.Count == 0) {
            StopSetup();
        }
    }

    void StopSetup() {
        if(controlledTroops.Count > 0) {
            controlState = ControlState.Controlling;
        } else {
            controlState = ControlState.Nothing;
        }

        // Hides bubble indicator
        if(bubbleIndicator.activeSelf == true){
            troopSelectorRadius.ClearTroopsInRadius();
            bubbleIndicatorTransform.localScale = new Vector3(BUBBLE_INIT_SIZE, 0.1f, BUBBLE_INIT_SIZE);
            bubbleIndicator.SetActive(false);
        }
        // Returns all controlled troops to default state
        foreach(GameObject troop in selectedTroops) {
            if(troop != null) {
                PlayerTroopAI troopAI = troop.GetComponent<PlayerTroopAI>();
                troopAI.HideCircle();
            }
        }
        selectedTroops.Clear();
    }

    // Stops controlling all troops and returns them to default state
    void StopControlling() {
        controlState = ControlState.Nothing;
        // Returns all controlled troops to default state
        foreach(GameObject troop in controlledTroops) {
            if(troop != null) {
                PlayerTroopAI troopAI = troop.GetComponent<PlayerTroopAI>();
                troopAI.DisableControl();
            }
        }

        // Hides flag
        waypointOutline.SetActive(false);

        controlledTroops.Clear();
    }

    // Adds all troops in the bubble indicator
    void AddTroopsInRadius() {
        foreach(GameObject troop in troopSelectorRadius.TroopsInRadius) {
            if(troop != null) {
                selectedTroops.Add(troop);
            }
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
            // For when the selection input is meant as a hold and should reveal bubble indicator
            bubbleIndicator.SetActive(true);
            bubbleIndicatorTransform.localScale = new Vector3(bubbleSize, 0.1f, bubbleSize);
            troopSelectorRadius.ClearTroopsInRadius();
            isGrowing = true;
        }
    }

    void MergePrompt() {
        PlaceWaypoint(true);
    }

    // Creates and places down a waypoint object and gives command of troops to waypoint
    void PlaceWaypoint(bool wantsToMerge = false) {
        // Gives control of troops to waypoint
        GameObject waypoint;
        if(wantsToMerge && closestWaypoint != null) {
            waypoint = closestWaypoint; 
        } else {
            waypoint = Instantiate(waypointPrefab, waypointOutline.transform.position, Quaternion.identity) as GameObject;
        }
        waypoint.GetComponent<Waypoint>().PlaceWaypoint(controlledTroops);
        controlledTroops.Clear();
        StopControlling();
    }

    // Pickup Waypoint and gives control of troops under its command to the player
    void PickupWaypoint(GameObject waypoint) {
        waypointOutline.SetActive(true);
        Waypoint waypointScript = waypoint.GetComponent<Waypoint>();
        controlledTroops.AddRange(waypointScript.PickupWaypoint());
        SetupControl();
    }

    // Detects and highlights when a waypoint is in range (will not highlight anything if the player is currently selecting troops)
    void WaypointInRadius() {
        if(controlState == ControlState.Selecting) {
            // If the player is selecting troops, then should not be considering flags 
            if(closestWaypoint != null) {
                closestWaypoint.GetComponent<Waypoint>().UnhighlightFlag();
                closestWaypoint = null;
            }
        } else {
            // Highlights closest waypoints in range
            GameObject waypoint = Waypoint.FindNearestWaypoint(gameObject.transform.position);
            if(waypoint == null) {
                // Deselcts old waypoint if it gets too far
                if(closestWaypoint != null) {
                    closestWaypoint.GetComponent<Waypoint>().UnhighlightFlag();
                }
                closestWaypoint = null;
            } else if(Vector3.Distance(waypoint.transform.position, gameObject.transform.position) <= MAX_PICKUP_DISTANCE) {
                // Unhighlight old waypoint and highlight new closest one
                if(waypoint != closestWaypoint) {
                    if(closestWaypoint != null) {
                        closestWaypoint.GetComponent<Waypoint>().UnhighlightFlag();
                    }
                    closestWaypoint = waypoint;
                    closestWaypoint.GetComponent<Waypoint>().HighlightFlag();
                }
            }
        }
    }
}
