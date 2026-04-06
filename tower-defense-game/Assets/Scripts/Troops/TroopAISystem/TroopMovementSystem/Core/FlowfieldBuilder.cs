using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

public static class FlowFieldBuilder
{
    public static FlowField CreateRegional(int2 goalPosition, int radius)
    {
        GridNode[,] grid = GridManager.Instance.GetGrid();
        // calculate region coverage
        int minX = Mathf.Max(0, goalPosition.x - radius);
        int maxX = Mathf.Min(GridManager.GRID_WIDTH, goalPosition.x + radius);
        int minY = Mathf.Max(0, goalPosition.y - radius);
        int maxY = Mathf.Min(GridManager.GRID_HEIGHT, goalPosition.y + radius);
        
        int width = maxX - minX;
        int height = maxY - minY;
        
        float[,] costField = GenerateCostFieldRegional(
            grid, goalPosition, minX, minY, width, height, null
        );
        int2[,] flowField = GenerateFlowField(costField, width, height);

        return new FlowField
        {
            goalPosition = goalPosition,
            costField = costField,
            flowField = flowField,
            lastAccessed = Time.time,
            regionMin = new int2(minX, minY),
            regionMax = new int2(maxX, maxY)
        };
    }

    public static FlowField CreateRegionalWithSeed(int2 goalPosition, int radius, List<FlowField> seedFields)
    {
        GridNode[,] grid = GridManager.Instance.GetGrid();
        int minX = Mathf.Max(0, goalPosition.x - radius);
        int maxX = Mathf.Min(GridManager.GRID_WIDTH, goalPosition.x + radius);
        int minY = Mathf.Max(0, goalPosition.y - radius);
        int maxY = Mathf.Min(GridManager.GRID_HEIGHT, goalPosition.y + radius);
        
        int width = maxX - minX;
        int height = maxY - minY;
        
        float[,] costField = GenerateCostFieldRegional(
            grid, goalPosition, minX, minY, width, height, seedFields
        );
        int2[,] flowField = GenerateFlowField(costField, width, height);

        return new FlowField
        {
            goalPosition = goalPosition,
            costField = costField,
            flowField = flowField,
            lastAccessed = Time.time,
            regionMin = new int2(minX, minY),
            regionMax = new int2(maxX, maxY)
        };
    }

    private static float[,] GenerateCostFieldRegional(
        GridNode[,] grid, 
        int2 goal, 
        int minX, 
        int minY,
        int width, 
        int height,
        List<FlowField> seedFields)
    {
        float[,] costField = new float[width, height];

        // lazy initialisation of cost field with existing seed data
        // should speed up, tune as necessary
        if (seedFields != null && seedFields.Count > 0)
        {
            InitializeWithSeedData(costField, goal, minX, minY, width, height, seedFields);
        }
        else
        {
            // initialise to infinity - go from start
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    costField[x, y] = float.MaxValue;
                }
            }
        }

        PriorityQueue<int2> openSet = new PriorityQueue<int2>();
        
        // local target in region
        int2 localGoal = new int2(goal.x - minX, goal.y - minY);
        costField[localGoal.x, localGoal.y] = 0;
        openSet.Enqueue(localGoal, 0);

        int2[] neighbors = GetNeighborOffsets();

        while (openSet.Count > 0)
        {
            int2 current = openSet.Dequeue();

            foreach (int2 offset in neighbors)
            {
                int2 neighbor = current + offset;

                if (!IsInBounds(neighbor, width, height)) continue;

                // 
                int2 globalNeighbor = new int2(neighbor.x + minX, neighbor.y + minY);
                if (!IsInBounds(globalNeighbor, GridManager.GRID_WIDTH, GridManager.GRID_HEIGHT))
                    continue;

                GridNode neighborNode = grid[globalNeighbor.x, globalNeighbor.y];

                if (neighborNode.walkCost >= float.MaxValue) continue;

                float moveCost = IsDiagonal(offset) ? 1.414f : 1f;
                float newCost = costField[current.x, current.y] + moveCost * neighborNode.walkCost;

                if (newCost < costField[neighbor.x, neighbor.y])
                {
                    costField[neighbor.x, neighbor.y] = newCost;
                    openSet.Enqueue(neighbor, newCost);
                }
            }
        }

        return costField;
    }

    private static void InitializeWithSeedData(
        float[,] costField,
        int2 newGoal,
        int minX,
        int minY, 
        int width, 
        int height,
        List<FlowField> seedFields)
    {
        // to infinity first
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                costField[x, y] = float.MaxValue;
            }
        }

        // try get seed
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int2 globalPos = new int2(x + minX, y + minY);
                
                foreach (FlowField seedField in seedFields)
                {
                    // check coverage and take cost
                    if (globalPos.x >= seedField.regionMin.x && globalPos.x < seedField.regionMax.x &&
                        globalPos.y >= seedField.regionMin.y && globalPos.y < seedField.regionMax.y)
                    {
                        int2 seedLocalPos = new int2(
                            globalPos.x - seedField.regionMin.x,
                            globalPos.y - seedField.regionMin.y
                        );
                        
                        float seedCost = seedField.costField[seedLocalPos.x, seedLocalPos.y];
                        
                        if (seedCost < float.MaxValue)
                        {
                            // lazy adjustment using distance between current goal and seeded goal
                            float2 oldGoalPos = new float2(seedField.goalPosition.x, seedField.goalPosition.y);
                            float2 newGoalPos = new float2(newGoal.x, newGoal.y);
                            float2 tilePos = new float2(globalPos.x, globalPos.y);
                            
                            float oldDistance = math.distance(tilePos, oldGoalPos);
                            float newDistance = math.distance(tilePos, newGoalPos);
                            float adjustment = newDistance - oldDistance;
                            
                            float estimatedCost = seedCost + adjustment;
                            // use best cost
                            if (estimatedCost < costField[x, y])
                            {
                                costField[x, y] = estimatedCost;
                            }
                        }
                        
                        break; // break - no need to recheck tile
                    }
                }
            }
        }
    }

    private static int2[,] GenerateFlowField(float[,] costField, int width, int height)
    {
        int2[,] directions = new int2[width, height];
        int2[] neighbors = GetNeighborOffsets();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (costField[x, y] == float.MaxValue)
                {
                    directions[x, y] = new int2(0, 0);
                    continue;
                }

                int2 bestDirection = FindBestDirection(costField, new int2(x, y), neighbors, width, height);
                directions[x, y] = bestDirection;
            }
        }

        return directions;
    }

    private static int2 FindBestDirection(float[,] costField, int2 position, int2[] neighbors, int width, int height)
    {
        float lowestCost = float.MaxValue;
        int2 bestDirection = new int2(0, 0);

        foreach (int2 offset in neighbors)
        {
            int2 neighbor = position + offset;

            if (!IsInBounds(neighbor, width, height)) continue;
            if (costField[neighbor.x, neighbor.y] >= float.MaxValue) continue;

            if (costField[neighbor.x, neighbor.y] < lowestCost)
            {
                lowestCost = costField[neighbor.x, neighbor.y];
                bestDirection = offset;
            }
        }

        return bestDirection;
    }

    private static int2[] GetNeighborOffsets()
    {
        return new int2[]
        {
            new int2(0, 1), new int2(1, 0), new int2(0, -1), new int2(-1, 0),
            new int2(1, 1), new int2(1, -1), new int2(-1, 1), new int2(-1, -1)
        };
    }

    private static bool IsDiagonal(int2 offset)
    {
        return offset.x != 0 && offset.y != 0;
    }

    private static bool IsInBounds(int2 pos, int width, int height)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }
}