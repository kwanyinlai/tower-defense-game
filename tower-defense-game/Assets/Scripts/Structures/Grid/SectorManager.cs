using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

// TODO: STORE A GLOBAL LIST TO THE BASE FOR ENEMIES WHEN LEVEL STARTS
// TODO: maybenot

public class SectorManager : MonoBehaviour
{

    public static SectorManager Instance { get; private set; }

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

    private GridSector[,] sectors;

    // cardinal direction offsets
    private static readonly int2[] DIRECTIONS = {
        new int2(0, 1),   // North
        new int2(1, 0),   // East
        new int2(0, -1),  // South
        new int2(-1, 0)   // West
    };

    #region Initialization

    public void SetSectorList(GridSector[,] sectors)
    {
        this.sectors = sectors;
    }

    #endregion

    #region Sector Connectivity

    public bool AreSectorsAdjacent(GridSector sector1, GridSector sector2)
    {
        return Mathf.Abs(sector1.sectorCoordinate.x - sector2.sectorCoordinate.x) + 
            Mathf.Abs(sector1.sectorCoordinate.y - sector2.sectorCoordinate.y) == 1;
    }

    #endregion

    #region High-Level Pathfinding

    /// <summary>
    /// Generates a high-level path through sectors from start to goal using A* pathfinding
    /// </summary>
    /// <returns>Ordered list of sectors to traverse, or null if no path exists</returns>
    public List<GridSector> GenerateHighLevelSectorPath(GridSector start, GridSector goal)
    {
        if (start == null || goal == null)
        {
            Debug.LogWarning("Cannot generate path: start or goal sector is null");
            return null;
        }

        if (start == goal)
        {
            return new List<GridSector> { start };
        }

        // Initialize A* data structures
        var openSet = new PriorityQueue<GridSector>();
        var cameFrom = new Dictionary<GridSector, GridSector>();
        var gScore = new Dictionary<GridSector, float>();
        var fScore = new Dictionary<GridSector, float>();
        var inOpenSet = new HashSet<GridSector>();

        // Initialize start sector
        gScore[start] = 0f;
        fScore[start] = CalculateHeuristic(start, goal);
        openSet.Enqueue(start, fScore[start]);
        inOpenSet.Add(start);

        // A* main loop
        while (openSet.Count > 0)
        {
            GridSector current = openSet.Dequeue();
            inOpenSet.Remove(current);

            // Goal reached
            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }

            // Explore neighbors
            ExploreNeighbors(current, goal, openSet, cameFrom, gScore, fScore, inOpenSet);
        }

        // No path found
        Debug.LogWarning($"No path found from sector {start.sectorCoordinate} to {goal.sectorCoordinate}");
        return null;
    }

    private void ExploreNeighbors(
        GridSector current,
        GridSector goal,
        PriorityQueue<GridSector> openSet,
        Dictionary<GridSector, GridSector> cameFrom,
        Dictionary<GridSector, float> gScore,
        Dictionary<GridSector, float> fScore,
        HashSet<GridSector> inOpenSet)
    {
        // Check all four cardinal directions
        for (int i = 0; i < 4; i++)
        {
            GridSector neighbor = current.neighbours[i];

            // Skip if no neighbor in this direction
            if (neighbor == null) continue;

            float currentG = gScore.ContainsKey(current) ? gScore[current] : float.PositiveInfinity;
            float tentativeGScore = currentG + neighbor.averageCost;

            // Found a better path to this neighbor
            if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
            {
                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = tentativeGScore + CalculateHeuristic(neighbor, goal);

                // Add to open set if not already there
                if (!inOpenSet.Contains(neighbor))
                {
                    openSet.Enqueue(neighbor, fScore[neighbor]);
                    inOpenSet.Add(neighbor);
                }
            }
        }
    }

    private List<GridSector> ReconstructPath(Dictionary<GridSector, GridSector> cameFrom, GridSector current)
    {
        var path = new List<GridSector> { current };

        // Walk backwards through the path
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }

    /// <summary>
    /// Manhattan distance heuristic for A* pathfinding
    /// </summary>
    private float CalculateHeuristic(GridSector from, GridSector to)
    {
        return Mathf.Abs(from.sectorCoordinate.x - to.sectorCoordinate.x) +
               Mathf.Abs(from.sectorCoordinate.y - to.sectorCoordinate.y);
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmos()
    {
        if (sectors == null) return;

        DrawSectorGrid();
    }

    private void DrawSectorGrid()
    {
        Gizmos.color = Color.yellow;

        for (int x = 0; x < sectors.GetLength(0); x++)
        {
            for (int y = 0; y < sectors.GetLength(1); y++)
            {
                Vector3 sectorWorldOrigin = CalculateSectorWorldPosition(x, y);
                Vector3 sectorCenter = sectorWorldOrigin + new Vector3(
                    GridSector.sectorWidth * GridManager.TILE_SIZE / 2f,
                    0f,
                    GridSector.sectorHeight * GridManager.TILE_SIZE / 2f
                );
                Vector3 sectorSize = new Vector3(
                    GridSector.sectorWidth * GridManager.TILE_SIZE,
                    0.1f,
                    GridSector.sectorHeight * GridManager.TILE_SIZE
                );

                Gizmos.DrawWireCube(sectorCenter, sectorSize);
            }
        }
    }

    private Vector3 CalculateSectorWorldPosition(int sectorX, int sectorY)
    {
        float worldX = (sectorX * GridSector.sectorWidth - GridManager.GRID_WIDTH / 2) * GridManager.TILE_SIZE;
        float worldZ = (sectorY * GridSector.sectorHeight - GridManager.GRID_HEIGHT / 2) * GridManager.TILE_SIZE;
        return new Vector3(worldX, 0f, worldZ);
    }
    
    #endregion
}