using UnityEngine;
using System.Collections.Generic;

public class GridSector // for HPA*
{
    public GridManager.GridNode[,] localGrid;
    public float[,] costField;
    public Vector2[,] vectorField;

    public Dictionary<int, List<GridNode>> entryPoints;
    public Vector2 centre;
    private int sectorID;

    private Dictionary<GridNode, float> costField;

    private Dictionary<GridNode, Vector2> flowVectors;

    private void CalculateCenter()
    {
        Vector2 sum = Vector2.zero;
        foreach (GridManager.GridNode tile in tiles)
            sum += tile.coord;
        this.centre = sum / tiles.Count;
    }

    private float aggregateCosts()
    {
        float sum = 0;
        foreach (var item in myDictionary.Keys)
        {
            sum += item;
        }
        return sum;
    }
}
