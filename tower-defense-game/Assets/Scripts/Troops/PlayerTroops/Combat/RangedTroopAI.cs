// using UnityEngine;
// using UnityEngine.AI;
// using System.Collections.Generic;

// public class RangedTroopAI : PlayerTroopAI
// {
//     public GameObject bulletPrefab; 
//     public override void Attack(Transform target)
//     {
//         transform.LookAt(enemyTarget);
//         transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w); // essentially only allows the y axis to move
        
//         CombatSystem combatSystem = enemyTarget.GetComponent<CombatSystem>(); // interacts directly with the target rather than creating a projectile
//         if (combatSystem != null)
//         {
//             combatSystem.TakeDamage((int)(dmg * (1 + combatSystem.GetEffectStrength("attackBuff") - combatSystem.GetEffectStrength("attackWeaken"))));
//         }
//         atkTimer = atkCooldown;

//     }

  

// }



