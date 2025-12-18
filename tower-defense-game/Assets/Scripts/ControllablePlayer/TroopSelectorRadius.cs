using UnityEngine;
using System.Collections.Generic;


public class TroopSelectorRadius : MonoBehaviour
{
    private List<GameObject> troopsInRadius = new List<GameObject>(); // List for all troops in radius
    public List<GameObject> TroopsInRadius { get {return troopsInRadius; }} // List for all troops in radius

    // Adds all collisions to the collision list if they are a troop
    private void OnTriggerEnter(Collider collision)
    {
        GameObject obj = collision.gameObject;
        if(obj.CompareTag("Troop")) {
            troopsInRadius.Add(obj);
        }
    }

    // Removes all collisions from the collision list if they are a troop
    private void OnTriggerExit(Collider collision)
    {
        GameObject obj = collision.gameObject;
        if(obj.CompareTag("Troop")) {
            troopsInRadius.Remove(obj);
        }
    }

    // Clears all troops in collision list
    public void ClearTroopsInRadius() {
        troopsInRadius.Clear();
    }
}
