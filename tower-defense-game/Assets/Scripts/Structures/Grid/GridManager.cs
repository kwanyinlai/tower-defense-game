using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

public class GridManager : MonoBehaviour

{
    #region Constants
    public static readonly int GRID_WIDTH = 200; // number of tiles in width
    public static readonly int GRID_HEIGHT = 200;
    private static int TERRITORY_RADIUS = 5;
    public static readonly float TILE_SIZE = 4f;
    public enum TerritoryStatus
    {
        NotAssigned = 0,
        AssigneOnWaveUpdate = 1,
        Assigned = 2,
        RemoveOnWaveUpdate = 3
    }
    #endregion

    #region Getters and Setters
    public GameObject PlayerBase
    {
        get { return playerBase; }
        set { playerBase = value; }
    }

    public static GridManager Instance { get; private set; } // singleton

    #endregion

    #region Buildable Tiles Overlay Fields
    [Header("Buildable Tiles")]
    [SerializeField] private GameObject buildableTilePrefab;
    [SerializeField] private Material buildableTileMaterial;

    private List<GameObject> buildableTiles;
    #endregion

    #region Territory Management Fields
    private bool starterTerritoryIsAssigned = false;
    private HashSet<int2> pendingTerritoryChanges = new HashSet<int2>();
    #endregion
    private GridNode[,] grid; 
    private GameObject playerBase;

    #region Initialization Methods

    
    private void InitializeGridAndSectors()
    {
        grid = new GridNode[GRID_WIDTH, GRID_HEIGHT];

        int numSectorsX = GRID_WIDTH / GridSector.sectorWidth;
        int numSectorsY = GRID_HEIGHT / GridSector.sectorHeight;
        GridSector[,] sectors = new GridSector[numSectorsX, numSectorsY];

        // initialise all sectors
        for (int sectorX = 0; sectorX < numSectorsX; sectorX++)
        {
            for (int sectorY = 0; sectorY < numSectorsY; sectorY++)
            {
                GridSector sector = new GridSector(new int2(sectorX, sectorY));
                sectors[sectorX, sectorY] = sector;

                // create nodes for this sector and add it to the main grid
                for (int localX = 0; localX < GridSector.sectorWidth; localX++)
                {
                    for (int localY = 0; localY < GridSector.sectorHeight; localY++)
                    {
                        int2 globalPos = new int2(sectorX * GridSector.sectorWidth + localX, sectorY * GridSector.sectorHeight + localY);

                        GridNode node = new GridNode(globalPos);
                        
                        grid[globalPos.x, globalPos.y] = node;
                        sector.localGrid[localX, localY] = node;
                        
                        // TODO: bidirectional references might not be cleanest, but we'll use them for now
                        node.gridSector = sector;
                        int2 localPos = new int2(localX, localY);
                        node.localPos = localPos;
                    }
                }

                sector.AggregateCosts();
            }
        }

        SectorManager.Instance.SetSectors(sectors);
        // IMPORTANT NOTE: sector's local grids stores references to the same GridNodes as the main grid

        ConnectSectorNeighbors(sectors);
    }

    private void ConnectSectorNeighbors(GridSector[,] sectors)
    {
        int numSectorsX = sectors.GetLength(0);
        int numSectorsY = sectors.GetLength(1);

        for (int x = 0; x < numSectorsX; x++)
        {
            for (int y = 0; y < numSectorsY; y++)
            {
                GridSector sector = sectors[x, y];

                // assign neighbors in each cardinal direction
                sector.neighbours[(int)GridSector.CardinalDirections.North] = 
                    (y < numSectorsY - 1) ? sectors[x, y + 1] : null;
                    
                sector.neighbours[(int)GridSector.CardinalDirections.East] = 
                    (x < numSectorsX - 1) ? sectors[x + 1, y] : null;
                    
                sector.neighbours[(int)GridSector.CardinalDirections.South] = 
                    (y > 0) ? sectors[x, y - 1] : null;
                    
                sector.neighbours[(int)GridSector.CardinalDirections.West] = 
                    (x > 0) ? sectors[x - 1, y] : null;

                sector.GenerateCostFieldForBorders();
            }
        }
    }

