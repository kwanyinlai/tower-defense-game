using UnityEngine;
using System.Collections.Generic;

public class Waypoint : MonoBehaviour
{

    // TODO: merge waypoints that are close together
    // TODO: arrow from troops or some indicator to waypoint

    // TODO: timer for troop command menu
    // TODO: bug with troops stuck after killing enemy but also selected

    public List<GameObject> troopsBound = new List<GameObject>();

    void Update(){
        if (troopsBound.Count==0){
            Destroy(gameObject);
        }
    }

}
