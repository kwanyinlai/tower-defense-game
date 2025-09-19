using UnityEngine;
using System.Collections;
using GridManager.GridNode;

public class GridSector // for HPA*
{
    public List<GridNode> tiles = new List<GridNode>();
    public Vector2 centre;

    private Vector2 CalculateCenter()
    {
        if (tiles.Count == 0){
            return;
        }
        Vector2 sum = Vector2.zero;
        foreach (var pos in tiles)
            sum += pos;
        this.centre = sum / tiles.Count;
    }
}
