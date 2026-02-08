using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;


public class GridSector // for HPA*
{

    public readonly static int sectorWidth = 10;
    public readonly static int sectorHeight = 10;

    public int2 sectorCoordinate;

    
    public enum CardinalDirections
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3
    }

    public GridNode[,] localGrid = new GridNode[sectorWidth, sectorHeight];



    // FOR HPA*
    public GridSector[] neighbours = new GridSector[4]; // corresponds to cardinal directions
    public float averageCost;
    private bool hasSectorChanged = false; // flag to check if we need to recalc fields
    public bool HasSectorChanged
    {
        set
        {
            hasSectorChanged = value;
        }
    }


    public GridSector(int2 sectorCoordinate)
    {
        this.sectorCoordinate = sectorCoordinate;
    }

    
    private static bool CheckConnected(GridNode node, int2 dir)
    {
        if (node.globalPos.x + dir.x >= GridManager.GRID_WIDTH || node.globalPos.x + dir.x < 0)
        {
            return false;
        }
        if (node.globalPos.y + dir.y >= GridManager.GRID_HEIGHT || node.globalPos.y + dir.y < 0)
        {
            return false;
        }
        GridNode neighbor = GridManager.Instance.NodeFromGridCoordinate(new Vector3Int(node.globalPos.x + dir.x, 0, node.globalPos.y + dir.y));
        return neighbor.walkCost < GridNode.UNWALKABLE && node.walkCost < GridNode.UNWALKABLE;
    }

    public void AggregateCosts()
    {
        float total = 0;
        int walkableNodes = 0;
        int totalNodes = 0;
        for (int x = 0; x < localGrid.GetLength(0); x++)
        {
            for (int y = 0; y < localGrid.GetLength(1); y++)
            {
                totalNodes++;
                float cost = localGrid[x, y].walkCost;
                if (cost < GridNode.UNWALKABLE)
                {
                    total += cost;
                    walkableNodes++;
                }
            }
        }
        
        if (walkableNodes == 0)
        {
            // entirely blocked
            this.averageCost = 999999f;
        }
        else
        {
            float baseCost = total / walkableNodes;
            // Penalize partially blocked sectors so A* prefers open ones
            float blockedRatio = 1f - ((float)walkableNodes / totalNodes);
            this.averageCost = baseCost * (1f + blockedRatio * 5f);
        }
    }

}

