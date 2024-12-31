using UnityEngine;

public class Placeable : MonoBehaviour
{
    // attach to the outline

    private MeshRenderer meshRenderer;
    public Vector2Int size;
    protected Vector2Int gridCoords;
    protected GridSystem grid;
    public GameObject prefab;

    void Start(){
        meshRenderer = GetComponent<MeshRenderer>();
        grid = GameObject.Find("grid-manager").GetComponent<GridSystem>(); // issue is that this is called befor eobject is even instantiated so it doesnt exist
        size = CalculateOccupyingSize();
    }

    Vector2Int CalculateOccupyingSize(){
        return new Vector2Int(Mathf.CeilToInt( meshRenderer.bounds.size.x / GridSystem.tileSize), 
                Mathf.CeilToInt(meshRenderer.bounds.size.z / GridSystem.tileSize) );
    }

    public bool IsBuildable(Vector3 placementPosition){
        return grid.IsTileAreaBuildable(placementPosition, size);
    }
}
