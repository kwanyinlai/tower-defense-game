using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;


// TODO: STORE A GLOBAL LIST TO THE BASE FOR ENEMIES WHEN LEVEL STARTS

public class SectorManager : MonoBehaviour
{
    public static SectorManager Instance { get; private set; }
    private GridNode[,] grid;
    private GridSector[,] sectors = new GridSector[GridManager.gridWidth / GridSector.sectorWidth, GridManager.gridHeight / GridSector.sectorHeight];
    private int2[] directions = {
                new int2(0,1),
                new int2(1,0),
                new int2(0,-1),
                new int2(-1,0)
                }; // corresponding to NESW

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

    public List<GridSector> GenerateHighLevelSectorPath(GridSector start, GridSector goal)
    {
        PriorityQueue<GridSector, float> openSet = new PriorityQueue<GridSector, float>();
        Dictionary<GridSector, GridSector> cameFrom = new Dictionary<GridSector, GridSector>();
        Dictionary<GridSector, float> fScore = new Dictionary<GridSector, float>();
        Dictionary<GridSector, float> gScore = new Dictionary<GridSector, float>();
        HashSet<GridSector> inOpenSet = new HashSet<GridSector>();

        openSet.Enqueue(start, fScore[start]);

        while (openSet.Count > 0)
        {
            GridSector current = openSet.Dequeue();
            inOpenSet.Remove(current);

            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }
            for (int i = 0; i < 4; i++)
            {
                var neighbour = sectors[current.sectorCoordinate.x + directions[i].x,
                                    current.sectorCoordinate.y + directions[i].y];
                var tentativeGScore = gScore[current] + neighbour.averageCost; // calc cost


                if (!gScore.ContainsKey(neighbour) || tentativeGScore < gScore[neighbour])
                {
                    cameFrom[neighbour] = current;
                    gScore[neighbour] = tentativeGScore;
                    fScore[neighbour] = tentativeGScore + AStarHeuristic(neighbour, goal);
                    if (!inOpenSet.Contains(neighbour))
                    {
                        openSet.Enqueue(neighbour, fScore[neighbour]);
                        inOpenSet.Add(neighbour);
                    }
                }
            }
        }
        return null;
    }

    private List<GridSector> ReconstructPath(Dictionary<GridSector, GridSector> cameFrom, GridSector current)
    {
        List<GridSector> sectorPath = new List<GridSector>();
        sectorPath.Add(current);
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            sectorPath.Insert(0, current);
        }
        return sectorPath;
    }
    
    private float AStarHeuristic(GridSector from, GridSector to)
    {
        return Mathf.Abs(from.sectorCoordinate.x - to.sectorCoordinate.x) + Mathf.Abs(from.sectorCoordinate.y - to.sectorCoordinate.y);
    } // using Manhattan for now since only 4, but might need to change
}
