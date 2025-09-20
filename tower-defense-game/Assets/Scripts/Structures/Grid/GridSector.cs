using UnityEngine;
using System.Collections.Generic;

public class GridSector // for HPA*
{
    public List<GridManager.GridNode> tiles = new List<GridManager.GridNode>();
    public Vector2 centre;

    private void CalculateCenter()
    {
        Vector2 sum = Vector2.zero;
        foreach (GridManager.GridNode tile in tiles)
            sum += tile.coord;
        this.centre = sum / tiles.Count;
    }
}
