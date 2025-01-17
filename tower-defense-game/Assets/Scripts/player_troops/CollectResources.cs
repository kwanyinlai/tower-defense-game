using UnityEngine;

public class CollectResources : MonoBehaviour
{
    
    void Update()
    {
        // check if in range
        // check if hotkey pressed?
        // do we want this to be on controllable character or specialised troop
        
        // do we use distance-based or ray-casting
        // e.g. hashmap of resource nodes + distances and then search for min distance?
        
        // if(Vector3.Distance())
    }

    private GameObject FindReosurceNode()
    {
        int minDist = 0;
        for(int i = 1 ; i < ResourceNode.resourceNodes.Count ; i++)
        {
            break;
            
        }
        return null;
    }
}
