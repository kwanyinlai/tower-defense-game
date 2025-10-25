using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;


[CreateAssetMenu(menuName = "Troop Behaviours/Single Healer Behavior")]
public class SingleHealerAIBehaviour : ISupportBehaviour
{

    public float healStrength = 10;
    public float healCooldown = 2;

    public override void InteractWithTarget(CombatSystem combatSystem)
    {
        Heal(combatSystem);
    }
    public void Heal(CombatSystem targetCombat)
    {
        float previousHealStrength = targetCombat.GetEffectStrength("heal");
        targetCombat.ApplyEffect("heal", healStrength, healCooldown);
        targetCombat.AddHealth(healStrength - previousHealStrength);
    }

}
