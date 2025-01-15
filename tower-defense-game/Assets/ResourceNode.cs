using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    [SerializeField] private int resource1Output = 1; // output per sec
    [SerializeField] private int resource2Output = 0; // output per sec

    public int TempResource1
    {
        get{ return resource1Output; }
    }
    public int TempResource2
    {
        get{ return resource2Output; }
    }


}
