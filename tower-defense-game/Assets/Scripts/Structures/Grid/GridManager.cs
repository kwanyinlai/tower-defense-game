using UnityEngine;
using System.Collections.Generic;

public struct GridNode
{
    public GridNode(int x, int y){
        walkable = false;
        territoryStatus = (int)TerritoryStatus.NotAssigned;
        buildable = false;
        this.x = x;
        this.y = y;
    }

    public bool walkable;
    public int territoryStatus; // refer to TerritoryStatus enum
    public bool buildable;
    public int x;
    public int y;
}

public class GridManager : MonoBehaviour
{

    public static int gridWidth = 200;
    public static int gridHeight = 200;
    public static float tileSize = 4f;

    private bool[,] grid; // true means buildable, false means occupied
    private List<Transform> playerPos;

    
    private int[,] territoryGrid; // corresponds to enum TerritoryStatus
    public GameObject target;
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
        territoryGrid = new int[gridWidth, gridHeight];

        playerPos = new List<Transform>();
        buildableTiles = new List<GameObject>();

        grid = new bool[gridWidth, gridHeight];
        
        

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                grid[x, z] = true;
                territoryGrid[x, z] = (int)TerritoryStatus.NotAssigned;
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
                    territoryGrid[i, j] = (int)TerritoryStatus.Assigned;
                }
            }
        }
    }


    public bool IsTileBuildable(Vector3Int coordinates)
    {
        // check if tile is occupied by player
        foreach (GameObject player in Player.players)
        {
            if (coordinates == CoordinatesToGrid(player.GetComponent<Transform>().position))
            {
                // TODO: this is slgihtly off rn but I'll fix it later // later note, what is off about this??
                return false;
            }
        }

        if (coordinates.x < 0 || coordinates.x >= gridWidth || coordinates.z < 0 || coordinates.z >= gridHeight ||
            territoryGrid[coordinates.x, coordinates.z] == (int)TerritoryStatus.NotAssigned) 
        {
            return false;
            
        }


        return grid[coordinates.x, coordinates.z];
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

                if (distSquared < Mathf.Pow(range, 2) && territoryGrid[i, j] != (int)TerritoryStatus.Assigned) 
                {
                    territoryGrid[i, j] = (int)TerritoryStatus.AssigneOnWaveUpdate;
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
                if (grid[x, z] && territoryGrid[x,z] == (int)TerritoryStatus.Assigned)
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
        for(int i = 0; i < territoryGrid.GetLength(0); i++)
        {
            for(int j = 0; j < territoryGrid.GetLength(1); j++)
            {
                if (territoryGrid[i, j] == (int)TerritoryStatus.AssigneOnWaveUpdate)
                {
                    territoryGrid[i, j] = (int)TerritoryStatus.Assigned;
                }
                else if (territoryGrid[i, j] == (int)TerritoryStatus.RemoveOnWaveUpdate)
                {
                    territoryGrid[i, j] = (int)TerritoryStatus.NotAssigned;
                }
            }
        }
    }


    public void UnassignTerritory(){
        starterTerritoryIsAssigned = false;
    }
    
}