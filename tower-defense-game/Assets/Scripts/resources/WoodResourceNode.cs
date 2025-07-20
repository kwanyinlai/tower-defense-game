using UnityEngine;
using System.Collections.Generic;

public class ResourceNode : MonoBehaviour
{
    private int maxTimesCollected;
    private int timesCollected;
    private Dictionary<string, int> resourcesPerCollect;
    private GameObject waveSystem;
    private int currWave = 0;


    void Start()
    {
        waveSystem = GameObject.Find("Wave System");
        timesCollected = 0;
        maxTimesCollected = 10;
        setResourcesPerCollect();
    }

    private void setResourcesPerCollect()
    {
        resourcesPerCollect = new Dictionary<string, int>();
        resourcesPerCollect.Add("TestResource1", 10);
    }

    void Update()
    {
        if (currWave != waveSystem.GetComponent<WaveScript>().waveNum)
        {
            currWave = waveSystem.GetComponent<WaveScript>().waveNum;
            timesCollected = 0;
        }
    }

    public void collectResources()
    {
        if(timesCollected < maxTimesCollected)
        {
            ResourcePool.AddResource(resourcesPerCollect);
            timesCollected++;
        }
    }

}
