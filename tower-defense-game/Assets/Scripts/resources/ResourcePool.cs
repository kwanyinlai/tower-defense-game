using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;

public class ResourcePool : MonoBehaviour
{
    
    private static int numResources = 3;
    private static Dictionary<string, int> resources = new Dictionary<string, int>();

    // make sure to initialize all types of resources to a default value in start
    private void Start()
    {
        resources["TestResource1"] = 10;
        resources["TestResource2"] = 10;
        resources["TestResource3"] = 10;
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
        string result = "";
        foreach (KeyValuePair<string, int> pair in resources)
        {
            result += pair.Key + ": " + pair.Value + "\n";
        }
        return result;
    }

}
