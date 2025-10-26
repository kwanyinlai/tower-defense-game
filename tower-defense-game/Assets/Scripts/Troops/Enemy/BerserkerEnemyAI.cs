using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;


[CreateAssetMenu(menuName = "Troop Behaviours/Berserker Behavior")]
public class BerserkerBehaviour : IAttackBehaviour
{
    public float hasteEffectDecimal = 0.10f;
    public float troopStrengthDecimal = 0.50f;

    protected override void ApplyBuffOnStart(TroopCombatSystem selfCombatSystem)
    {
        selfCombatSystem.ApplyEffect("haste", hasteEffectDecimal, -1); // TODO: check
    }

}
