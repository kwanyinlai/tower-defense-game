using UnityEngine;

public class WoodResourceBuilding : ResourceBuilding
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    new void Start()
    {
        range = 5;
        base.Start();
        collectionInterval = 5f;
    }

    protected override void IntializeSellResources()
    {
        sellResources.Add("TestResource1", 100);
        sellResources.Add("TestResource2", 100);
    }

}
