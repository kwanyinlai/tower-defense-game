using UnityEngine;
using System.Collections.Generic;

public class Building : MonoBehaviour
{
    public readonly int range = 10; //for territory purposes, can also be used as actual range
    public string building_name;
    public GameObject building_model; // used in order to highlighting the building structure

    private Dictionary<string, int> requiredResources;

    public void InstantiateBuilding(Dictionary<string, int> required)
    {
        this.requiredResources = required;
    }

    public Dictionary<string, int> getRequiredResources()
    {
        return requiredResources;
    }
    
}
