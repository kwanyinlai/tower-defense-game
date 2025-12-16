using UnityEngine;
using System.Collections.Generic;

public class TroopControllerManager : MonoBehaviour
{
    enum ControlState {
        Selecting,
        Controling,
        Nothing
    }

    private float timeElapsed;
    private float bubbleSize;
    private bool isGrowing;
    private const float BUBBLE_GROWTH_RATE = 15.0f;
    private const float BUBBLE_MAX_SCALE = 45.0f;
    private const float MAX_RADIUS = 45.0f;
    private const float TAP_THRESHOLD = 0.35f;
    [SerializeField] private GameObject bubbleIndicator;
    [SerializeField] private TroopSelectorRadius troopSelectorRadius;
    private List<GameObject> selectedTroops = new List<GameObject>();
    private Transform bubbleIndicatorTransform;
    private ControlState controlState = ControlState.Nothing;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bubbleIndicator.SetActive(false);
        bubbleIndicatorTransform = bubbleIndicator.transform; // bubbleIndicator.GetComponent<Transform>();
        timeElapsed = -1;
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
        if(Input.GetKeyDown(KeyCode.O)) {
            if(controlState == ControlState.Controling) {
                StopControling();
            }
            SetupSelecting();
        }

        if(Input.GetKeyUp(KeyCode.O)) {
            if(isGrowing) {
                AddTroopsInRadius();
            } else {
                SelectClosestTroop();
            }
            timeElapsed = -1;
        }

        if(Input.GetKeyUp(KeyCode.Return) && controlState == ControlState.Selecting) {
            SetupControl();
        }

        if(Input.GetKeyUp(KeyCode.Escape) && controlState != ControlState.Nothing) {
            StopControling();
        }
    }

    void SetupSelecting() {
        timeElapsed = 0;
        bubbleSize = 1.0f;
        controlState = ControlState.Selecting;
        isGrowing = false;
    }

    void SetupControl() {
        controlState = ControlState.Controling;
        Debug.Log("TESTING " + selectedTroops.Count + " under command");
        troopSelectorRadius.SetIsAcceptingCollisions(false);
        troopSelectorRadius.ClearTroopsInRadius();
        bubbleIndicator.SetActive(false);
        foreach(GameObject troop in selectedTroops) {
            PlayerTroopAI troopAI = troop.GetComponent<PlayerTroopAI>();
            troopAI.IsUnderSelection = true;
        }
    }

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

    void StopControling() {
        controlState = ControlState.Nothing;
        foreach(GameObject troop in selectedTroops) {
            PlayerTroopAI troopAI = troop.GetComponent<PlayerTroopAI>();
            troopAI.IsUnderSelection = false;
        }
        selectedTroops.Clear();
    }

    void AddTroopsInRadius() {
        foreach(GameObject troop in troopSelectorRadius.GetTroopsInRadius()) {
            selectedTroops.Add(troop);
        }
    }

    // Decides when to increase the radius selector and if it is visible
    void IncreaseSelectionRadius() {
        timeElapsed += Time.deltaTime;
        // Either increase bubble size or init bubble size
        if(isGrowing) {
            if(bubbleSize < BUBBLE_MAX_SCALE) {
                bubbleSize = (timeElapsed - TAP_THRESHOLD) * BUBBLE_GROWTH_RATE;
                // 44.0f/9.0f * (timeElapsed - 3.0f - TAP_THRESHOLD) * (timeElapsed - 3.0f - TAP_THRESHOLD) + 45.0f;
                // 21.5f/3.375f * (timeElapsed -1.5f) * (timeElapsed -1.5f) * (timeElapsed -1.5f) + 22.5f;
                if(bubbleSize > BUBBLE_MAX_SCALE) {
                    bubbleSize = BUBBLE_MAX_SCALE;
                }
                bubbleIndicatorTransform.localScale = new Vector3(bubbleSize, 0.1f, bubbleSize);
            }
        } else if(timeElapsed > TAP_THRESHOLD) {
            // When to know the difference between a taping and holding a button
            bubbleIndicator.SetActive(true);
            // bubbleIndicator is visible w/ dimensions of 1
            bubbleIndicatorTransform.localScale = new Vector3(bubbleSize, 0.1f, bubbleSize);
            troopSelectorRadius.SetIsAcceptingCollisions(true);
            troopSelectorRadius.ClearTroopsInRadius();
            isGrowing = true;
        }
    }
}
