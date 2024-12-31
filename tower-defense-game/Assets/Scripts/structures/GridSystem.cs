using UnityEngine;

public class GridSystem : MonoBehaviour
{

    //TODO: don't show outline if cant be placed
    // TODO: indicator for moving which isn't too intrusive
    // TODO: build menu for selecitng buildings
    public int gridWidth = 100;
    public int gridHeight = 100;
    public static float tileSize = 4f;

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

    void GetFloorSize(){

    }

    public bool IsTileBuildable(Vector3 coordinates)
    {
        Vector3Int gridCoords = CoordinatesToGrid(coordinates);

        if (gridCoords.x >= 0 && gridCoords.x < gridWidth && 
            gridCoords.y >= 0 && gridCoords.y < gridHeight)
        {
            return grid[gridCoords.x, gridCoords.y];
        }
        return false;
    }

    public bool IsTileAreaBuildable(Vector3 coordinates, Vector2Int size)
    {
        Vector3 gridCoords = CoordinatesToGrid(coordinates);
        for (int x = (int) gridCoords.x; x < (int) gridCoords.x + size.x; x++)
        {
            for (int z = (int) gridCoords.z; z < (int) gridCoords.z + size.y; z++)
            {

                if (x < 0 || x >= gridWidth || z < 0 || z >= gridHeight || !grid[x, z])
                {

                    return false; 
                }
            }
        }
        return true;
    }


    public void OccupyArea(Vector3 coordinates, Vector2Int size)
    {
        Vector3Int gridCoords = CoordinatesToGrid(coordinates);
        for (int x = gridCoords.x; x < gridCoords.x + size.x; x++)
        {
            for (int z = gridCoords.z; z < gridCoords.z + size.y; z++)
            {


                if (x >= 0 && x < gridWidth && z >= 0 && z < gridHeight)
                {
                    grid[x, z] = false;

                }
            }
        }
    }
    public void StopOccupying(Vector2Int gridCoords, Vector2Int size)
    {
        for (int x = gridCoords.x; x < gridCoords.x + size.x; x++)
        {
            for (int z = gridCoords.y; z < gridCoords.y + size.y; z++)
            {

                if (x >= 0 && x < gridWidth && z >= 0 && z < gridHeight)
                {
 
                    grid[x, z] = true;
                }
            }
        }
    }

    public Vector3Int CoordinatesToGrid(Vector3 coordinates)
    {
        int x = Mathf.FloorToInt(coordinates.x / tileSize);
        int z = Mathf.FloorToInt(coordinates.z / tileSize);
        return new Vector3Int(x,0,z);
    }

    public Vector3 GridToCoordinates(Vector3 gridCoords)
    {
        return new Vector3(gridCoords.x * tileSize, 0f, gridCoords.z * tileSize);
    }

    public Quaternion SnapRotation(Quaternion currentRotation)
    {
        float yRotation = currentRotation.eulerAngles.y;
        yRotation = Mathf.Round(yRotation / 90f) * 90f;

        return Quaternion.Euler(0f, yRotation, 0f);
    }
}
