using UnityEngine;

public class MeleeTroopAI : TroopAI
{
    public override void Attack()
    {
        transform.LookAt(enemyTarget);
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w); // essentially only allows the y axis to move
        
        CombatSystem combatSystem = enemyTarget.GetComponent<CombatSystem>(); // interacts directly with the target rather than creating a projectile
        if (combatSystem != null)
        {
            combatSystem.TakeDamage(dmg);
        }
        atkTimer = atkCooldown;
    }
}
