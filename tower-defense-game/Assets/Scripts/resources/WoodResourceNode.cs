using UnityEngine;
using System.Collections.Generic;

public class ResourceNode : MonoBehaviour
{
    private int maxTimesCollected;
    private int timesCollected;
    private Dictionary<string, int> resourcesPerCollect;
    private WaveManager waveManager;
    private int currWave = 0;


    void Start()
    {
        waveManager = WaveManager.Instance;
        timesCollected = 0;
        maxTimesCollected = 10;
        SetResourcesPerCollect();
    }

    private void SetResourcesPerCollect()
    {
        resourcesPerCollect = new Dictionary<string, int>();
        resourcesPerCollect.Add("Wood", 10);
    }

    void Update()
    {
        if (currWave != waveManager.waveNum)
        {
            currWave = waveManager.waveNum;
            timesCollected = 0;
        }
    }

    public void CollectResources()
    {
        if(timesCollected < maxTimesCollected)
        {
            ResourcePool.AddResource(resourcesPerCollect);
            timesCollected++;
        }
    }

}
