using UnityEngine;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public static List<GameObject> players = new List<GameObject>();
    public enum PlayerStates {
        ControllingCharacter,
        SelectingTroops,
        BuildMenuOpen,
        ObserverMode,
        DisabledControls,
        PlacingBuilding
    }
    private PlayerStates currentState;
    public PlayerStates CurrentState { get; set; }

    void Start()
    {
        players.Add(gameObject);
    }

    void Update()
    {
        
    }

    // for convenience of local coop, just a script to attach which has all controllable player objects

    
}
