using UnityEngine;

public class GridSystem : MonoBehaviour
{

    // TODO: build menu for selecting buildings
    public static int gridWidth = 200;
    public static int gridHeight = 200;
    public static float tileSize = 4f;

    private static bool[,] grid;

    void Start()
    {
        //make all squares buidable
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

    public static bool IsTileBuildable(Vector3 coordinates)
    {
        Vector3Int gridCoords = CoordinatesToGrid(coordinates);

        if (gridCoords.x >= 0 && gridCoords.x < gridWidth && 
            gridCoords.y >= 0 && gridCoords.y < gridHeight)
        {
            return grid[gridCoords.x, gridCoords.y];
        }
        return false;
    }

    public static bool IsTileAreaBuildable(Vector3 coordinates, Vector2Int size)
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


    public static void OccupyArea(Vector3 coordinates, Vector2Int size)
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

    public static Vector3Int CoordinatesToGrid(Vector3 coordinates)
    {
        int x = Mathf.FloorToInt(coordinates.x / tileSize) + gridWidth/2; // this could be broken i think
        int z = Mathf.FloorToInt(coordinates.z / tileSize) + gridHeight/2;
        return new Vector3Int(x,0,z);
    }

    public static Vector3 GridToCoordinates(Vector3 gridCoords)
    {
        return new Vector3((gridCoords.x - gridWidth/2) * tileSize, 0f, (gridCoords.z - gridHeight/2) * tileSize);
    }

    public static Quaternion SnapRotation(Quaternion currentRotation)
    {
        float yRotation = currentRotation.eulerAngles.y;
        yRotation = Mathf.Round(yRotation / 90f) * 90f;

        return Quaternion.Euler(0f, yRotation, 0f);
    }
}
