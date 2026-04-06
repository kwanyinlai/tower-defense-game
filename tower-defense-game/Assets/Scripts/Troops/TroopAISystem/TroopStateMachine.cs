using UnityEngine;

public class TroopStateMachine
{
    public TroopState DetermineState(
        TroopState currentState,
        Transform target,
        TroopCombat combat,
        TroopMovement movement,
        TroopStats stats)
    {
        if (currentState == TroopState.Dead)
        {
            return TroopState.Dead;
        }

        if (target == null)
        {
            if (movement.IsMoving())
            {
                return TroopState.MovingToTarget;
            }
            return TroopState.Idle;
        }

        float distanceToTarget = Vector3.Distance(movement.transform.position, target.position);
        float attackRange = combat.AttackRange;

        if (distanceToTarget <= attackRange && movement.GetCurrentSpeed() < 0.1f)
        {
            return TroopState.InCombat;
        }

        // target but not in range
        if (distanceToTarget > attackRange)
        {
            return TroopState.MovingToTarget;
        }

        // otherwise we're in attack range so combat
        return TroopState.InCombat;
    }
}