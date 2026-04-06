using UnityEngine;
using Unity.Mathematics;

/// <summary>
/// Cost field visualiser
/// </summary>
public class PathfindingDebugOverlay : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool showOnStart = false;
    [SerializeField] private float highCostThreshold = 5f;
    [SerializeField] private int visualRadius = 30;
    
    private bool showOverlay = false;
    
    void Start()
    {
        showOverlay = showOnStart;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            showOverlay = !showOverlay;
            Debug.Log($"Grid debug overlay: {(showOverlay ? "ON" : "OFF")}");
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showOverlay || !Application.isPlaying) return;
        if (GridManager.Instance == null) return;
        
        var grid = GridManager.Instance.GetGrid();
        if (grid == null) return;
        
        Camera cam = Camera.main;
        if (cam == null) return;
        
        Vector3 camPos = cam.transform.position;
        Vector3Int camGrid = GridManager.WorldPosFromCoordinates(camPos);
        
        int minX = Mathf.Max(0, camGrid.x - visualRadius);
        int maxX = Mathf.Min(GridManager.GRID_WIDTH - 1, camGrid.x + visualRadius);
        int minZ = Mathf.Max(0, camGrid.z - visualRadius);
        int maxZ = Mathf.Min(GridManager.GRID_HEIGHT - 1, camGrid.z + visualRadius);
        
        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                float cost = grid[x, z].walkCost;
                
                if (cost >= GridNode.UNWALKABLE)
                {
                    Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // Red = unwalkable
                    Vector3 worldPos = GridManager.CoordinatesToWorldPos(new Vector3(x, 0, z));
                    Gizmos.DrawCube(worldPos + Vector3.up * 0.2f, 
                        new Vector3(GridManager.TILE_SIZE * 0.9f, 0.1f, GridManager.TILE_SIZE * 0.9f));
                }
                else if (cost > highCostThreshold)
                {
                    Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Yellow = high cost
                    Vector3 worldPos = GridManager.CoordinatesToWorldPos(new Vector3(x, 0, z));
                    Gizmos.DrawCube(worldPos + Vector3.up * 0.2f, 
                        new Vector3(GridManager.TILE_SIZE * 0.9f, 0.1f, GridManager.TILE_SIZE * 0.9f));
                }
            }
        }
    }
}
