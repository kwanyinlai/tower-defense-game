using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

[System.Serializable]
public struct GridNode
{
    public GridNode(int x, int y) // might have to assign walk cost later on creation
    {
        walkCost = 1;
        territoryStatus = (int)GridManager.TerritoryStatus.NotAssigned;
        buildable = true;
        coord = new Vector2(x, y);
    }


    public float walkCost;
    // public int sectorID; // TODO: Figure out how to not have to initialise this on creation
    // but later in the code
    public int territoryStatus; // refer to TerritoryStatus enum
    public bool buildable;
    public Vector2 coord;

}

public class GridManager : MonoBehaviour
{
    public static readonly int gridWidth = 200; // number of tiles
    public static readonly int gridHeight = 200;
    public static readonly float tileSize = 4f;

    // private bool[,] grid; // true means buildable, false means occupied
    private List<Transform> playerPos;

    private GridNode[,] grid;
    public GridNode[,] GetGrid(){
        return grid;
    }

    // private int[,] territoryGrid; // corresponds to enum TerritoryStatus

    private GameObject target;
    public GameObject Target
    {
        get { return target; }
        set { target = value; }
    }
    private bool starterTerritoryIsAssigned;

    public static GridManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public enum TerritoryStatus
    {
        NotAssigned = 0,
        AssigneOnWaveUpdate = 1,
        Assigned = 2,
        RemoveOnWaveUpdate = 3
    }

    [Header("Buildable Tiles")]
    [SerializeField] private GameObject buildableTilePrefab;
    [SerializeField] private Material buildableTileMaterial;

    private List<GameObject> buildableTiles;

    public static Vector3Int CoordinatesToGrid(Vector3 coordinates)
    {
        int x = Mathf.FloorToInt(coordinates.x / tileSize) + gridWidth/2; // this might be broken i think
        int z = Mathf.FloorToInt(coordinates.z / tileSize) + gridHeight/2;
        return new Vector3Int(x,0,z);
    }

    public static Vector3 GridToCoordinates(Vector3 gridCoords)
    {
        return new Vector3((gridCoords.x - gridWidth/2) * tileSize, 0f, (gridCoords.z - gridHeight/2) * tileSize);
    }

