using UnityEngine;
using System;

public class TroopControllerManager : MonoBehaviour
{
    private float timeElapsed;
    private float bubbleSize;
    private bool isGrowing;
    private const float BUBBLE_GROWTH_RATE = 15.0f;
    private const float BUBBLE_MAX_SCALE = 45.0f;
    private const float TAP_THRESHOLD = 0.35f;
    [SerializeField] private GameObject bubbleIndicator;
    private Transform bubbleIndicatorTransform;
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
        if(Input.GetKeyDown(KeyCode.O)) {
            timeElapsed = 0;
            bubbleSize = 1.0f;
            isGrowing = false;
        }

        if(Input.GetKeyUp(KeyCode.O)) {
            if(isGrowing) {
                Debug.Log("Hold " + bubbleSize);
            } else {
                Debug.Log("Tap");
            }
            timeElapsed = -1;
        }

        if(timeElapsed != -1) {
            timeElapsed += Time.deltaTime;
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
                Debug.Log("Holding Threshold Reached");
                bubbleIndicator.SetActive(true);
                isGrowing = true;
                // bubbleIndicator is visible w/ dimensions of 1
            }
        }
    }
}
