using UnityEngine;
using System.Collections.Generic;

public class Building : MonoBehaviour
{
    public readonly int range = 10; //for territory purposes, can also be used as actual range
    private Dictionary<string, int> requiredResources;

    public void InstantiateBuilding(Dictionary<string, int> required){
        this.requiredResources = required;
    }
}
