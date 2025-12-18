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
    [SerializeField] private PlayerManager playerData;
    
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
                if(controlState == ControlState.Controlling) {
                    StopControlling();
                }
                SetupSelecting();
            }

            // Stops selecting or controlling
            if(Input.GetKeyUp(KeyCode.Escape) && controlState != ControlState.Nothing) {
                StopControlling();
            }
        }
        
        // Checks wheter inputs are meant for selecting or controlling
        if(playerData.CurrentState == PlayerManager.PlayerStates.SelectingTroops) {
            // Proccess whether selection input is for wide spread (radius) or single
            if(Input.GetKeyUp(KeyCode.O)) {
                if(isGrowing) {
                    AddTroopsInRadius();
                } else {
                    SelectClosestTroop();
                }
                timeElapsed = -1;
            }

            // Makes all troops selected to following flag 
            if(Input.GetKeyUp(KeyCode.Return) && controlState == ControlState.Selecting) {
                SetupControl();
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
                troopAI.IsUnderSelection = true;
            }
        }
    }

    // Selects the closest troops that's not already in the selected list
    void SelectClosestTroop() {
        GameObject closestTroop = null;
        double closestDistance = 0.0;
        Vector3 playerPos = gameObject.transform.position;
        foreach(GameObject troop in PlayerTroopAI.AllPlayerTroops) {
            double currDistance = Vector3.Distance(troop.transform.position, playerPos);
            if(closestTroop == null || (currDistance < closestDistance && !selectedTroops.Contains(troop))) {
                closestTroop = troop;
                closestDistance = currDistance;
            }
        }

        if(closestTroop != null && closestDistance <= MAX_RADIUS) {
            selectedTroops.Add(closestTroop);
        }
    }

    // Stops controlling all troops and returns them to default state
    void StopControlling() {
        controlState = ControlState.Nothing;
        // Returns all controlled troops to default state
        foreach(GameObject troop in selectedTroops) {
            PlayerTroopAI troopAI = troop.GetComponent<PlayerTroopAI>();
            troopAI.IsUnderSelection = false;
        }
        // Hides bubble indicator
        if(bubbleIndicator.activeSelf == true){
            troopSelectorRadius.ClearTroopsInRadius();
            bubbleIndicatorTransform.localScale = new Vector3(BUBBLE_INIT_SIZE, 0.1f, BUBBLE_INIT_SIZE);
            bubbleIndicator.SetActive(false);
        }
        selectedTroops.Clear();
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
}
