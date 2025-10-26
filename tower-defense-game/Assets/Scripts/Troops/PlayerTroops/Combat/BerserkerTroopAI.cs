// using UnityEngine;

// public class BerserkerTroopAI : PlayerTroopAI
// {
//     public float hasteEffectDecimal = 0.10f;
//     public float troopStrengthDecimal = 0.50f;

//     protected override void Start()
//     {
//         base.Start();
//         combatSystem.ApplyEffect("haste", hasteEffectDecimal, -1);
//     }
    
//     public override void Attack(Transform target)
//     {
//         transform.LookAt(enemyTarget);
//         transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w); // essentially only allows the y axis to move
        
//         CombatSystem combatSystem = enemyTarget.GetComponent<CombatSystem>(); // interacts directly with the target rather than creating a projectile
//         if (combatSystem != null)
//         {
//             combatSystem.TakeDamage((int)(dmg * ((enemyTarget == null) ? 1.0f : 1 + troopStrengthDecimal) * (1 + combatSystem.GetEffectStrength("attackBuff") - combatSystem.GetEffectStrength("attackWeaken"))));
//         }
//         atkTimer = atkCooldown;
//     }
// }
