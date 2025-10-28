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
    public Vector2[,] vectorField = new Vector2[sectorWidth, sectorHeight];

    private float[,] costField = new float[sectorWidth, sectorHeight];


    // FOR HPA*
    public bool[] neighbours = new bool[4]; // corresponds to cardinal directions
    public float averageCost;
    private bool hasSectorChanged = false; // flag to check if we need to recalc fields


    public GridSector(int2 sectorCoordinate)
    {
        this.sectorCoordinate = sectorCoordinate;
        GenerateLocalCostField();
    }
    public void GenerateLocalCostField()
    {
        for (int x = 0; x < sectorWidth; x++)
        {
            for (int y = 0; y < sectorHeight; y++)
            {
                costField[x, y] = localGrid[x, y].walkCost;
            }
        }
    }

    public void GenerateVectorField(List<GridNode> exitNodes)
    {
        // Perform Dijkstra's from goal 
        for (int x = 0; x < localGrid.GetLength(0); x++)
        {
            for (int y = 0; y < localGrid.GetLength(1); y++)
            {
                costField[x, y] = float.MaxValue;
            }
        }

        PriorityQueue<GridNode, float> uncheckedNodes = new PriorityQueue<GridNode, float>();
        // enqueue all border nodes first
        foreach (var node in exitNodes)
        {
            costField[node.localX, node.localY]= 0f;
            uncheckedNodes.Enqueue(node, 0f);
        }
        // first generate cost field, each node points to closest exit
        while (uncheckedNodes.Count > 0)
        {
            var current = uncheckedNodes.Dequeue();
            foreach (var neighbour in GetNeighbouringNodes(current))
            {
                float tentativeCost = costField[current.localX, current.localY] + neighbour.walkCost;
                if (tentativeCost < costField[neighbour.localX, neighbour.localY])
                {
                    costField[neighbour.localX, neighbour.localY] = tentativeCost;
                    uncheckedNodes.Enqueue(neighbour, tentativeCost);
                }
            }
        }
    }

    public void GenerateVectorField(GridNode goalNode)
    {
        // Perform Dijkstra's from goal 
        for (int x = 0; x < localGrid.GetLength(0); x++)
        {
            for (int y = 0; y < localGrid.GetLength(1); y++)
            {
                costField[x, y] = float.MaxValue;
            }
        }

        PriorityQueue<GridNode, float> uncheckedNodes = new PriorityQueue<GridNode, float>();
        // enqueue all border nodes first

        costField[goalNode.localX, goalNode.localY]= 0f;
        uncheckedNodes.Enqueue(goalNode, 0);
        
        // first generate cost field, each node points to closest exit
        while (uncheckedNodes.Count > 0)
        {
            var current = uncheckedNodes.Dequeue();
            foreach (var neighbour in GetNeighbouringNodes(current))
            {
                float tentativeCost = costField[current.localX, current.localY] + neighbour.walkCost;
                if (tentativeCost < costField[neighbour.localX, neighbour.localY])
                {
                    costField[neighbour.localX, neighbour.localY] = tentativeCost;
                    uncheckedNodes.Enqueue(neighbour, tentativeCost);
                }
            }
        }
    }

    private void GenerateVectorField()
    {
        for (int x =0; x < localGrid.GetLength(0); x++)
        {
            for(int y=0; y<localGrid.GetLength(1); y++)
            {
                float x1 = x > 0 ? costField[x - 1, y] : costField[x, y];
                float x2 = x < costField.GetLength(0) - 1 ? costField[x + 1, y] : costField[x, y];
                float y1 = y > 0 ? costField[x, y - 1] : costField[x, y];
                float y2 = y < costField.GetLength(1) - 1 ? costField[x, y + 1] : costField[x, y];
                float dx = x2 - x1;
                float dy = y2 - y1;
                vectorField[x, y] = -1 * new Vector2(dx, dy).normalized;
            }
        }
    }

    // private float AggregateCosts()
    // {
    //     // cost = entryPoints.Keys.Sum(); 
    //     // TODO: This doesn't work. Can't sum on KeyCollection like Java
    //     return cost;
    // }
    // // TODO:

    public GridNode[] GetNeighbouringNodes(GridNode node)
    {
        GridNode[] neighbours = new GridNode[4]; 
        int2[] directions = {
            new int2(0,1),
            new int2(1,0),
            new int2(0,-1),
            new int2(-1,0)
            }; // corresponding to NESW
        for (int i = 0; i < 4; i++)
        {
            if (0 <= node.localX + directions[i].x && node.localX + directions[i].x < localGrid.GetLength(0) &&
                0 <= node.localY + directions[i].y && node.localY + directions[i].y< localGrid.GetLength(1))
            {
                neighbours[i] = localGrid[node.localX + directions[i].x, node.localY + directions[i].y];
            }
        }
        return neighbours;
    }
}
