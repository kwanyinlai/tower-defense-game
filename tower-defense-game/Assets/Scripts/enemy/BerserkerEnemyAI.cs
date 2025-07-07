using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;



public class BerserkerEnemyAI : EnemyAI
{
    public float hasteEffectDecimal = 0.10f;
    public float troopStrengthDecimal = 1.50f;

    protected void Start()
    {
        base.Start();
        combatSystem.ApplyEffect("haste", hasteEffectDecimal, -1);
    }
    
    public override void Attack(Transform target)
    {
        transform.LookAt(target);
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w); // essentially only allows the y axis to move
        
        CombatSystem combatSystem = target.GetComponent<CombatSystem>(); // interacts directly with the target rather than creating a projectile
        if (combatSystem != null)
        {
            Debug.Log((int)((float)dmg * ((troopTarget == null) ? 1.0f : troopStrengthDecimal)));
            combatSystem.TakeDamage((int)((float)dmg * ((troopTarget == null) ? 1.0f : troopStrengthDecimal)));
        }
        atkTimer = atkCooldown;
    }

}
