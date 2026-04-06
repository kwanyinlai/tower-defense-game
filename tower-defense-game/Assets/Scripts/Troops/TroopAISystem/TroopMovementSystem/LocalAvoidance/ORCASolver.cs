using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

/// <summary>
/// Burst-compatible ORCA (Optimal Reciprocal Collision Avoidance) solver.
/// All float2 — usable from both MonoBehaviour and ECS/Jobs.
///
/// Reference: van den Berg et al., "Reciprocal n-body Collision Avoidance" (2011)
/// </summary>
[BurstCompile]
public static class ORCASolver
{
    /// <summary>
    /// Compact agent data for neighbor queries. 24 bytes — cache-friendly.
    /// </summary>
    public struct AgentData
    {
        public float2 position;
        public float2 velocity;
        public float  radius;
        public float  maxSpeed;
    }

    /// <summary>
    /// A half-plane constraint: all velocities v where dot(v - point, normal) >= 0 are valid.
    /// Stored as (point, direction) where direction is along the constraint boundary and
    /// the valid half-plane is to the LEFT of direction.
    /// </summary>
    public struct ORCALine
    {
        public float2 point;
        public float2 direction; // unit length, boundary direction
    }

    // scratch buffer sizes
    private const int MAX_ORCA_LINES = 32;

    /// <summary>
    /// Compute the ORCA-adjusted velocity for a single agent.
    /// </summary>
    /// <param name="preferredVelocity">Desired velocity from pathfinding.</param>
    /// <param name="position">Agent's 2D position.</param>
    /// <param name="radius">Agent's collision radius.</param>
    /// <param name="maxSpeed">Agent's max speed.</param>
    /// <param name="neighbors">Slice of nearby agent data.</param>
    /// <param name="neighborCount">Number of valid entries in neighbors.</param>
    /// <param name="timeHorizon">Seconds ahead to plan for (higher = earlier avoidance).</param>
    /// <returns>Adjusted velocity closest to preferredVelocity satisfying all ORCA constraints.</returns>
    public static float2 ComputeNewVelocity(
        float2 preferredVelocity,
        float2 position,
        float2 velocity,
        float  radius,
        float  maxSpeed,
        in NativeArray<AgentData> neighbors,
        int    neighborCount,
        float  timeHorizon)
    {
        // stack-allocate ORCA line buffer (Burst supports fixed arrays via unsafe)
        var orcaLines = new NativeArray<ORCALine>(
            math.min(neighborCount, MAX_ORCA_LINES),
            Allocator.Temp,
            NativeArrayOptions.UninitializedMemory);

        int lineCount = 0;

        for (int i = 0; i < neighborCount && lineCount < orcaLines.Length; i++)
        {
            AgentData other = neighbors[i];

            float2 relPos = other.position - position;
            float2 relVel = velocity - other.velocity;
            float  combinedRadius = radius + other.radius;
            float  distSq = math.lengthsq(relPos);

            ORCALine line;

            if (distSq > combinedRadius * combinedRadius)
            {
                // no collision — project on truncated VO cone
                line = ComputeORCALineNoCollision(
                    relPos, relVel, combinedRadius, distSq, timeHorizon);
            }
            else
            {
                // already overlapping — push apart
                line = ComputeORCALineCollision(
                    relPos, relVel, combinedRadius, distSq, timeHorizon);
            }

            orcaLines[lineCount++] = line;
        }

        float2 result = SolveLinearProgram(orcaLines, lineCount, maxSpeed, preferredVelocity);

        orcaLines.Dispose();
        return result;
    }

    // ----------------------------------------------------------------
    // ORCA line computation (no collision)
    // ----------------------------------------------------------------
    static ORCALine ComputeORCALineNoCollision(
        float2 relPos,
        float2 relVel,
        float  combinedRadius,
        float  distSq,
        float  timeHorizon)
    {
        float invTimeHorizon = 1f / timeHorizon;

        // vector from cutoff center to relative velocity
        float2 w = relVel - invTimeHorizon * relPos;
        float  wLenSq = math.lengthsq(w);

        float dotProduct1 = math.dot(w, relPos);

        ORCALine line;

        if (dotProduct1 < 0f && dotProduct1 * dotProduct1 > combinedRadius * combinedRadius * wLenSq)
        {
            // project on cutoff circle
            float wLen = math.sqrt(wLenSq);
            float2 unitW = w / math.max(wLen, 1e-6f);

            line.direction = new float2(unitW.y, -unitW.x);
            float2 u = (combinedRadius * invTimeHorizon - wLen) * unitW;
            line.point = relVel - u + 0.5f * u; // velocity + 0.5 * u (take half responsibility)
        }
        else
        {
            // project on legs
            float leg = math.sqrt(math.max(distSq - combinedRadius * combinedRadius, 0f));

            if (Det(relPos, w) > 0f)
            {
                // project on left leg
                line.direction = new float2(
                    relPos.x * leg - relPos.y * combinedRadius,
                    relPos.x * combinedRadius + relPos.y * leg) / distSq;
            }
            else
            {
                // project on right leg
                line.direction = -new float2(
                    relPos.x * leg + relPos.y * combinedRadius,
                    -relPos.x * combinedRadius + relPos.y * leg) / distSq;
            }

            float dotProduct2 = math.dot(relVel, line.direction);
            float2 u = dotProduct2 * line.direction - relVel;
            line.point = relVel + 0.5f * u; // take half responsibility
        }

        return line;
    }

