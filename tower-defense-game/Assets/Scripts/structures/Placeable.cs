using UnityEngine;

public class Placeable : MonoBehaviour
{
    // attach to the outline

    private MeshRenderer meshRenderer;
    public Vector2Int size;
    protected Vector2Int gridCoords;
    public GameObject prefab;

    void Start(){
        meshRenderer = GetComponent<MeshRenderer>();
        size = CalculateOccupyingSize();
    }

    Vector2Int CalculateOccupyingSize(){
        return new Vector2Int(Mathf.CeilToInt( meshRenderer.bounds.size.x / GridSystem.tileSize), 
                Mathf.CeilToInt(meshRenderer.bounds.size.z / GridSystem.tileSize) );
    }

    public bool IsBuildable(Vector3 placementPosition){
        return GridSystem.IsTileAreaBuildable(placementPosition, size);
    }
}