    // void SetStarterTerritory()
    // {
    //     Vector3 pos = playerBase.transform.position;
    //     Vector3Int gridPos = WorldPosFromCoordinates(pos);
    //     for(int i = gridPos.x - TERRITORY_RADIUS; i <= gridPos.x + TERRITORY_RADIUS; i++)
    //     {
    //         for(int j =  gridPos.z - TERRITORY_RADIUS; j <= gridPos.z + TERRITORY_RADIUS; j++)
    //         {
    //             float distSquared = Mathf.Pow(i - gridPos.x, 2) + Mathf.Pow(j - gridPos.z, 2);
    //             if(distSquared < Mathf.Pow(TERRITORY_RADIUS, 2))
    //             {
    //                 grid[i, j].territoryStatus = (int)TerritoryStatus.Assigned;
    //             }
    //         }
    //     }
    // }

    #endregion

    #region Buildable Checks

    public bool IsTileBuildable(Vector3Int coordinates)
    {
        // check if tile is occupied by player
        foreach (GameObject player in PlayerManager.players)
        {
            if (coordinates == WorldPosFromCoordinates(player.GetComponent<Transform>().position))
            {
                // TODO: this is slgihtly off rn but I'll fix it later // later note, what is off about this??
                return false;
            }
        }

        if (coordinates.x < 0 || coordinates.x >= GRID_WIDTH || coordinates.z < 0 || coordinates.z >= GRID_HEIGHT)
        {
            return false;
        }

        return grid[coordinates.x, coordinates.z].buildable &&
            grid[coordinates.x, coordinates.z].territoryStatus == (int)TerritoryStatus.Assigned;
    }

    public bool IsTileAreaBuildable(Vector3 coordinates, int2 size)
    {
        Vector3 gridCoords = WorldPosFromCoordinates(coordinates);
        for (int x = (int) gridCoords.x; x < (int) gridCoords.x + size.x; x++)
        {
            for (int z = (int) gridCoords.z; z < (int) gridCoords.z + size.y; z++)
            {
                if (!IsTileBuildable(new Vector3Int(x,0,z))){
                    return false;
                } 
            }
        }
        return true;
    }
    #endregion

    #region Occupy / Unoccupy Tiles
    public void OccupyArea(Vector3 coordinates, int2 size, int range)
    {
        Vector3Int gridPos = WorldPosFromCoordinates(coordinates);
        for (int x = gridPos.x; x < gridPos.x + size.x; x++)
        {
            for (int z = gridPos.z; z < gridPos.z + size.y; z++)
            {
                if (x >= 0 && x < GRID_WIDTH
         && z >= 0 && z < GRID_HEIGHT)
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
        
        Vector3Int gridPos = WorldPosFromCoordinates(coordinates);
        for (int x = gridPos.x; x < gridPos.x + size.x; x++)
        {
            for (int z = gridPos.z; z < gridPos.z + size.y; z++)
            {
                if (x >= 0 && x < GRID_WIDTH
        && z >= 0 && z < GRID_HEIGHT)
                {
                    grid[x, z].buildable = true;
                }
            }
        }

    }

    #endregion

    #region Buildable Tiles Overlay Methods
    public void DrawBuildableGridOverlay()
    {
        // ClearBuildableGridOverlay();
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int z = 0; z < GRID_HEIGHT; z++)
            {
                if (grid[x, z].buildable && grid[x,z].territoryStatus == (int)TerritoryStatus.Assigned)
                {
                    DrawBuildableSquare(CoordinatesToWorldPos(new Vector3Int(x, 0, z)));
                }
            }
        }
    }

