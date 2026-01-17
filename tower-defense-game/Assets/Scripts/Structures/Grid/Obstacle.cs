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

        // Fall back to Renderer
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

        Vector3Int gridPos = GridManager.WorldPosFromCoordinates(transform.position);
        
        // Check if the area is valid
        if (gridPos.x < 0 || gridPos.x + size.x > GridManager.GRID_WIDTH ||
            gridPos.z < 0 || gridPos.z + size.y > GridManager.GRID_HEIGHT)
        {
            Debug.LogWarning($"Obstacle at {transform.position} is outside grid bounds.");
            return;
        }
        Debug.Log($"Obstacle occupying grid from ({gridPos.x}, {gridPos.z}) size {size}");
        GridManager.Instance.OccupyArea(transform.position, size, 0);
        
        isOccupying = true;
    }

    private void FreeGridArea()
    {
        if (!isOccupying || GridManager.Instance == null)
        {
            return;
        }

        GridManager.Instance.StopOccupying(transform.position, size);
        isOccupying = false;
    }

}