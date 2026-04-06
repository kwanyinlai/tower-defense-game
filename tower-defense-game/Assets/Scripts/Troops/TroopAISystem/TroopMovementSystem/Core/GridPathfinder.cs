using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

/// <summary>
/// Simple A* pathfinder on the 200x200 grid.
/// Returns a list of world-space waypoints.
/// </summary>
public static class GridPathfinder
{
    private static readonly int2[] NEIGHBORS = new int2[]
    {
        new int2(0, 1), new int2(1, 0), new int2(0, -1), new int2(-1, 0),
        new int2(1, 1), new int2(1, -1), new int2(-1, 1), new int2(-1, -1)
    };

    /// <summary>
    /// Runs A* on the node grid and returns a smoothed list of world-space waypoints.
    /// Returns null if no path exists.
    /// </summary>
    public static List<Vector3> FindPath(Vector3 startWorld, Vector3 endWorld)
    {
        GridNode[,] grid = GridManager.Instance.GetGrid();

        Vector3Int startCoord = GridManager.WorldPosFromCoordinates(startWorld);
        Vector3Int endCoord = GridManager.WorldPosFromCoordinates(endWorld);

        int2 start = new int2(startCoord.x, startCoord.z);
        int2 goal = new int2(endCoord.x, endCoord.z);

        if (!InBounds(start) || !InBounds(goal)) return null;
        if (grid[goal.x, goal.y].walkCost >= GridNode.UNWALKABLE) return null;

        // A* on flat grid
        var openSet = new PriorityQueue<int2>();
        var cameFrom = new Dictionary<long, long>();
        var gScore = new Dictionary<long, float>();

        long startKey = PackKey(start);
        long goalKey = PackKey(goal);

        gScore[startKey] = 0f;
        openSet.Enqueue(start, Heuristic(start, goal));

        bool found = false;

        while (openSet.Count > 0)
        {
            int2 current = openSet.Dequeue();
            long currentKey = PackKey(current);

            if (currentKey == goalKey)
            {
                found = true;
                break;
            }

            float currentG = gScore[currentKey];

            for (int i = 0; i < NEIGHBORS.Length; i++)
            {
                int2 neighbor = current + NEIGHBORS[i];
                if (!InBounds(neighbor)) continue;

                GridNode node = grid[neighbor.x, neighbor.y];
                if (node.walkCost >= GridNode.UNWALKABLE) continue;

                float moveCost = (NEIGHBORS[i].x != 0 && NEIGHBORS[i].y != 0) ? 1.414f : 1f;

                // block diagonal movement through walls
                if (NEIGHBORS[i].x != 0 && NEIGHBORS[i].y != 0)
                {
                    GridNode adj1 = grid[current.x + NEIGHBORS[i].x, current.y];
                    GridNode adj2 = grid[current.x, current.y + NEIGHBORS[i].y];
                    if (adj1.walkCost >= GridNode.UNWALKABLE || adj2.walkCost >= GridNode.UNWALKABLE)
                        continue;
                }

                float tentativeG = currentG + moveCost * node.walkCost;
                long neighborKey = PackKey(neighbor);

                if (!gScore.ContainsKey(neighborKey) || tentativeG < gScore[neighborKey])
                {
                    gScore[neighborKey] = tentativeG;
                    cameFrom[neighborKey] = currentKey;
                    float f = tentativeG + Heuristic(neighbor, goal);
                    openSet.Enqueue(neighbor, f);
                }
            }
        }

        if (!found) return null;

        // Reconstruct path as grid coordinates
        List<int2> gridPath = new List<int2>();
        long traceKey = goalKey;
        while (traceKey != startKey)
        {
            gridPath.Add(UnpackKey(traceKey));
            traceKey = cameFrom[traceKey];
        }
        gridPath.Add(start);
        gridPath.Reverse();

        // Simplify: only keep waypoints where direction changes
        List<Vector3> waypoints = SimplifyPath(gridPath);

        return waypoints;
    }

    /// <summary>
    /// Reduces the path to only keep turning points (direction changes).
    /// </summary>
    private static List<Vector3> SimplifyPath(List<int2> gridPath)
    {
        if (gridPath.Count <= 2)
        {
            List<Vector3> result = new List<Vector3>(gridPath.Count);
            foreach (var p in gridPath)
                result.Add(GridManager.CoordinatesToWorldPos(p));
            return result;
        }

        List<Vector3> waypoints = new List<Vector3>();
        waypoints.Add(GridManager.CoordinatesToWorldPos(gridPath[0]));

        int2 prevDir = gridPath[1] - gridPath[0];

        for (int i = 2; i < gridPath.Count; i++)
        {
            int2 dir = gridPath[i] - gridPath[i - 1];
            if (dir.x != prevDir.x || dir.y != prevDir.y)
            {
                // direction changed — add the previous point as a waypoint
                waypoints.Add(GridManager.CoordinatesToWorldPos(gridPath[i - 1]));
                prevDir = dir;
            }
        }

        // always add the final destination
        waypoints.Add(GridManager.CoordinatesToWorldPos(gridPath[gridPath.Count - 1]));

        return waypoints;
    }

    private static float Heuristic(int2 a, int2 b)
    {
        // octile distance
        int dx = math.abs(a.x - b.x);
        int dy = math.abs(a.y - b.y);
        return math.max(dx, dy) + 0.414f * math.min(dx, dy);
    }

    private static bool InBounds(int2 pos)
    {
        return pos.x >= 0 && pos.x < GridManager.GRID_WIDTH &&
               pos.y >= 0 && pos.y < GridManager.GRID_HEIGHT;
    }

    // pack two ints into a long for dictionary keys (avoids int2 hashing overhead)
    private static long PackKey(int2 pos)
    {
        return ((long)pos.x << 32) | (uint)pos.y;
    }

    private static int2 UnpackKey(long key)
    {
        return new int2((int)(key >> 32), (int)(key & 0xFFFFFFFF));
    }
}
