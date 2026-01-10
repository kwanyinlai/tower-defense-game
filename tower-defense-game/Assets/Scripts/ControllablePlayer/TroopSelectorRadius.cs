using UnityEngine;
using System.Collections.Generic;


public class TroopSelectorRadius : MonoBehaviour
{
    private List<GameObject> troopsInRadius = new List<GameObject>(); // List for all troops in radius
    public List<GameObject> TroopsInRadius { get {return troopsInRadius; }} // List for all troops in radius

    private List<PlayerTroopAI> troopAIList = new List<PlayerTroopAI>();
    public List<GameObject> TroopAIList { get {return troopsInRadius; }}
    
    // Adds all collisions to the collision list if they are a troop
    private void OnTriggerEnter(Collider collision)
    {
        GameObject obj = collision.gameObject;
        if(obj.CompareTag("Troop")) {
            PlayerTroopAI troopAI = obj.GetComponent<PlayerTroopAI>();
            if(!troopAI.IsUnderSelection) {
                troopAI.ShowCircle();
                troopAIList.Add(troopAI);
                troopsInRadius.Add(obj);
            }
        }
    }

    // Removes all collisions from the collision list if they are a troop
    private void OnTriggerExit(Collider collision)
    {
        GameObject obj = collision.gameObject;
        if(obj.CompareTag("Troop")) {
            int index = troopsInRadius.IndexOf(obj);
            if(index != -1) {
                troopAIList[index].HideCircle();
                troopAIList.RemoveAt(index);
                troopsInRadius.RemoveAt(index);
            }
        }
    }

    // Clears all troops in collision list
    public void ClearTroopsInRadius() {
        troopsInRadius.Clear();
    }
}
