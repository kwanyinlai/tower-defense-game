using UnityEngine;
using System.Collections.Generic;

public class GridSystem : MonoBehaviour
{

    // TODO: build menu for selecting buildings
    public static int gridWidth = 200;
    public static int gridHeight = 200;
    public static float tileSize = 4f;



    private bool[,] grid;
    private int[,] territoryGrid; //0 for not territory, 1 not -> territory, 2 for territory, 3 for territory -> not
    public GameObject target;
    private bool starterTerritoryIsAssigned;
    private List<Transform> playerPos;


    [SerializeField] private GameObject buildableSquares;
    [SerializeField] private Material tileMaterial;


    public void StarterTerritoryNotAssigned(){
        starterTerritoryIsAssigned = false;
    }

    void Start()
    {
        playerPos = new List<Transform>();
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
        starterTerritoryIsAssigned = false;

        
    }
    


    void SetStarterTerritory()
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
        DrawGrid();
    }

    void Update(){
        if(target==null){
            target = GameObject.Find("base-manager").GetComponent<BaseManager>().GetBase();
        }
        else{
            if(!starterTerritoryIsAssigned){
                SetStarterTerritory();
                starterTerritoryIsAssigned = true;
            }
        }
    }


    void GetFloorSize(){

    }

    public bool IsTileBuildable(Vector3Int coordinates)
    {

        
        // check if tile is occupied by player
        foreach (GameObject player in Player.players)
        {
            Debug.Log(coordinates);
            Debug.Log(CoordinatesToGrid(player.GetComponent<Transform>().position) + "e");
            if (coordinates == CoordinatesToGrid(player.GetComponent<Transform>().position))
            {
                return false;
            }
        }

        if (coordinates.x < 0 || coordinates.x >= gridWidth || coordinates.z < 0 || coordinates.z >= gridHeight ||
            !grid[coordinates.x, coordinates.z] || territoryGrid[coordinates.x, coordinates.z] != 2)
        {
            return false;

        }

  
        return true;
    }

    public bool IsTileAreaBuildable(Vector3 coordinates, Vector2Int size)
    {
 
        Vector3 gridCoords = CoordinatesToGrid(coordinates);
        for (int x = (int) gridCoords.x; x < (int) gridCoords.x + size.x; x++)
        {
            for (int z = (int) gridCoords.z; z < (int) gridCoords.z + size.y; z++)
            {
                if (!IsTileBuildable(new Vector3Int(x, 0, z))){
                    return false;
                }
            }
        }
        return true;
    }

    public void updateWaveTerritory()
    {
        for(int i = 0; i < territoryGrid.GetLength(0); i++)
        {
            for(int j = 0; j < territoryGrid.GetLength(1); j++)
            {
                if (territoryGrid[i, j] == 1)
                {
                    territoryGrid[i, j] = 2;
                } else if (territoryGrid[i, j] == 3)
                {
                    territoryGrid[i, j] = 4;
                }
            }
        }
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
                    territoryGrid[i, j] = 1;
                }
            }
        }
    }
    public void StopOccupying(Vector3 coordinates, Vector2Int size)
    {
        
        Vector3Int gridPos = CoordinatesToGrid(coordinates);
        for (int x = gridPos.x; x < gridPos.x + size.x; x++)
        {
            for (int z = gridPos.z; z < gridPos.z + size.y; z++)
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

    public void DrawGrid()
    {
        // ClearGrid();
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                if (territoryGrid[x, z]==2)
                {
                    DrawSquare(GridToCoordinates(new Vector3Int(x, 0, z)));
                }
            }
        }
    }

    private void DrawSquare(Vector3 position)
    {
        GameObject buildableTile = GameObject.CreatePrimitive(PrimitiveType.Quad);
        buildableTile.transform.position = position + new Vector3(0, 0.1f, 0);
        buildableTile.transform.localScale = new Vector3(tileSize, tileSize, tileSize);
        buildableTile.GetComponent<Renderer>().material = tileMaterial;
        buildableTile.GetComponent<Renderer>().material.color = new Color(0, 0, 1, 0.5f);

        buildableTile.transform.parent = buildableSquares.transform;
        buildableTile.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
