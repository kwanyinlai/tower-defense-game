using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

[System.Serializable]
public class GridNode
{
    public float walkCost = 1f;
    public int territoryStatus; // refer to TerritoryStatus enum
    public bool buildable;
    public int2 globalPos; // coordinates in global grid
    public int2 localPos; // coordinates in local sector grid.
    public GridSector gridSector; // which grid sector it belongs to
    public GridNode(int2 globalPos, float walkCost = 1f) // might have to assign walk cost later on creation
    {
        walkCost = 1f;
        territoryStatus = (int)GridManager.TerritoryStatus.NotAssigned;
        buildable = true;
        this.globalPos = globalPos;
    }

}

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
    public GridNode[,] GetGrid(){
        return grid;
    }

    public GameObject Target
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
    private GridNode[,] grid; 
    private GameObject playerBase;
    private bool starterTerritoryIsAssigned = false;

    #region Initialization Methods
    void Start()
    {
        grid = new GridNode[GRID_WIDTH, GRID_HEIGHT];

        buildableTiles = new List<GameObject>();

        GridSector[,] sectors = new GridSector[GRID_WIDTH / GridSector.sectorWidth, GRID_HEIGHT / GridSector.sectorHeight];

        // new code to generate grid and sectors at the same time

        for (int x = 0; x < sectors.GetLength(0); x++)
        {
            for (int y = 0; y < sectors.GetLength(1); y++)
            {
                GridSector sector = new GridSector(new int2(x, y));
                sectors[x, y] = sector;

                for (int i = 0; i < GridSector.sectorWidth; i++)
                {
                    for (int j = 0; j < GridSector.sectorHeight; j++)
                    {
                        int2 globalPos = new int2(x * GridSector.sectorWidth + i, y * GridSector.sectorHeight + j);
                        GridNode newNode = new GridNode(globalPos);
                        grid[globalPos.x, globalPos.y] = newNode;
                        newNode.gridSector = sector;
                        sector.localGrid[i, j] = newNode;
                        newNode.localPos.x = i;
                        newNode.localPos.y = j;
                    }
                }
               
                
                sector.AggregateCosts();
                
            }
        }
        SectorManager.Instance.SetSectors(sectors); // initialize sectors in SectorManager

        // Generate sector neighbors
        for (int x = 0; x < sectors.GetLength(0); x++)
        {
            for (int y = 0; y < sectors.GetLength(1); y++)
            {
                GridSector sector = sectors[x, y];
                sector.neighbours[(int)GridSector.CardinalDirections.North] = (y < sectors.GetLength(1) - 1) ? sectors[x, y + 1] : null;
                sector.neighbours[(int)GridSector.CardinalDirections.East] = (x < sectors.GetLength(0) - 1) ? sectors[x + 1, y] : null;
                sector.neighbours[(int)GridSector.CardinalDirections.South] = (y > 0) ? sectors[x, y - 1] : null;
                sector.neighbours[(int)GridSector.CardinalDirections.West] = (x > 0) ? sectors[x - 1, y] : null;
                // GenerateCostFieldForBorders() requires grid to be fully initialised, which is not the case
                // if done in the previous for loop
                sector.GenerateCostFieldForBorders();
            }
        }

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
    void SetStarterTerritory()
    {
        Vector3 pos = playerBase.transform.position;
        Vector3Int gridPos = CoordinatesToGrid(pos);
        for(int i = gridPos.x - TERRITORY_RADIUS; i <= gridPos.x + TERRITORY_RADIUS; i++)
        {
            for(int j =  gridPos.z - TERRITORY_RADIUS; j <= gridPos.z + TERRITORY_RADIUS; j++)
            {
                float distSquared = Mathf.Pow(i - gridPos.x, 2) + Mathf.Pow(j - gridPos.z, 2);
                if(distSquared < Mathf.Pow(TERRITORY_RADIUS, 2))
                {
                    grid[i, j].territoryStatus = (int)TerritoryStatus.Assigned;
                }
            }
        }
    }

    #endregion

    #region Buildable Checks

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

        if (coordinates.x < 0 || coordinates.x >= GRID_WIDTH || coordinates.z < 0 || coordinates.z >= GRID_HEIGHT)
        {
            return false;
        }

        return grid[coordinates.x, coordinates.z].buildable &&
            grid[coordinates.x, coordinates.z].territoryStatus == (int)TerritoryStatus.Assigned;
    }

    public bool IsTileAreaBuildable(Vector3 coordinates, int2 size)
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
    #endregion

    #region Occupy / Unoccupy Tiles
    public void OccupyArea(Vector3 coordinates, int2 size, int range)
    {
        Vector3Int gridPos = CoordinatesToGrid(coordinates);
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
        
        Vector3Int gridPos = CoordinatesToGrid(coordinates);
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
    public void DrawBuildGrid()
    {
        // ClearGrid();
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int z = 0; z < GRID_HEIGHT; z++)
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
        buildableTile.transform.localScale = new Vector3(TILE_SIZE, TILE_SIZE, TILE_SIZE);

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
    #endregion

    #region Utility Methods

    public bool IsInBounds(int2 pos)
    {
        return pos.x >= 0 && pos.x < GRID_WIDTH && pos.y >= 0 && pos.y < GRID_HEIGHT;
    }
    /// <summary>
    /// Euclidean Distance between two GridNodes
    /// </summary>
    public static float Distance (GridNode node1, GridNode node2)
    {
        return Mathf.Sqrt((node1.globalPos.x - node2.globalPos.x) * (node1.globalPos.x - node2.globalPos.x)+ (node1.globalPos.y - node2.globalPos.y) *  (node1.globalPos.y - node2.globalPos.y));
    }
    
    public static Vector3Int CoordinatesToGrid(Vector3 coordinates)
    {
        int x = Mathf.FloorToInt(coordinates.x / TILE_SIZE) + GRID_WIDTH / 2; // centred on 0, 0
        int z = Mathf.FloorToInt(coordinates.z / TILE_SIZE) + GRID_HEIGHT / 2;
        return new Vector3Int(x, 0, z);
    }

    public static Vector3 GridToCoordinates(Vector3 gridCoords)
    {
        return new Vector3((gridCoords.x - GRID_WIDTH / 2) * TILE_SIZE, 0f, (gridCoords.z - GRID_HEIGHT / 2) * TILE_SIZE);
    }
    public static Quaternion SnapRotation(Quaternion currentRotation)
    {
        float yRotation = currentRotation.eulerAngles.y;
        yRotation = Mathf.Round(yRotation / 90f) * 90f;

        return Quaternion.Euler(0f, yRotation, 0f);
    }

    /// <summary>
    /// Get the GridNode corresponding to world coordinates in 3D space
    /// </summary>
    /// <param name="coordinates"></param>
    /// <returns>GridNode</returns>
    public GridNode NodeFromWorldPos(Vector3 coordinates)
    {
        int x = Mathf.FloorToInt(coordinates.x / TILE_SIZE) + GRID_WIDTH / 2; 
        int z = Mathf.FloorToInt(coordinates.z / TILE_SIZE) + GRID_HEIGHT / 2;
        return grid[x, z];
    }
    #endregion

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


    void Update(){
        if(playerBase == null){
            playerBase = BaseManager.Instance.GetBase();
        }
        else{
            if(!starterTerritoryIsAssigned){
                SetStarterTerritory();
                starterTerritoryIsAssigned = true;
            }
        }
    }

}



