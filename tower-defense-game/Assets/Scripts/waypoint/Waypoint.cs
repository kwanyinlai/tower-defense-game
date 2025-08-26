using UnityEngine;
using System.Collections.Generic;

public class Waypoint : MonoBehaviour
{

    // TODO: merge waypoints that are close together
    // TODO: arrow from troops or some indicator to waypoint

    // TODO: timer for troop command menu
    // TODO: not deleting waypoint correctly

    
    private static List<GameObject> allWaypoints = new List<GameObject>();
    public List<GameObject> troopsBound = new List<GameObject>();


    void Start(){
        allWaypoints.Add(gameObject);
    }

    void Update(){
        if (troopsBound.Count==0){
            Destroy(gameObject);
            allWaypoints.Remove(gameObject);
        }
    }

    public static GameObject FindNearestWaypoint(Vector3 coordinate){ 
        // find nearest waypoint within radius of 5
        if (allWaypoints.Count == 0){
            return null;
        }
        GameObject minWaypoint = allWaypoints[0];
        foreach(GameObject waypointPos in allWaypoints){
            if (Vector3.Distance(waypointPos.transform.position, coordinate) <= Vector3.Distance(minWaypoint.transform.position, coordinate)){
                minWaypoint = waypointPos;
            }
        }
        if (Vector3.Distance(coordinate, minWaypoint.transform.position) <= 5){
            return minWaypoint;
        }
        return null;
    }

}
