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

    private float[][,] borderCostFields;

    // Linked hash
    private FlowFieldCache cachedFields;

    private int cacheSize = 10;


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
        cachedFields = new FlowFieldCache(cacheSize);
    }


    // public void GenerateVectorField(List<GridNode> exitNodes)
    // {
    //     // Perform Dijkstra's from goal 
    //     for (int x = 0; x < localGrid.GetLength(0); x++)
    //     {
    //         for (int y = 0; y < localGrid.GetLength(1); y++)
    //         {
    //             costField[x, y] = float.MaxValue;
    //         }
    //     }

    //     PriorityQueue<GridNode, float> uncheckedNodes = new PriorityQueue<GridNode, float>();
    //     // enqueue all border nodes first
    //     foreach (var node in exitNodes)
    //     {
    //         costField[node.localX, node.localY]= 0f;
    //         uncheckedNodes.Enqueue(node, 0f);
    //     }
    //     // first generate cost field, each node points to closest exit
    //     while (uncheckedNodes.Count > 0)
    //     {
    //         var current = uncheckedNodes.Dequeue();
    //         foreach (var neighbour in GetNeighbouringNodes(current))
    //         {
    //             float tentativeCost = costField[current.localX, current.localY] + neighbour.walkCost;
    //             if (tentativeCost < costField[neighbour.localX, neighbour.localY])
    //             {
    //                 costField[neighbour.localX, neighbour.localY] = tentativeCost;
    //                 uncheckedNodes.Enqueue(neighbour, tentativeCost);
    //             }
    //         }
    //     }
    // }

    public void GenerateCostFieldForBorders()
    {
        List<GridNode> borderNodes = GetBorderNodes(CardinalDirections.North);
        borderCostFields[(int)CardinalDirections.North] = GenerateCostField(borderNodes);

        borderNodes = GetBorderNodes(CardinalDirections.East);
        borderCostFields[(int)CardinalDirections.East] = GenerateCostField(borderNodes);

        borderNodes = GetBorderNodes(CardinalDirections.South);
        borderCostFields[(int)CardinalDirections.South] = GenerateCostField(borderNodes);

        borderNodes = GetBorderNodes(CardinalDirections.West);
        borderCostFields[(int)CardinalDirections.West] = GenerateCostField(borderNodes);


    }
    
    private static bool CheckConnected(GridNode node, int2 dir)
    {
        if (node.globalX + dir.x >= GridManager.Instance.GetGrid().GetLength(0) || node.globalX + dir.y < 0)
        {
            return false;
        }
        if (node.globalY + dir.y >= GridManager.Instance.GetGrid().GetLength(1) || node.globalX + dir.y < 0)
        {
            return false;
        }

        return GridManager.Instance.GetGrid()[node.globalX + dir.x, node.globalY + dir.y].walkCost != Mathf.Infinity && node.walkCost != Mathf.Infinity;
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
                    if (GridSector.CheckConnected(node, new int2(0, -1))) // check connected
                    {
                        border.Add(node);
                    }
                }
                break;

            case CardinalDirections.East:
                for (int y = 0; y < sectorHeight; y++)
                {
                    var node = localGrid[sectorWidth - 1, y];
                    if (GridSector.CheckConnected(node, new int2(1, 0)))
                    {
                        border.Add(node);
                    }
                }
                break;

            case CardinalDirections.South:
                for (int x = 0; x < sectorWidth; x++)
                {
                    var node = localGrid[x, 0];
                    if (GridSector.CheckConnected(node, new int2(0, 1)))
                    {
                        border.Add(node);
                    }
                }
                break;

            case CardinalDirections.West:
                for (int y = 0; y < sectorHeight; y++)
                {
                    var node = localGrid[0, y];
                    if (GridSector.CheckConnected(node, new int2(-1, 0)))
                    {
                        border.Add(node);
                    }
                }
                break;
        }

        return border;
    }

    private float[,] GenerateCostField(List<GridNode> sourceNodes)
    {
        var field = new float[sectorWidth, sectorHeight];

        // Initialize field with infinity
        for (int x = 0; x < sectorWidth; x++)
        {
            for (int y = 0; y < sectorHeight; y++)
            {
                field[x, y] = Mathf.Infinity;
            }
        }

        var pq = new PriorityQueue<GridNode, float>();
        Dictionary<GridNode, float> nodeCosts = new Dictionary<GridNode, float>();

        // Add source nodes
        foreach (var src in sourceNodes)
        {
            field[src.localX, src.localY] = 0f;
            pq.Enqueue(src, 0f);
            nodeCosts[src] = 0f;
        }

        // Dijkstra-like loop
        while (pq.Count > 0)
        {
            var current = pq.Dequeue();
            float currentCost = nodeCosts[current];

            // Skip if we already found a better path
            if (currentCost > field[current.localX, current.localY])
                continue;

            foreach (var neighbour in GetNeighbouringNodes(current))
            {
                if (neighbour == null) continue;

                float tentativeCost = currentCost + neighbour.walkCost;

                if (tentativeCost < field[neighbour.localX, neighbour.localY])
                {
                    field[neighbour.localX, neighbour.localY] = tentativeCost;
                    pq.Enqueue(neighbour, tentativeCost);
                    nodeCosts[neighbour] = tentativeCost;
                }
            }
        }

        return field;
    }


    public Vector2[,] GenerateVectorField(GridNode goalNode) // single goal dijk
    {
        float[,] costField = new float[sectorWidth, sectorHeight];
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

        costField[goalNode.localX, goalNode.localY] = 0f;
        uncheckedNodes.Enqueue(goalNode, 0);

        // first generate cost field, each node points to closest exit
        while (uncheckedNodes.Count > 0)
        {
            var current = uncheckedNodes.Dequeue();
            foreach (var neighbour in GetNeighbouringNodes(current))
            {
                if (neighbour == null)
                {
                    continue;
                }
                float tentativeCost = costField[current.localX, current.localY] + neighbour.walkCost;
                if (tentativeCost < costField[neighbour.localX, neighbour.localY])
                {
                    costField[neighbour.localX, neighbour.localY] = tentativeCost;
                    uncheckedNodes.Enqueue(neighbour, tentativeCost);
                }
            }
        }
        return GenerateVectorFieldFromCost(costField);
    }

    private Vector2[,] GenerateVectorFieldFromCost(float[,] costField) // multigoal dijk
    {
        Vector2[,] newField = new Vector2[sectorWidth, sectorHeight];
        for (int x = 0; x < localGrid.GetLength(0); x++)
        {
            for (int y = 0; y < localGrid.GetLength(1); y++)
            {
                float x1 = x > 0 ? costField[x - 1, y] : costField[x, y];
                float x2 = x < costField.GetLength(0) - 1 ? costField[x + 1, y] : costField[x, y];
                float y1 = y > 0 ? costField[x, y - 1] : costField[x, y];
                float y2 = y < costField.GetLength(1) - 1 ? costField[x, y + 1] : costField[x, y];
                float dx = x2 - x1;
                float dy = y2 - y1;
                newField[x, y] = -1 * new Vector2(dx, dy).normalized;
            }
        }
        return newField;
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
    // TODO:

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
            if (0 <= node.localX + directions[i].x && node.localX + directions[i].x < localGrid.GetLength(0) &&
                0 <= node.localY + directions[i].y && node.localY + directions[i].y < localGrid.GetLength(1))
            {
                neighbours.Add(localGrid[node.localX + directions[i].x, node.localY + directions[i].y]);
            }
        }
        return neighbours;
    }

    // public Vector2 GetSmoothedVectorDirection(Vector2 coordinates)
    // {
    //     Vector2 dxp = coordinates.x + 1 <= sectorWidth ? vectorField[coordinates.x + 1, coordinates.y] : vectorField[coordinates.x, coordinates.y];
    //     Vector2 dxn = coordinates.x - 1 > 0 ? vectorField[coordinates.x - 1, coordinates.y] : vectorField[coordinates.x, coordinates.y];
    //     return null;
    // }

    public Vector2 QueryFlowField(GridNode currentNode, GridNode targetNode)
    {
        if (cachedFields.TryGetFlowField(targetNode, out Vector2[,] cachedField))
        {
            return cachedField[currentNode.localX, currentNode.localY];
        }

        Vector2[,] newField = GenerateVectorField(targetNode);
        cachedFields.AddFlowFieldToCache(targetNode, newField);
        return newField[currentNode.localX, currentNode.localY];
    }

    public Vector2 GetCentre()
    {
        return new Vector2((float)((sectorCoordinate.x + 0.5)*sectorWidth), (float)((sectorCoordinate.y +0.5)*sectorHeight));
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
        Vector2 currentNodePos = new Vector2(currentNode.globalX, currentNode.globalY);

        Vector2 desiredDir = (targetCenter - currentNodePos).normalized;

        foreach (var node in candidates)
        {
            float baseCost = currentNode.gridSector.borderCostFields[(int)borderDir][node.localX, node.localY];
            Vector2 nodePos = new Vector2(node.globalX, node.globalY);
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

        Vector2 currentNodePos = new Vector2(currentNode.globalX, currentNode.globalY);

        Vector2 desiredDir = (targetCenter - currentNodePos).normalized;

        foreach (var node in candidates)
        {
            float baseCost = currentNode.gridSector.borderCostFields[(int)borderDir][node.localX, node.localY];
            Vector2 nodePos = new Vector2(node.globalX, node.globalY);
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

}


public class FlowFieldCache
{
    private int cacheSize;
    public FlowFieldCache(int cacheSize)
    {
        this.cacheSize = cacheSize;
    }
    private class CacheItem
    {
        public GridNode target;
        public Vector2[,] flowField;

        public CacheItem prev;
        public CacheItem next;

        public CacheItem(GridNode target, Vector2[,] flowField)
        {
            this.target = target;
            this.flowField = flowField;
        }
    }
    private CacheItem head;
    private CacheItem tail;
    private Dictionary<GridNode, CacheItem> cachedFields;

    private int capacity = 5; // TODO: update this maybe

    public bool TryGetFlowField(GridNode key, out Vector2[,] flowField)
    {
        if (cachedFields.TryGetValue(key, out CacheItem target))
        {
            UpdateMostRecentlyUsed(target);
            flowField = target.flowField;
            return true;
        }
        flowField = null;
        return false;
    }

    public void AddFlowFieldToCache(GridNode targetNode, Vector2[,] flowField)
    {
        if (!cachedFields.TryGetValue(targetNode, out CacheItem item))
        {
            item.flowField = flowField;
            UpdateMostRecentlyUsed(item);
            return;
        }
        item = new CacheItem(targetNode, flowField);
        AddNodeToHead(item);
        cachedFields[targetNode] = item;

        if (cachedFields.Count > cacheSize)
        {
            RemoveTail();
        }
    }

    private void UpdateMostRecentlyUsed(CacheItem node)
    {
        if (node == head)
        {
            return;
        }
        // remove node
        if (node.prev != null)
        {
            node.prev.next = node.next;
        }
        if (node.next != null)
        {
            node.next.prev = node.prev;
        }
        // add it back
        AddNodeToHead(node);
    }

    private void AddNodeToHead(CacheItem node)
    {
        node.prev = null;
        node.next = head;
        if (head != null)
        {
            head.prev = node;
        }
        head = node;
        if (tail == null)
        {
            tail = head;
        }
    }

    private void RemoveTail()
    {
        if (tail == null)
        {
            return;
        }
        cachedFields.Remove(tail.target);
        if (tail.prev != null)
        {
            tail.prev.next = null;
            tail = tail.prev;
        }
        else
        {
            head = null;
            tail = null;
        }
    }
}