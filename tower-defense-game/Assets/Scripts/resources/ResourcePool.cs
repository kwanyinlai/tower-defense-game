using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;

public class ResourcePool : MonoBehaviour
{
    
    // private static int numResources = 3;  // commenting this out for now, uncomment if you need it
    private static Dictionary<string, int> resources = new Dictionary<string, int>();

    // make sure to initialize all types of resources to a default value in start
    private void Start()
    {
        resources["TestResource1"] = 510;
        resources["TestResource2"] = 510;
        resources["TestResource3"] = 510;
    }

    void Update()
    {
        
    }

    public static void AddResource(string resource, int amount)
    {
        resources[resource] += amount;
    }

    public static void AddResource(Dictionary<string, int> required)
    {
        foreach (var elem in required)
        {
            resources[elem.Key] += elem.Value;
        }
    }

    public static bool DepleteResource(Dictionary<string, int> required)
    {
        if(EnoughResources(required) == false)
        {
            return false;
        }
        foreach (var elem in required)
        {
            resources[elem.Key] -= elem.Value;
        }
        return true;
    }

    public static bool DepleteResource(string resource, int amount)
    {
        if(EnoughResources(resource, amount))
        {
            resources[resource] -= amount;
            return true;
        }
        return false;
    }


    public static bool EnoughResources(Dictionary<string, int> required)
    {
        foreach(var elem in required)
        {
            if (resources[elem.Key] < elem.Value)
            {
                return false;
            }
        }
        return true;
    }

    public static bool EnoughResources(string resource, int amount)
    {
        return resources[resource] >= amount;
    }

    public static string getResourceText()
    {
        return StaticScripts.resourcesToText(resources);
    }

}
