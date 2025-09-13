using UnityEngine;

public class BaseManager : MonoBehaviour
{
    [SerializeField] private GameObject basePrefab;
    [SerializeField] private Vector3 instantiatePos;
    private GameObject currBase;

    public static BaseManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        currBase=Instantiate(basePrefab, instantiatePos, Quaternion.identity);
    }

    public GameObject GetBase(){
        return currBase;
    }

}
