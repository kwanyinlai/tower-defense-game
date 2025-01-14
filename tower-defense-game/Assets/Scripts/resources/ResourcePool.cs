using UnityEngine;

public class ResourcePool : MonoBehaviour
{
    [SerializeField] private static int tempResource1 = 10; // we can rename to whatever more appropriate when we 
    [SerializeField] private static int tempResource2 = 10; // get to that stage e.g. wood


    
    void Update()
    {
        
    }

    public static bool EnoughResources(int resource1, int resource2)
    {
        return resource1 <= tempResource1 && resource2 <= tempResource2;
    }

    public static void AddResource1(int resourceAmount) // rename as appropriate
    {
        tempResource1 += resourceAmount;

    }

    public static bool DepleteResource1(int resourceAmount)
    {   
        if(tempResource1 - resourceAmount >= 0){
            tempResource1 -= resourceAmount;
            Debug.Log(tempResource1);
            Debug.Log("decrease by " + resourceAmount);
            return true;
        }
        return false;
       
    }

    public static void AddResource2(int resourceAmount) // again rename as appropriate
    {
        tempResource2 += resourceAmount;

    }

    public static bool DepleteResource2(int resourceAmount)
    {   
        if(tempResource2 - resourceAmount >= 0){
            tempResource2 -= resourceAmount;
            return true;
        }
        return false;
       
    }

    
    
}
