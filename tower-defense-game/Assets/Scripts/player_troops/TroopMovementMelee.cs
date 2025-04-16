using UnityEngine;

public class TroopMovementMelee : TroopMovement
{
    public override void Attack()
    {
        transform.LookAt(enemyTarget);
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w); // essentially only allows the y axis to move
        
        BattleSystem battleSystem = enemyTarget.GetComponent<BattleSystem>(); // interacts directly with the target rather than creating a projectile
        if (battleSystem != null)
        {
            battleSystem.TakeDamage(dmg);
        }
        atkTimer = atkCooldown;
    }
}
