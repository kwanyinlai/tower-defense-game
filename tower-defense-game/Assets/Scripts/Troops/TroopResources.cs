using UnityEngine;
using System.Collections.Generic;

public class TroopResources : MonoBehaviour
{
    // Resource Attributes
    protected Dictionary<string, int> sellResources = new Dictionary<string, int>();
    public string TroopName { get; set; } // TODO: will probably delete later

    void Start()
    {
        IntializeSellResources();
    }



    protected virtual void IntializeSellResources() // TODO: move this to sep script
    {
        //TODO: Remove and replace with code to actually add the correct resources based on building
        sellResources.Add("Wood", 100);
        sellResources.Add("TestResource2", 100);
    }
    
    public void SetSellResources(Dictionary<string, int> resources)
    {
        sellResources = resources;
    }

    public Dictionary<string, int> GetSellResources()
    {
        return sellResources;
    }
}
