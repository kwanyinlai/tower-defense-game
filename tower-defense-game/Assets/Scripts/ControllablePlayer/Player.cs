using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public static List<GameObject> players = new List<GameObject>(); 

    void Start()
    {
        players.Add(gameObject);
    }

    // for convenience of local coop, just a script to attach which has all controllable player objects

    
}
