using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

// TODO: do i need to cache waypoints as well???
public static class WaypointPathBuilder
{
    public static List<Vector3> GenerateWaypoints(List<GridSector> sectorPath, Vector3Int start, Vector3Int end)
    {
        // generate using sectors
        List<Vector3> waypoints = new List<Vector3>();

        waypoints.Add(GridManager.CoordinatesToWorldPos(start));

        // simple set waypoints on each sector border
        for (int i = 0; i < sectorPath.Count - 1; i++)
        {
            Vector3 transitionPoint = FindBestTransitionPoint(sectorPath[i], sectorPath[i + 1]);
            waypoints.Add(transitionPoint);
        }

        waypoints.Add(GridManager.CoordinatesToWorldPos(end));

        // smooth the path down to be more natural
        waypoints = SmoothPath(waypoints);

        return waypoints;
    }

    private static Vector3 FindBestTransitionPoint(GridSector from, GridSector to)
    {
        // naive using border centre - perhaps improve this
        int direction = GetDirectionBetweenSectors(from, to);

        switch (direction)
        {
            case (int)GridSector.CardinalDirections.North:
                return GetBorderCenterPoint(from, GridSector.sectorWidth / 2, GridSector.sectorHeight - 1);

            case (int)GridSector.CardinalDirections.East:
                return GetBorderCenterPoint(from, GridSector.sectorWidth - 1, GridSector.sectorHeight / 2);

            case (int)GridSector.CardinalDirections.South:
                return GetBorderCenterPoint(from, GridSector.sectorWidth / 2, 0);

            case (int)GridSector.CardinalDirections.West:
                return GetBorderCenterPoint(from, 0, GridSector.sectorHeight / 2);

            default:
                return GetSectorCenter(from);
        }
    }

    private static Vector3 GetBorderCenterPoint(GridSector sector, int localX, int localY)
    {
        GridNode node = sector.localGrid[localX, localY];
        return GridManager.CoordinatesToWorldPos(new int2(node.globalPos.x, node.globalPos.y));
    }

    private static Vector3 GetSectorCenter(GridSector sector)
    {
        int centerX = sector.sectorCoordinate.x * GridSector.sectorWidth + GridSector.sectorWidth / 2;
        int centerY = sector.sectorCoordinate.y * GridSector.sectorHeight + GridSector.sectorHeight / 2;
        return GridManager.CoordinatesToWorldPos(new int2(centerX, centerY));
    }

    private static int GetDirectionBetweenSectors(GridSector from, GridSector to)
    {
        int2 diff = to.sectorCoordinate - from.sectorCoordinate;

        if (diff.y > 0) return (int)GridSector.CardinalDirections.North;
        if (diff.x > 0) return (int)GridSector.CardinalDirections.East;
        if (diff.y < 0) return (int)GridSector.CardinalDirections.South;
        if (diff.x < 0) return (int)GridSector.CardinalDirections.West;

        return -1;
    }

    #region Path Smoothing

    private static List<Vector3> SmoothPath(List<Vector3> waypoints)
    {
        // no need to smooth with few waypoints
        if (waypoints.Count <= 2) return waypoints;

        List<Vector3> smoothed = new List<Vector3> { waypoints[0] };
        int currentIndex = 0;

        while (currentIndex < waypoints.Count - 1)
        {
            int furthestVisible = FindFurthestVisibleWaypoint(waypoints, currentIndex);
            smoothed.Add(waypoints[furthestVisible]);
            currentIndex = furthestVisible;
        }

        return smoothed;
    }

    private static int FindFurthestVisibleWaypoint(List<Vector3> waypoints, int startIndex)
    {
        int furthestVisible = startIndex + 1;

        for (int i = startIndex + 2; i < waypoints.Count; i++)
        {
            if (HasLineOfSight(waypoints[startIndex], waypoints[i]))
            {
                furthestVisible = i;
            }
            else
            {
                break;
            }
        }

        return furthestVisible;
    }

    private static bool HasLineOfSight(Vector3 from, Vector3 to)
    {
        Vector3Int vectoredFrom = GridManager.WorldPosFromCoordinates(from);
        Vector3Int vectoredTo = GridManager.WorldPosFromCoordinates(to);
        int2 fromGrid = new int2(vectoredFrom.x, vectoredFrom.z);
        int2 toGrid = new int2(vectoredTo.x, vectoredTo.z);
        // get all grid nodes between the waypoint
        List<int2> linePoints = BresenhamLine(fromGrid, toGrid);

        foreach (int2 point in linePoints)
        {
            GridNode node = GridManager.Instance.NodeFromGridCoordinate(point);
            if (node == null || node.walkCost >= GridNode.UNWALKABLE)
            {
                return false;
            }
        }

        return true;
    }

    private static List<int2> BresenhamLine(int2 from, int2 to)
    {
        // standard bresenham line algo
        List<int2> points = new List<int2>();

        int dx = Mathf.Abs(to.x - from.x);
        int dy = Mathf.Abs(to.y - from.y);
        int sx = from.x < to.x ? 1 : -1;
        int sy = from.y < to.y ? 1 : -1;
        int err = dx - dy;

        int x = from.x;
        int y = from.y;

        while (true)
        {
            points.Add(new int2(x, y));

            if (x == to.x && y == to.y) break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }

        return points;
    }

    #endregion
}