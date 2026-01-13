using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

[System.Serializable]
public class GridNode
{
    public float walkCost = 1f;
    public int territoryStatus; // refer to TerritoryStatus enum
    public bool buildable;
    public int2 globalPos; // coordinates in global grid
    public int2 localPos; // coordinates in local sector grid.
    public GridSector gridSector; // which grid sector it belongs to
    public GridNode(int2 globalPos, float walkCost = 1f) // might have to assign walk cost later on creation
    {
        walkCost = 1f;
        territoryStatus = (int)GridManager.TerritoryStatus.NotAssigned;
        buildable = true;
        this.globalPos = globalPos;
    }

}
