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
        // BFS
        HashSet<GridNode> visited = new HashSet<GridNode>();
        Queue<GridNode> uncheckedNodes = new Queue<GridNode>();
        foreach (var node in exitNodes)
        {
            uncheckedNodes.Enqueue(node);
        }

        while (uncheckedNodes.Count > 0)
        {
            var current = uncheckedNodes.Dequeue();
            for
        }
    }

    // private float AggregateCosts()
    // {
    //     // cost = entryPoints.Keys.Sum(); 
    //     // TODO: This doesn't work. Can't sum on KeyCollection like Java
    //     return cost;
    // }
    // // TODO:

    public List<GridNode> GetNeighbouringNodes()
    {
        int2[] directions = {
            new int2(0,1),
            new int2(1,0),
            new int2(0,-1),
            new int2(-1,0)
            }; // corresponding to NESW
        for (int i = 0; i < 4; i++)
        {
            pass;
        }
    }
}
