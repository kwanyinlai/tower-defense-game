using UnityEngine;
using System.Collections.Generic;

public class Waypoint : MonoBehaviour
{

    // TODO: merge waypoints that are close together
    // TODO: arrow from troops or some indicator to waypoint

    // TODO: timer for troop command menu
    // TODO: not deleting waypoint correctly

    

    public List<GameObject> troopsBound = new List<GameObject>();

    void Update(){
        if (troopsBound.Count==0){
            Destroy(gameObject);
        }
    }

}
