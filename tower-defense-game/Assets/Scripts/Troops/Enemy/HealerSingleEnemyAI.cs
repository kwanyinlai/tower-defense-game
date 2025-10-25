using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;

public class SingleHealerAIBehaviour : ISupportBehaviour
{

    public float healStrength = 10;
    public float healCooldown = 2;

    public override void InteractWithTarget(CombatSystem combatSystem)
    {
        Heal(combatSystem);
    }
    public void Heal(Transform bulletTarget)
    {
        float previousHealStrength = targetCombat.GetEffectStrength("heal");
        targetCombat.ApplyEffect("heal", healStrength, healCooldown);
        targetCombat.AddHealth(healStrength - previousHealStrength);
    }
    
}
