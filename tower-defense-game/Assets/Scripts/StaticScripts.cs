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
    } // Put this into a relevant class rather than in a dedicated "StaticScripts" class

    public static float horizontalDistance(float x1, float y1, float x2, float y2)
    {
        return Mathf.Sqrt(Mathf.Pow(x2 - x1, 2) + Mathf.Pow(y2 - y1, 2));
    } // TODO: There is an in-built function for this already
}
