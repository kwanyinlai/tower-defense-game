using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;

public class MusicianTroopAI : ISupportBehaviour
{

    public float attackBuffStrengthDecimal = 1.25f;
    public float attackCooldown = 2f;
    protected List<GameObject> allyList = null;

    public override void InteractWithTarget(CombatSystem combatSystem)
    {
        allyList = GetAlliesInRange();
        
        if (allyList != null && allyList.Count != 0)
        {
            enemyTarget = allyList[0].transform;
        }        
        BuffAttack(allyList);
    }
    public void BuffAttack(List<GameObject> allyList)
    {
        // CombatSystem targetCombat; // commenting out for clarity, uncomment if needed
        foreach (var targetAlly in allyList)
        {
            targetAlly.GetComponent<CombatSystem>().ApplyEffect("attackBuff", attackBuffStrengthDecimal, attackCooldown);
        }
    }

}
