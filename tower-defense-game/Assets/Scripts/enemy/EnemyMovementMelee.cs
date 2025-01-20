using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;



public class EnemyMovementMelee : EnemyMovement
{
    // public float range = 2.7f;
    
    public override void Attack(Transform target)
    {
        transform.LookAt(target);
        BattleSystem battleSystem = target.GetComponent<BattleSystem>(); // interacts directly with the target rather than creating a projectile
        if (battleSystem != null)
        {
            battleSystem.TakeDamage(dmg);
        }
        atkTimer = atkCooldown;
    }

}
