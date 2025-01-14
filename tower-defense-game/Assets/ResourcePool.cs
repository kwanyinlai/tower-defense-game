using UnityEngine;

public class ResourcePool : MonoBehaviour
{
    private static int tempResource1; // we can rename to whatever more appropriate when we 
    private static int tempResource2; // get to that stage e.g. wood
    
    void Update()
    {
        
    }


    public static void AddResource1(int resourceAmount) // again rename as appropriate
    {
        tempResource1 += resourceAmount;

    }

    public static bool DepleteResource1(int resourceAmount)
    {   
        if(tempResource1 - resourceAmount >= 0){
            tempResource1 -= resourceAmount;
            return true;
        }
        return false;
       
    }

    
    
}
