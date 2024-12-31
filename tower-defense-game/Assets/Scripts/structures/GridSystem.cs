using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public int gridWidth = 100;
    public int gridHeight = 100;
    public float tileSize = 1f;

    private bool[,] grid;

    void Start()
    {
        grid = new bool[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                grid[x, z] = true;
            }
        }

    }

    public bool IsBuildable(Vector3 worldPosition)
    {
        Vector3Int gridCoords = WorldToGridPosition(worldPosition);

        if (gridCoords.x >= 0 && gridCoords.x < gridWidth && 
            gridCoords.y >= 0 && gridCoords.y < gridHeight)
        {
            return grid[gridCoords.x, gridCoords.y];
        }
        return false;
    }

    public Vector3Int WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / tileSize);
        int z = Mathf.FloorToInt(worldPosition.z / tileSize);
        return new Vector3Int(x,0,z);
    }

    public Vector3 GridToWorldPosition(Vector3 gridCoords)
    {
        return new Vector3(gridCoords.x * tileSize, 0f, gridCoords.y * tileSize);
    }

    public Quaternion SnapRotation(Quaternion currentRotation)
    {
        float yRotation = currentRotation.eulerAngles.y;
        yRotation = Mathf.Round(yRotation / 90f) * 90f;

        return Quaternion.Euler(0f, yRotation, 0f);
    }
}
