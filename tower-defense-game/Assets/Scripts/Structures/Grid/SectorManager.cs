using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;


// TODO: STORE A GLOBAL LIST TO THE BASE FOR ENEMIES WHEN LEVEL STARTS

public class SectorManager : MonoBehaviour
{
    public static SectorManager Instance { get; private set; }
    private GridNode[,] grid;
    private GridSector[,] sectors = new GridSector[GridManager.gridWidth / GridSector.sectorWidth, GridManager.gridHeight / GridSector.sectorHeight];

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

    void Start()
    {
        grid = GridManager.Instance.GetGrid();
    }

    public static void InitializeSector(int2 sectorCoordinates, GridNode[,] globalGrid)
    {
        GridSector newSector = new GridSector(sectorCoordinates);
        GridNode[,] newSectorLocalGrid = new GridNode[GridSector.sectorWidth, GridSector.sectorHeight];
        for (int x = 0; x < GridSector.sectorWidth; x++)
        {
            for (int y = 0; y < GridSector.sectorHeight; y++)
            {
                GridNode newGridNode = new GridNode(sectorCoordinates.x + x, sectorCoordinates.y + y);
                newSectorLocalGrid[x, y] = newGridNode;
                globalGrid[x, y] = newGridNode;

            }
        }
    }

    private void CheckAllSectorConnections()
    {
        int2[] directions = {
            new int2(0,1),
            new int2(1,0),
            new int2(0,-1),
            new int2(-1,0)
            };

        for (int x = 0; x < sectors.GetLength(0); x++)
        {
            for (int y = 0; y < sectors.GetLength(1); y++)
            {
                for (int i = 0; i < 4; i++)
                {
                    sectors[x, y].neighbours[i] =
                    ComputeConnectivity(
                        sectors[x, y],
                        sectors[x + directions[i].x, y + directions[i].y],
                        i);

                }
            }
        }
    }

    private bool ComputeConnectivity(GridSector from, GridSector to, int cardinality)
    {
        // again cardinality corresponds to enum, North is cardinality = 0
        if (cardinality == 0)
        {
            for (int x = 0; x < GridSector.sectorWidth; x++)
            {
                if (from.localGrid[x, GridSector.sectorHeight - 1].walkCost == float.MaxValue &&
                    to.localGrid[x, 0].walkCost == float.MaxValue)
                {
                    return false;
                }
            }
        }
        else if (cardinality == 1)
        {
            for (int y = 0; y < GridSector.sectorHeight; y++)
            {
                if (from.localGrid[GridSector.sectorWidth - 1, y].walkCost == float.MaxValue &&
                    to.localGrid[0, y].walkCost == float.MaxValue)
                {
                    return false;
                }
            }
        }
        else if (cardinality == 2)
        {
            for (int x = 0; x < GridSector.sectorWidth; x++)
            {
                if (from.localGrid[x, 0].walkCost == float.MaxValue &&
                    to.localGrid[x, GridSector.sectorHeight - 1].walkCost == float.MaxValue)
                {
                    return false;
                }
            }
        }
        else
        {
            for (int y = 0; y < GridSector.sectorHeight; y++)
            {
                if (from.localGrid[0, y].walkCost == float.MaxValue &&
                    to.localGrid[GridSector.sectorHeight - 1, y].walkCost == float.MaxValue)
                {
                    return false;
                }
            }
        }
        return true;
    } 
    
    public List<GridSector> GenerateHighLevelSectorPath(GridSector from, GridSector to)
    {
        PriorityQueue<int2, float> openSet = new PriorityQueue<GridSector, float>();
        Dictionary<int2, int2> cameFrom = new Dictionary<GridSector, GridSector>();
        Dictionary<int2, float> fScore = new Dictionary<int2, float>();
        Dictionary<int2, float> gScore = new Dictionary<int2, float>();
        openSet.Enqueue(from.gridCoords, fScore[from.gridCoords]);
    }
}
