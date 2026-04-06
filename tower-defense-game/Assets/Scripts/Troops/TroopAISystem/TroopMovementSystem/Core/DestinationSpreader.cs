using UnityEngine;

/// <summary>
/// Computes spread-out arrival positions for a group of troops targeting the same location.
/// Uses a Fibonacci disc (sunflower) pattern to distribute troops evenly around a center point.
/// Validates positions against the grid to avoid placing troops in unwalkable cells.
/// </summary>
public static class DestinationSpreader
{
    /// <summary>Spacing between troops in the spread pattern (world units).</summary>
    private const float TROOP_SPACING = 2.5f;

    /// <summary>Golden angle in radians (≈137.508°) — produces the most uniform disc distribution.</summary>
    private const float GOLDEN_ANGLE = 2.39996323f;

    /// <summary>
    /// Compute spread positions for a group of troops around a center destination.
    /// Troop 0 gets the exact center; subsequent troops spiral outward.
    /// </summary>
    /// <param name="center">The shared destination all troops were commanded to.</param>
    /// <param name="troopCount">Total number of troops in the group.</param>
    /// <returns>Array of spread positions, one per troop.</returns>
    public static Vector3[] ComputeSpreadPositions(Vector3 center, int troopCount)
    {
        if (troopCount <= 0) return System.Array.Empty<Vector3>();

        var positions = new Vector3[troopCount];
        positions[0] = center; // first troop goes to exact target

        GridManager gridManager = GridManager.Instance;

        for (int i = 1; i < troopCount; i++)
        {
            // Fibonacci disc: radius grows with sqrt(index), angle increments by golden angle
            float r = TROOP_SPACING * Mathf.Sqrt(i);
            float theta = i * GOLDEN_ANGLE;

            Vector3 offset = new Vector3(r * Mathf.Cos(theta), 0f, r * Mathf.Sin(theta));
            Vector3 candidate = center + offset;

            // Validate against grid — fall back to center if unwalkable
            if (gridManager != null)
            {
                var node = gridManager.NodeFromWorldPos(candidate);
                if (node == null || node.walkCost >= GridNode.UNWALKABLE)
                {
                    // Try a slightly shorter radius
                    candidate = center + offset * 0.5f;
                    node = gridManager.NodeFromWorldPos(candidate);
                    if (node == null || node.walkCost >= GridNode.UNWALKABLE)
                    {
                        candidate = center; // last resort: just go to center
                    }
                }
            }

            positions[i] = candidate;
        }

        return positions;
    }
}
