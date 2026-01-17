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

    private float[][,] borderCostFields = new float[4][,]; // NESW


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
        return GridManager.Instance.NodeFromGridCoordinate(new Vector3Int(node.globalPos.x + dir.x, 0, node.globalPos.y + dir.y)).walkCost != Mathf.Infinity && node.walkCost != Mathf.Infinity;
    }

    private List<GridNode> GetBorderNodes(CardinalDirections dir)
    {
        var border = new List<GridNode>();

        switch (dir)
        {
            case CardinalDirections.North:
                for (int x = 0; x < sectorWidth; x++)
                {
                    var node = localGrid[x, sectorHeight - 1];
                    if (CheckConnected(node, new int2(0, -1))) // check connected
                    {
                        border.Add(node);
                    }
                }
                break;

            case CardinalDirections.East:
                for (int y = 0; y < sectorHeight; y++)
                {
                    var node = localGrid[sectorWidth - 1, y];
                    if (CheckConnected(node, new int2(1, 0)))
                    {
                        border.Add(node);
                    }
                }
                break;

            case CardinalDirections.South:
                for (int x = 0; x < sectorWidth; x++)
                {
                    var node = localGrid[x, 0];
                    if (CheckConnected(node, new int2(0, 1)))
                    {
                        border.Add(node);
                    }
                }
                break;

            case CardinalDirections.West:
                for (int y = 0; y < sectorHeight; y++)
                {
                    var node = localGrid[0, y];
                    if (CheckConnected(node, new int2(-1, 0)))
                    {
                        border.Add(node);
                    }
                }
                break;
        }

        return border;
    }

    public List<GridNode> GetNeighbouringNodes(GridNode node)
    {
        List<GridNode> neighbours = new List<GridNode>();
        int2[] directions = {
            new int2(0,1),
            new int2(1,0),
            new int2(0,-1),
            new int2(-1,0)
            }; // corresponding to NESW
        for (int i = 0; i < 4; i++)
        {
            if (0 <= node.localPos.x + directions[i].x && node.localPos.x + directions[i].x < localGrid.GetLength(0) &&
                0 <= node.localPos.y + directions[i].y && node.localPos.y + directions[i].y < localGrid.GetLength(1))
            {
                neighbours.Add(localGrid[node.localPos.x + directions[i].x, node.localPos.y + directions[i].y]);
            }
        }
        return neighbours;
    }
    public Vector2 GetCentre()
    {
        return new Vector2(
            (sectorCoordinate.x * sectorWidth) + (sectorWidth / 2f),
            (sectorCoordinate.y * sectorHeight) + (sectorHeight / 2f)
        );
    }

    // take into account the next sector, and the next after
    public GridNode GuessOptimalExitNode(GridNode currentNode, GridSector nextSector, GridSector nextNextSector)
    {
        float alignmentWeight = 1f; //tune
        CardinalDirections borderDir = 0; // corresponds to CardinalDirection enum
        if (nextSector.sectorCoordinate.y > currentNode.gridSector.sectorCoordinate.y) borderDir = CardinalDirections.North; // North
        else if (nextSector.sectorCoordinate.y < currentNode.gridSector.sectorCoordinate.y) borderDir = CardinalDirections.South; // South
        else if (nextSector.sectorCoordinate.x > currentNode.gridSector.sectorCoordinate.x) borderDir = CardinalDirections.East; // East
        else if (nextSector.sectorCoordinate.x < currentNode.gridSector.sectorCoordinate.x) borderDir = CardinalDirections.West; // West

        var candidates = GetBorderNodes(borderDir);
        GridNode bestNode = null;
        float bestCost = Mathf.Infinity;

        Vector2 targetCenter = nextNextSector.GetCentre();
        Vector2 currentNodePos = new Vector2(currentNode.globalPos.x, currentNode.globalPos.y);

        Vector2 desiredDir = (targetCenter - currentNodePos).normalized;

        foreach (var node in candidates)
        {
            float baseCost = currentNode.gridSector.borderCostFields[(int)borderDir][node.localPos.x, node.localPos.y];
            Vector2 nodePos = new Vector2(node.globalPos.x, node.globalPos.y);
            Vector2 toExit = (nodePos - currentNodePos).normalized;
            float alignment = Vector2.Dot(desiredDir, toExit);
            float penalty = (1f - alignment) * alignmentWeight;

            float totalCost = baseCost + penalty;

            if (totalCost < bestCost)
            {
                bestCost = totalCost;
                bestNode = node;
            }
        }

        return bestNode;
    }
    
    // take into account the next sector, and the next after
    public GridNode GuessOptimalExitNode(GridNode currentNode, GridSector nextSector)
    {
        float alignmentWeight = 1f; //tune

        CardinalDirections borderDir = 0; // corresponds to CardinalDirection enum
        if (nextSector.sectorCoordinate.y > currentNode.gridSector.sectorCoordinate.y) borderDir = CardinalDirections.North; // North
        else if (nextSector.sectorCoordinate.y < currentNode.gridSector.sectorCoordinate.y) borderDir = CardinalDirections.South; // South
        else if (nextSector.sectorCoordinate.x > currentNode.gridSector.sectorCoordinate.x) borderDir = CardinalDirections.East; // East
        else if (nextSector.sectorCoordinate.x < currentNode.gridSector.sectorCoordinate.x) borderDir = CardinalDirections.West; // West

        var candidates = GetBorderNodes(borderDir);
        GridNode bestNode = null;
        float bestCost = Mathf.Infinity;

        Vector2 targetCenter = nextSector.GetCentre();

        Vector2 currentNodePos = new Vector2(currentNode.globalPos.x, currentNode.globalPos.y);

        Vector2 desiredDir = (targetCenter - currentNodePos).normalized;

        foreach (var node in candidates)
        {
            float baseCost = currentNode.gridSector.borderCostFields[(int)borderDir][node.localPos.x, node.localPos.y];
            Vector2 nodePos = new Vector2(node.globalPos.x, node.globalPos.y);
            Vector2 toExit = (nodePos - currentNodePos).normalized;
            float alignment = Vector2.Dot(desiredDir, toExit);
            float penalty = (1f - alignment) * alignmentWeight;

            float totalCost = baseCost + penalty;

            if (totalCost < bestCost)
            {
                bestCost = totalCost;
                bestNode = node;
            }
        }

        return bestNode;
    }

    public void AggregateCosts()
    {
        float total = 0;
        int numNodes = 0;
        for (int x = 0; x < localGrid.GetLength(0); x++)
        {
            for (int y = 0; y < localGrid.GetLength(1); y++)
            {
                total += localGrid[x, y].walkCost;
                numNodes++;
            }
        }
        this.averageCost = total / numNodes;
    }

}