    void Start()
    {
        grid = new GridNode[gridWidth, gridHeight];

        playerPos = new List<Transform>();
        
        buildableTiles = new List<GameObject>();

        // // Set up global grid

        // for (int x = 0; x < gridWidth; x++)
        // {
        //     for (int z = 0; z < gridHeight; z++)
        //     {
        //         grid[x, z] = new GridNode(x, z);
        //     }
        // }
        
        // Set up global grid and sectors at the same time

        for (int x = 0; x < gridWidth / GridSector.sectorWidth; x++)
        {
            for (int y = 0; y < gridHeight / GridSector.sectorHeight; y++)
            {
                SectorManager.InitializeSector(new int2(x, y), grid);
            }
        }

        // starterTerritoryIsAssigned = false;

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
                if(distSquared < 25)
                {
                    grid[i, j].territoryStatus = (int)TerritoryStatus.Assigned;
                }
            }
        }
    }


    public bool IsTileBuildable(Vector3Int coordinates)
    {
        // check if tile is occupied by player
        foreach (GameObject player in PlayerManager.players)
        {
            if (coordinates == CoordinatesToGrid(player.GetComponent<Transform>().position))
            {
                // TODO: this is slgihtly off rn but I'll fix it later // later note, what is off about this??
                return false;
            }
        }

        if (coordinates.x < 0 || coordinates.x >= gridWidth || coordinates.z < 0 || coordinates.z >= gridHeight ||
            grid[coordinates.x, coordinates.z].territoryStatus == (int)TerritoryStatus.NotAssigned) 
        {
            return false;
            
        }

        return grid[coordinates.x, coordinates.z].buildable;
    }

    public bool IsTileAreaBuildable(Vector3 coordinates, int2 size)
    {
        Vector3 gridCoords = CoordinatesToGrid(coordinates);
        for (int x = (int) gridCoords.x; x < (int) gridCoords.x + size.x; x++)
        {
            for (int z = (int) gridCoords.z; z < (int) gridCoords.z + size.y; z++)
            {
                // if (!IsTileBuildable(new Vector3Int(x, 0, z))){
                //     return false;
                // } // This is very inefficient
                if (!grid[x,z].buildable){
                    return false;
                }
            }
        }
        return true;
    }

    

    public void OccupyArea(Vector3 coordinates, int2 size, int range)
    {
        Vector3Int gridPos = CoordinatesToGrid(coordinates);
        for (int x = gridPos.x; x < gridPos.x + size.x; x++)
        {
            for (int z = gridPos.z; z < gridPos.z + size.y; z++)
            {
                if (x >= 0 && x < gridWidth && z >= 0 && z < gridHeight)
                {
                    grid[x, z].buildable = false;
                }
            }
        }

        for (int i = gridPos.x - range; i <= gridPos.x + size.x + range; i++)
        {
            for (int j = gridPos.z - range; j <= gridPos.z + size.y + range; j++)
            {   
                float distSquared = Mathf.Pow(i - gridPos.x - size.x/2, 2) + Mathf.Pow(j - gridPos.z - size.y/2, 2);

                if (distSquared < Mathf.Pow(range, 2) && grid[i, j].territoryStatus != (int)TerritoryStatus.Assigned) 
                {
                    grid[i, j].territoryStatus = (int)TerritoryStatus.AssigneOnWaveUpdate;
                }
            }
        }
    }
    public void StopOccupying(Vector3 coordinates, int2 size)
    {
        
        Vector3Int gridPos = CoordinatesToGrid(coordinates);
        for (int x = gridPos.x; x < gridPos.x + size.x; x++)
        {
            for (int z = gridPos.z; z < gridPos.z + size.y; z++)
            {
                if (x >= 0 && x < gridWidth && z >= 0 && z < gridHeight)
                {
                    grid[x, z].buildable = true;
                }
            }
        }
    }


    public static Quaternion SnapRotation(Quaternion currentRotation)
    {
        float yRotation = currentRotation.eulerAngles.y;
        yRotation = Mathf.Round(yRotation / 90f) * 90f;

        return Quaternion.Euler(0f, yRotation, 0f);
    }

    public void DrawBuildGrid()
    {
        // ClearGrid();
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                if (grid[x, z].buildable && grid[x,z].territoryStatus == (int)TerritoryStatus.Assigned)
                {
                    DrawBuildableSquare(GridToCoordinates(new Vector3Int(x, 0, z)));
                }
            }
        }
    }

    private void DrawBuildableSquare(Vector3 position)
    {
        GameObject buildableTile = GameObject.CreatePrimitive(PrimitiveType.Quad);
        buildableTiles.Add(buildableTile);

        buildableTile.transform.position = position + new Vector3(0, 0.1f, 0); // raise slightly above ground to be seen
        buildableTile.transform.localScale = new Vector3(tileSize, tileSize, tileSize);

        buildableTile.GetComponent<Renderer>().material = buildableTileMaterial;
        buildableTile.GetComponent<Renderer>().material.color = new Color(0, 0, 1, 0.5f);

        buildableTile.transform.parent = buildableTilePrefab.transform;

        buildableTile.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    public void ClearBuildGrid()
    {
        foreach (GameObject tile in buildableTiles)
        {
            Destroy(tile);
        }
    }

    void Update(){
        if(target==null){
            target = BaseManager.Instance.GetBase();
        }
        else{
            if(!starterTerritoryIsAssigned){
                SetStarterTerritory();
                starterTerritoryIsAssigned = true;
            }
        }
    }

    
    public void TerritoryUpdate()
    {
        for(int i = 0; i < grid.GetLength(0); i++)
        {
            for(int j = 0; j < grid.GetLength(1); j++)
            {
                if (grid[i, j].territoryStatus == (int)TerritoryStatus.AssigneOnWaveUpdate)
                {
                    grid[i, j].territoryStatus = (int)TerritoryStatus.Assigned;
                }
                else if (grid[i, j].territoryStatus == (int)TerritoryStatus.RemoveOnWaveUpdate)
                {
                    grid[i, j].territoryStatus = (int)TerritoryStatus.NotAssigned;
                }
            }
        }
    }


    public void UnassignTerritory(){
        starterTerritoryIsAssigned = false;
    }
    
}



