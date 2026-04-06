using UnityEngine;

public interface ILocalAvoidance
{
    /// <summary>
    /// Adjust the current direction to avoid obstacles nearby
    /// </summary>
    /// <param name="currentPosition">Current position of the troop</param>
    /// <param name="desiredDirection">The original direction.</param>
    /// <param name="speed">Speed of the troop.</param>
    /// <returns>Adjusted movement direction after avoidance</returns>
    Vector3 GetAvoidanceDirection(Vector3 currentPosition, Vector3 desiredDirection, float speed);
}
