using UnityEngine;
using Unity.Mathematics;

public class Obstacle : MonoBehaviour
{
    [Header("Obstacle Size")]
    [SerializeField] private int2 size = new int2(1, 1);
    
    [Header("Auto-Calculate Size")]
    [Tooltip("Automatically calculate size from collider or renderer bounds")]
    [SerializeField] private bool autoCalculateSize = true;
    
    private bool isOccupying = false;
    private Vector3 occupyOrigin;

    void Start()
    {
        if (autoCalculateSize)
        {
            CalculateSize();
        }
        OccupyGridArea();
    }

    private void CalculateSize()
    {
        Bounds bounds = GetObjectBounds();
        
        if (bounds.size == Vector3.zero)
        {
            Debug.LogWarning($"Could not calculate bounds for {gameObject.name}. Using manual size.");
            return;
        }

        int tilesX = Mathf.CeilToInt(bounds.size.x / GridManager.TILE_SIZE);
        int tilesZ = Mathf.CeilToInt(bounds.size.z / GridManager.TILE_SIZE);
        
        size = new int2(Mathf.Max(1, tilesX), Mathf.Max(1, tilesZ));
    }

    private Bounds GetObjectBounds()
    {
        // try collider then renderers
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            return col.bounds;
        }

        // fall back to Renderer
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            return rend.bounds;
        }

        return new Bounds(transform.position, Vector3.zero);
    }

    void OnDestroy()
    {
        FreeGridArea();
    }

    private void OccupyGridArea()
    {
        // Use bounds min corner so the grid occupation aligns with the actual visual obstacle,
        // not shifted from the pivot/center
        Bounds bounds = GetObjectBounds();
        occupyOrigin = new Vector3(bounds.min.x, transform.position.y, bounds.min.z);

        Vector3Int gridPos = GridManager.WorldPosFromCoordinates(occupyOrigin);
        
        // Check if the area is valid
        if (gridPos.x < 0 || gridPos.x + size.x > GridManager.GRID_WIDTH ||
            gridPos.z < 0 || gridPos.z + size.y > GridManager.GRID_HEIGHT)
        {
            Debug.LogWarning($"Obstacle at {transform.position} is outside grid bounds.");
            return;
        }
        Debug.Log($"Obstacle '{gameObject.name}' occupying grid from ({gridPos.x}, {gridPos.z}) size {size} (boundsMin={bounds.min}, worldPos={transform.position})");
        GridManager.Instance.OccupyArea(occupyOrigin, size, 0);
        
        isOccupying = true;
    }

    private void FreeGridArea()
    {
        if (!isOccupying || GridManager.Instance == null)
        {
            return;
        }

        GridManager.Instance.StopOccupying(occupyOrigin, size);
        isOccupying = false;
    }

}