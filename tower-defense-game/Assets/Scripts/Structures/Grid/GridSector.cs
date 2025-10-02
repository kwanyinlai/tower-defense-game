using UnityEngine;
using System.Collections.Generic;


public class MapSector
{
    public MapSector tr;
    public MapSector tl;
    public MapSector bl;
    public MapSector br;
    
    // tl, bl, br, tr == null for data to be stored??
}
public class GridSector // for HPA*
{

    public GridManager.GridNode[,] localGrid;
    public Vector2[,] vectorField;

    public Dictionary<int, List<GridManager.GridNode>> entryPoints;
    public Vector2 centre;
    private int sectorID;
    private float cost;

    private Dictionary<GridManager.GridNode, float> costField;

    private Dictionary<GridManager.GridNode, Vector2> flowVectors;

    private void CalculateCenter()
    {
        Vector2 sum = Vector2.zero;
        foreach (GridManager.GridNode tile in tiles)
            sum += tile.coord;
        this.centre = sum / tiles.Count;
    }

    private float AggregateCosts()
    {
        cost = myDictionary.Keys.Sum();
        return cost;
    }
}
