using UnityEngine;
using System.Collections.Generic;

public abstract class Building : MonoBehaviour
{
    public readonly int range = 10; //for territory purposes, can also be used as actual range
    public string building_name;
    public GameObject building_model; // used in order to highlighting the building structure

    public Dictionary<string, int> sellResources = new Dictionary<string, int>();

    protected abstract void IntializeSellResources();

    protected void Start()
    {
        IntializeSellResources();
    }

    public Dictionary<string, int> getSellResources()
    {
        return sellResources;
    }
    
}
