using UnityEngine;
using System.Collections.Generic;

public class Building : MonoBehaviour
{
    public readonly int range = 10; //for territory purposes, can also be used as actual range
    private int tempResource1;
    private int tempResource2; // for refund purposes, the resources spent to build building
    // need to set values for tempResources upon instantiation

    public void InstantiateBuilding(int tempResource1, int tempResource2){
        this.tempResource1 = tempResource1;
        this.tempResource2 = tempResource2;
    }
}
