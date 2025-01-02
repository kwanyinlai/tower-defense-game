using UnityEngine;

public class GridSystem : MonoBehaviour
{

    // TODO: build menu for selecting buildings
    public int gridWidth = 200;
    public int gridHeight = 200;
    public static float tileSize = 4f;

    private bool[,] grid;
    private int[,] territoryGrid; //0 for not territory, 1 not -> territory, 2 for territory, 3 for territory -> not
    public GameObject target;


    void Start()
    {
        //make all squares buidable
        grid = new bool[gridWidth, gridHeight];
        territoryGrid = new int[gridWidth, gridHeight];
        territoryGrid = new int[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                grid[x, z] = true;
                territoryGrid[x, z] = 0;
            }
        }
        setStarterTerritory();
        
        
    }

    void setStarterTerritory()
    {
        Vector3 pos = target.transform.position;
        Vector3Int gridPos = CoordinatesToGrid(pos);
        for(int i = gridPos.x - 5; i <= gridPos.x + 5; i++)
        {
            for(int j =  gridPos.z - 5; j <= gridPos.z + 5; j++)
            {
                float distSquared = Mathf.Pow(i - gridPos.x, 2) + Mathf.Pow(j - gridPos.z, 2);
                if(distSquared < 25) //Territory radius of 3 around the target
                {
                    territoryGrid[i, j] = 2;
                }
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

                if (x < 0 || x >= gridWidth || z < 0 || z >= gridHeight || !grid[x, z] || territoryGrid[x, z] != 2)
                {

                    return false; 
                }
            }
        }
        return true;
    }


    public void OccupyArea(Vector3 coordinates, Vector2Int size, int range)
    {
        Vector3Int gridPos = CoordinatesToGrid(coordinates);
        for (int x = gridPos.x; x < gridPos.x + size.x; x++)
        {
            for (int z = gridPos.z; z < gridPos.z + size.y; z++)
            {


                if (x >= 0 && x < gridWidth && z >= 0 && z < gridHeight)
                {
                    grid[x, z] = false;

                }
            }
        }

        for (int i = gridPos.x - range; i <= gridPos.x + size.x + range; i++)
        {
            for (int j = gridPos.z - range; j <= gridPos.z + size.y + range; j++)
            {   
                float distSquared = Mathf.Pow(i - gridPos.x - size.x/2, 2) + Mathf.Pow(j - gridPos.z - size.y/2, 2);
                if (distSquared < Mathf.Pow(range, 2) && territoryGrid[i, j] != 2) //Territory radius of 3 around the target
                {
                    territoryGrid[i, j] = 2; //TODO: When implementing wave system, change to 1
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
        int x = Mathf.FloorToInt(coordinates.x / tileSize) + gridWidth/2;
        int z = Mathf.FloorToInt(coordinates.z / tileSize) + gridHeight/2;
        return new Vector3Int(x,0,z);
    }

    public Vector3 GridToCoordinates(Vector3 gridCoords)
    {
        return new Vector3((gridCoords.x - gridWidth/2) * tileSize, 0f, (gridCoords.z - gridHeight/2) * tileSize);
    }

    public Quaternion SnapRotation(Quaternion currentRotation)
    {
        float yRotation = currentRotation.eulerAngles.y;
        yRotation = Mathf.Round(yRotation / 90f) * 90f;

        return Quaternion.Euler(0f, yRotation, 0f);
    }
}
