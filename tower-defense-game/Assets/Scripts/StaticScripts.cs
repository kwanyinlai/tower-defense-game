using System.Collections.Generic;
using UnityEngine;

public class StaticScripts : MonoBehaviour
{
    //Class storing static scripts such as constant operations, conversions, etc

    public static string resourcesToText(Dictionary<string, int> resources)
    {
        string result = "";
        foreach (KeyValuePair<string, int> pair in resources)
        {
            result += pair.Key + ": " + pair.Value + "\n";
        }
        return result;
    }
}
