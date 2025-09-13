using UnityEngine;

public class Troops : MonoBehaviour
{
    public static Troops Instance { get; private set; }
    public static Transform Transform { get; private set;}

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Transform = transform;
        DontDestroyOnLoad(gameObject);
    }
}
