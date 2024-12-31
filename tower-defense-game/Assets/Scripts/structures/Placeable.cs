using UnityEngine;

public class Placeable : MonoBehaviour
{
    public Vector2Int CalculateGridSizeFromBounds(Bounds bounds)
    {
        float tileSize = GridSystem.tileSize;
        float width = Mathf.Ceil(bounds.size.x / tileSize);
        float depth = Mathf.Ceil(bounds.size.z / tileSize);

        return new Vector2Int((int)Mathf.Ceil(width), (int)Mathf.Ceil(depth));
    }

}
