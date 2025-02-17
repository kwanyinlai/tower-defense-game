using UnityEngine;

public class TroopMovementMelee : TroopMovement
{
    public override void Attack()
    {
        transform.LookAt(enemyTarget);
        BattleSystem battleSystem = enemyTarget.GetComponent<BattleSystem>(); // interacts directly with the target rather than creating a projectile
        if (battleSystem != null)
        {
            battleSystem.TakeDamage(dmg);
        }
        atkTimer = atkCooldown;
    }
}