    private void DrawBuildableSquare(Vector3 position)
    {
        GameObject buildableTile = GameObject.CreatePrimitive(PrimitiveType.Quad);
        buildableTiles.Add(buildableTile);

        buildableTile.transform.position = position + new Vector3(0, 0.1f, 0); // raise slightly above ground to be seen
        buildableTile.transform.localScale = new Vector3(TILE_SIZE, TILE_SIZE, TILE_SIZE);

        buildableTile.GetComponent<Renderer>().material = buildableTileMaterial;
        buildableTile.GetComponent<Renderer>().material.color = new Color(0, 0, 1, 0.5f);

        buildableTile.transform.parent = buildableTilePrefab.transform;

        buildableTile.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    public void ClearBuildableGridOverlay()
    {
        foreach (GameObject tile in buildableTiles)
        {
            Destroy(tile);
        }
    }
    #endregion

    #region Coordinate Conversion Methods
      
    public static Vector3Int WorldPosFromCoordinates(Vector3 coordinates)
    {
        int x = Mathf.FloorToInt(coordinates.x / TILE_SIZE) + GRID_WIDTH / 2; // centred on 0,0
        int z = Mathf.FloorToInt(coordinates.z / TILE_SIZE) + GRID_HEIGHT / 2;
        return new Vector3Int(x, 0, z);
    }
    

    public static Vector3 CoordinatesToWorldPos(Vector3 gridCoords)
    {
        return new Vector3((gridCoords.x - GRID_WIDTH / 2) * TILE_SIZE, 0f, (gridCoords.z - GRID_HEIGHT / 2) * TILE_SIZE);
    }

    public static Vector3 CoordinatesToWorldPos(int2 gridCoords)
    {
        return new Vector3((gridCoords.x - GRID_WIDTH / 2) * TILE_SIZE, 0f, (gridCoords.y - GRID_HEIGHT / 2) * TILE_SIZE);
    }
    #endregion

    #region Utility Methods

    public bool NodeIsInBounds(int2 pos)
    {
        return pos.x >= 0 && pos.x < GRID_WIDTH && pos.y >= 0 && pos.y < GRID_HEIGHT;
    }
    
    public static float Distance (GridNode node1, GridNode node2)
    {
        return Mathf.Sqrt((node1.globalPos.x - node2.globalPos.x) * (node1.globalPos.x - node2.globalPos.x)+ (node1.globalPos.y - node2.globalPos.y) *  (node1.globalPos.y - node2.globalPos.y));
    }
    public static Quaternion SnapRotation(Quaternion currentRotation)
    {
        float yRotation = currentRotation.eulerAngles.y;
        yRotation = Mathf.Round(yRotation / 90f) * 90f;

        return Quaternion.Euler(0f, yRotation, 0f);
    }

    public GridNode NodeFromWorldPos(Vector3 coordinates)
    {
        int x = Mathf.FloorToInt(coordinates.x / TILE_SIZE) + GRID_WIDTH / 2; 
        int z = Mathf.FloorToInt(coordinates.z / TILE_SIZE) + GRID_HEIGHT / 2;
        if (x < 0 || x >= GRID_WIDTH || z < 0 || z >= GRID_HEIGHT)
        {
            return null;
        }
        return grid[x, z];
    }

    public GridNode NodeFromGridCoordinate(Vector3Int gridCoords)
    {
        if (gridCoords.x < 0 || gridCoords.x >= GRID_WIDTH || gridCoords.z < 0 || gridCoords.z >= GRID_HEIGHT)
        {
            return null;
        }
        return grid[gridCoords.x, gridCoords.z];
    }
    public GridNode NodeFromGridCoordinate(int2 gridCoords)
    {
        if (gridCoords.x < 0 || gridCoords.x >= GRID_WIDTH || gridCoords.y < 0 || gridCoords.y >= GRID_HEIGHT)
        {
            return null;
        }
        return grid[gridCoords.x, gridCoords.y];
    }
    #endregion

    public GridNode[,] GetGrid() // TODO: probably want to replace this -- unsafe to let people have access to grid
    {
        return grid;
    }
    /*
    Commenting out territory management for now. Want to remove territory altogether.
    #region Territory Management
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
    #endregion
    */
    void Start()
    {
        InitializeGridAndSectors();
    }
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

    void Update(){
        if(playerBase == null){
            playerBase = BaseManager.Instance.GetBase();
        }
        else{
            if(!starterTerritoryIsAssigned){
                // SetStarterTerritory();
                starterTerritoryIsAssigned = true;
            }
        }
    }

}



