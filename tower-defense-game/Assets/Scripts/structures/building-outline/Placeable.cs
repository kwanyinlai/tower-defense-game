using System.Collections.Generic;
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

    private Dictionary<string, int> requiredResources = new Dictionary<string, int>();
    
    public Dictionary<string, int> RequiredResources
    {
        get { return requiredResources; }
    }

    private GridManager gridManager;

    void Update(){
        if(ResourcePool.EnoughResources(RequiredResources))
        {
            meshRenderer.material = normalMat;
        
        }
        else{
            meshRenderer.material = bankruptMat;
        }
    }

    void Start(){
        InitPlaceable();
        gridManager = GridManager.Instance;
    }

    // Created an Init Placeable Function So That Creating 
    public void InitPlaceable() {
        meshRenderer = GetComponent<MeshRenderer>();
        size = CalculateOccupyingSize();
        offset = CalculateOffset();

        //TODO: Remove and replace with code to actually add the correct resources based on building
        requiredResources.TryAdd("Wood", 150);
        requiredResources.TryAdd("TestResource2", 150);
    }

    Vector2Int CalculateOccupyingSize(){
        return new Vector2Int(Mathf.CeilToInt( meshRenderer.bounds.size.x / GridManager.tileSize), 
                Mathf.CeilToInt(meshRenderer.bounds.size.z / GridManager.tileSize) );
    }

    public bool IsBuildable(Vector3 placementPosition){
        return gridManager.IsTileAreaBuildable(placementPosition, size);
    }

    int CalculateOffset(){
        return (System.Math.Max(size.x, size.y)/2) + 2;
    }

    public void DebugStatement() {
        Debug.Log("name of prefab selected is " + prefab.name);
        Debug.Log("size is " + size.x + ", " + size.y);
        Debug.Log("grid is " + gridCoords.x + ", " + gridCoords.y);
    }
}
