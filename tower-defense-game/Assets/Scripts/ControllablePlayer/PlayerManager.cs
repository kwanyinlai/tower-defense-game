using UnityEngine;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public static List<GameObject> players = new List<GameObject>();
    public enum PlayerStates {
        ControllingCharacter,
        SelectingTroops,
        MenuOpen,
        ObserverMode,
        DisabledControls,
        PlacingBuilding
    }
    private PlayerStates currentState;
    public PlayerStates CurrentState {
        get
        {
            return currentState;
        } 
    }

    void Start()
    {
        players.Add(gameObject);
    }
    public void ToggleMenu()
    {
        if (currentState != PlayerStates.MenuOpen)
        {
            currentState = PlayerStates.MenuOpen;
        }
        else
        {
            currentState = PlayerStates.ControllingCharacter;
        }
    }

    void Update()
    {
        // NOTE: It might feel redundant to check inputs in two scripts and only allow
        // states to change in this script, but we really want state changing to be centralised
        Debug.Log(currentState);
        if (CurrentState == PlayerStates.ControllingCharacter)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                currentState = GetComponent<CharacterCameraController>().ChangeCameraState();
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                currentState = PlayerStates.MenuOpen;
                
            }
        }
        else if (CurrentState == PlayerStates.MenuOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                currentState = PlayerStates.ControllingCharacter;
            }
        }
        else if (currentState == PlayerStates.PlacingBuilding)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                currentState = PlayerStates.ControllingCharacter;
            }
            
        }
        
    }


    // for convenience of local coop, just a script to attach which has all controllable player objects

    
}
