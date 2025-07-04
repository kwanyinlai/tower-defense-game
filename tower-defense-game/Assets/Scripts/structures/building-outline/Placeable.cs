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
        meshRenderer = GetComponent<MeshRenderer>();
        size = CalculateOccupyingSize();
        offset = CalculateOffset();
        Debug.Log("offset = " + offset);

        //TODO: Remove and replace with code to actually add the correct resources based on building
        requiredResources.Add("TestResource1", 150);
        requiredResources.Add("TestResource2", 150);
    }


    Vector2Int CalculateOccupyingSize(){
        return new Vector2Int(Mathf.CeilToInt( meshRenderer.bounds.size.x / GridSystem.tileSize), 
                Mathf.CeilToInt(meshRenderer.bounds.size.z / GridSystem.tileSize) );
    }

    public bool IsBuildable(Vector3 placementPosition){
        return GridSystem.IsTileAreaBuildable(placementPosition, size);
    }

    int CalculateOffset(){
        return (System.Math.Max(size.x, size.y)/2) + 2;
    }
}