    // ----------------------------------------------------------------
    // ORCA line computation (already colliding)
    // ----------------------------------------------------------------
    static ORCALine ComputeORCALineCollision(
        float2 relPos,
        float2 relVel,
        float  combinedRadius,
        float  distSq,
        float  timeHorizon)
    {
        // use a very short time horizon to push apart quickly
        float invTimeStep = 10f; // use a high value to resolve quickly

        float2 w = relVel - invTimeStep * relPos;
        float  wLen = math.length(w);

        float2 unitW = wLen > 1e-6f ? w / wLen : new float2(1, 0);

        ORCALine line;
        line.direction = new float2(unitW.y, -unitW.x);
        float2 u = (combinedRadius * invTimeStep - wLen) * unitW;
        line.point = relVel + 0.5f * u;

        return line;
    }

    // ----------------------------------------------------------------
    // 2D Linear Programming — find velocity closest to preferred
    // that satisfies all half-plane constraints + speed limit
    // ----------------------------------------------------------------
    static float2 SolveLinearProgram(
        NativeArray<ORCALine> lines,
        int          lineCount,
        float        maxSpeed,
        float2       preferredVelocity)
    {
        float2 result = preferredVelocity;

        // clamp preferred velocity to max speed circle
        if (math.lengthsq(result) > maxSpeed * maxSpeed)
        {
            result = math.normalizesafe(result) * maxSpeed;
        }

        for (int i = 0; i < lineCount; i++)
        {
            // check if current result already satisfies line i
            if (Det(lines[i].direction, lines[i].point - result) > 0f)
            {
                // violated — project onto this line
                float2 tempResult = result;
                if (!SolveLinearProgram1(lines, i, maxSpeed, ref result))
                {
                    result = tempResult;
                    // can't satisfy — try to get as close as possible
                    SolveLinearProgram2(lines, i, maxSpeed, ref result);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Solve LP with a new constraint line added (line index i).
    /// Project result onto line i while satisfying lines 0..i-1.
    /// </summary>
    static bool SolveLinearProgram1(
        NativeArray<ORCALine> lines,
        int          lineIndex,
        float        maxSpeed,
        ref float2   result)
    {
        float2 linePoint = lines[lineIndex].point;
        float2 lineDir   = lines[lineIndex].direction;

        float dotProduct  = math.dot(linePoint, lineDir);
        float discriminant = dotProduct * dotProduct + maxSpeed * maxSpeed - math.lengthsq(linePoint);

        if (discriminant < 0f)
        {
            // max speed circle doesn't intersect this constraint line
            return false;
        }

        float sqrtDisc = math.sqrt(discriminant);
        float tLeft  = -dotProduct - sqrtDisc;
        float tRight = -dotProduct + sqrtDisc;

        for (int j = 0; j < lineIndex; j++)
        {
            float denominator = Det(lineDir, lines[j].direction);
            float numerator   = Det(lines[j].direction, linePoint - lines[j].point);

            if (math.abs(denominator) <= 1e-6f)
            {
                // lines are parallel
                if (numerator < 0f)
                    return false;
                continue;
            }

            float t = numerator / denominator;

            if (denominator >= 0f)
            {
                // constraint from the right
                tRight = math.min(tRight, t);
            }
            else
            {
                // constraint from the left
                tLeft = math.max(tLeft, t);
            }

            if (tLeft > tRight)
                return false;
        }

        // project preferred velocity onto the valid range on this line
        float tOpt = math.dot(lineDir, result - linePoint);
        tOpt = math.clamp(tOpt, tLeft, tRight);

        result = linePoint + tOpt * lineDir;
        return true;
    }

    /// <summary>
    /// Fallback: when LP1 fails, find the safest possible velocity.
    /// Tries to minimally violate constraints.
    /// </summary>
    static void SolveLinearProgram2(
        NativeArray<ORCALine> lines,
        int        numObstLines,
        float      maxSpeed,
        ref float2 result)
    {
        float distance = 0f;

        for (int i = 0; i < numObstLines; i++)
        {
            float det = Det(lines[i].direction, lines[i].point - result);
            if (det > distance)
            {
                distance = det;

                // project result onto this most-violated line
                float2 lineDir   = lines[i].direction;
                float2 linePoint = lines[i].point;

                float t = math.dot(lineDir, result - linePoint);
                result = linePoint + t * lineDir;

                // clamp to speed limit
                if (math.lengthsq(result) > maxSpeed * maxSpeed)
                {
                    result = math.normalizesafe(result) * maxSpeed;
                }
            }
        }
    }

    // ----------------------------------------------------------------
    // Utilities
    // ----------------------------------------------------------------

    /// <summary> 2D cross product / determinant. </summary>
    public static float Det(float2 a, float2 b)
    {
        return a.x * b.y - a.y * b.x;
    }
}
