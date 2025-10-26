using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;

[CreateAssetMenu(menuName = "Troop Behaviours/Musician Behaviour")]
public class MusicianTroopAI : ISupportBehaviour
{

    public float attackBuffStrengthDecimal = 1.25f;
    public float attackCooldown = 2f;
    protected List<GameObject> allyList = null;

    public override void InteractWithTarget(TroopCombatSystem selfCombatSystem, TroopCombatSystem enemyCombatSystem)
    {
        allyList = GetAlliesInRange(selfCombatSystem);

        if (allyList != null && allyList.Count != 0)
        {
            BuffAttack(allyList);
        }

    }
    public void BuffAttack(List<GameObject> allyList)
    {
        // CombatSystem targetCombat; // commenting out for clarity, uncomment if needed
        foreach (var targetAlly in allyList)
        {
            targetAlly.GetComponent<TroopCombatSystem>().ApplyEffect("attackBuff", attackBuffStrengthDecimal, attackCooldown);
        }
    }

}
