using UnityEngine;
using Unity.Mathematics;

public class FlowField
{
    public int2 goalPosition;
    public float[,] costField;
    public int2[,] flowField;
    public float lastAccessed;


    public Vector3 GetDirectionAt(Vector3 worldPosition)
    {
        Vector3Int vectoredGrid = GridManager.WorldPosFromCoordinates(worldPosition);
        int2 gridPos = new int2(vectoredGrid.x, vectoredGrid.z);

        if (!GridManager.Instance.NodeIsInBounds(gridPos))
        {
            return Vector3.zero;
        }

        int2 flowDir = flowField[gridPos.x, gridPos.y];
        return new Vector3(flowDir.x, 0, flowDir.y).normalized;
    }
}