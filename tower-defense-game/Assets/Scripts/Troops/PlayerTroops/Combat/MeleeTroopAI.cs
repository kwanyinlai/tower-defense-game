using UnityEngine;

public class MeleeTroopAI : PlayerTroopAI
{
    public override void Attack(Transform target)
    {
        transform.LookAt(enemyTarget);
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w); // essentially only allows the y axis to move
        
        CombatSystem combatSystem = enemyTarget.GetComponent<CombatSystem>(); // interacts directly with the target rather than creating a projectile
        if (combatSystem != null)
        {
            combatSystem.TakeDamage((int)(dmg * (1 + combatSystem.GetEffectStrength("attackBuff") - combatSystem.GetEffectStrength("attackWeaken"))));
        }
        atkTimer = atkCooldown;
    }

    protected override void IntializeSellResources()
    {
        //TODO: Remove and replace with code to actually add the correct resources based on building
        sellResources.Add("Wood", 100);
        sellResources.Add("TestResource2", 100);
    }
}
