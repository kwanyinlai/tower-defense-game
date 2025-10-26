using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;

[CreateAssetMenu(menuName = "Troop Behaviours/Radius Healer Behavior")]
public class RadiusHealerBehaviour : ISupportBehaviour
{

    public float healStrength = 10;
    public float healCooldown = 2;
    protected List<GameObject> allyList = null;

    public override void InteractWithTarget(TroopCombatSystem selfCombatSystem, TroopCombatSystem enemyCombatSystem)
    {
        allyList = GetAlliesInRange(selfCombatSystem);
        Heal(allyList);
    }

    public void Heal(List<GameObject> allyList)
    {
        TroopCombatSystem targetCombat;
        foreach (var targetAlly in allyList)
        {
            targetCombat = targetAlly.GetComponent<TroopCombatSystem>();
            float previousHealStrength = targetCombat.GetEffectStrength("heal");
            if (previousHealStrength < healStrength)
            {
                targetCombat.ApplyEffect("heal", healStrength, healCooldown);
                targetCombat.AddHealth(healStrength - previousHealStrength);
            }
        }

    }
}
