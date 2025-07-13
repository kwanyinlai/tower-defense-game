using UnityEngine;

public class BasicBerserkerBarrack : Barracks
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    new void Start()
    {
        base.Start();
        spawnInterval = 3f;
        maxTroops = 10f;
        timer = 0f;
        currentTroops = 0;
    }

    protected override void InitializeTroopResources()
    {
        //TODO: Remove and replace with code to actually add the correct resources based on building
        troopResources.Add("TestResource3", 10);
    }

    protected override void IntializeSellResources()
    {
        //TODO: Remove and replace with code to actually add the correct resources based on building
        sellResources.Add("TestResource1", 100);
        sellResources.Add("TestResource2", 100);
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
    }
}
