using UnityEngine;

public class Placeable : MonoBehaviour
{
    // attach to the outline

    private MeshRenderer meshRenderer;
    public Vector2Int size;
    protected Vector2Int gridCoords;
    public GameObject prefab;
    public int offset;
    
    [SerializeField] private Material bankruptMat;
    [SerializeField] private Material normalMat;


    [SerializeField] private int tempResource1 = 5; 

    public int TempResource1{
        get{ return tempResource1;}
    }

    [SerializeField] private int tempResource2 = 5;

    public int TempResource2{
        get{ return tempResource2;}
    }

    
    void Update(){
        if(ResourcePool.EnoughResources(tempResource1, tempResource2))
        {
            meshRenderer.material = normalMat;
        
        }
        else{
            meshRenderer.material = bankruptMat;
        }
    }

    void Start(){
        meshRenderer = GetComponent<MeshRenderer>();
        size = CalculateOccupyingSize();
        offset = CalculateOffset();
        Debug.Log("offset = " + offset);
    }


    Vector2Int CalculateOccupyingSize(){
        return new Vector2Int(Mathf.CeilToInt( meshRenderer.bounds.size.x / GridSystem.tileSize), 
                Mathf.CeilToInt(meshRenderer.bounds.size.z / GridSystem.tileSize) );
    }

    public bool IsBuildable(Vector3 placementPosition){
        return GridSystem.IsTileAreaBuildable(placementPosition, size);
    }

    int CalculateOffset(){
        return -(System.Math.Max(size.x, size.y)/2) -6 ;
    }
}
