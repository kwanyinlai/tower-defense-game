using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;



public class TankEnemyAI : EnemyAI
{
    public float range = 2f;
    public float slowEffectDecimal = 0.10f;

    protected void Start()
    {
        base.Start();
        combatSystem.ApplyEffect("slow", slowEffectDecimal, -1);
    }
    
    public override void Attack(Transform target)
    {
        transform.LookAt(target);
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w); // essentially only allows the y axis to move
        
        CombatSystem combatSystem = target.GetComponent<CombatSystem>(); // interacts directly with the target rather than creating a projectile
        if (combatSystem != null)
        {
            combatSystem.TakeDamage(dmg);
        }
        atkTimer = atkCooldown;
    }

}
