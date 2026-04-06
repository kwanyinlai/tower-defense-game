using Unity.Mathematics;
using UnityEngine;

public class FlowField
{
    public int2 goalPosition;
    public float[,] costField;
    public int2[,] flowField;
    public float lastAccessed;
    
    // Add region bounds
    public int2 regionMin;
    public int2 regionMax;

    public Vector3 GetDirectionAt(Vector3 worldPosition)
    {
        Vector3Int vectoredGrid = GridManager.WorldPosFromCoordinates(worldPosition);
        int2 gridPos = new int2(vectoredGrid.x, vectoredGrid.z);

        // Check if position is within flow field region
        if (gridPos.x < regionMin.x || gridPos.x >= regionMax.x ||
            gridPos.y < regionMin.y || gridPos.y >= regionMax.y)
        {
            // fallback
            Vector3 goalWorld = GridManager.CoordinatesToWorldPos(goalPosition);
            return (goalWorld - worldPosition).normalized;
        }

        // Convert to local coordinates
        int2 localPos = new int2(gridPos.x - regionMin.x, gridPos.y - regionMin.y);
        int2 flowDir = flowField[localPos.x, localPos.y];
        
        return new Vector3(flowDir.x, 0, flowDir.y).normalized;
    }
}