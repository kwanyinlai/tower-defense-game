using UnityEngine;

public class BaseManager : MonoBehaviour
{
    [SerializeField] private GameObject basePrefab;
    [SerializeField] private Vector3 instantiatePos;
    private GameObject currBase;

    void Start()
    {
        currBase=Instantiate(basePrefab, instantiatePos, Quaternion.identity);
    }

    public GameObject GetBase(){
        return currBase;
    }


}
