using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;

public abstract class SupportEnemyAI : EnemyAI
{
    public Transform GetBestAllyInRange()
    {
        if (EnemyAI.enemies.Count == 0)
        {
            return null;
        }

        
        List<GameObject> potentialAllies = new List<GameObject>();

        // adds allies within range
        foreach (var ally in EnemyAI.enemies)
        {
            float distance = Vector3.Distance(transform.position, ally.transform.position);

            if (distance <= aggroRange)
            {
                potentialAllies.Add(ally);
            }
        }

        GameObject bestAlly = null;

         // remove from potential if ally at 100% or is on cooldown
        for(int i = potentialAllies.Count - 1; i >= 0; i--)
        {
            float currHealth = potentialAllies[i].GetComponent<CombatSystem>().currentHealth;
            float currMaxHealth = potentialAllies[i].GetComponent<CombatSystem>().maxHealth;

            if (currMaxHealth == currHealth || potentialAllies[i].GetComponent<CombatSystem>().GetEffectStrength("heal") != 0)
            {
                potentialAllies.RemoveAt(i);
            }
        }


        if(potentialAllies.Count > 0)
        {
            bestAlly = potentialAllies[0];
            float lowestHealth = bestAlly.GetComponent<CombatSystem>().currentHealth;
            float maxHealth = bestAlly.GetComponent<CombatSystem>().maxHealth;
            
            // determines best ally
            foreach (var ally in potentialAllies)
            {
                float distance = Vector3.Distance(transform.position, ally.transform.position);
                float currHealth = ally.GetComponent<CombatSystem>().currentHealth;
                float currMaxHealth = ally.GetComponent<CombatSystem>().maxHealth;

                if (currHealth <= lowestHealth)
                {
                    bestAlly = ally;
                    lowestHealth = currHealth;
                    maxHealth = currMaxHealth;
                }
            }
        }

        if (bestAlly == null) { return null; }
        return bestAlly.transform;
    }

    public List<GameObject> GetAlliesInRange()
    {        
        List<GameObject> allies = new List<GameObject>();

        // adds allies within range
        foreach (var ally in EnemyAI.enemies)
        {
            CombatSystem targetCombat = ally.GetComponent<CombatSystem>();
            float distance = Vector3.Distance(transform.position, ally.transform.position);

            if (distance <= aggroRange && targetCombat.currentHealth != targetCombat.maxHealth)
            {
                allies.Add(ally);
            }
        }
        
        return allies.Count == 0 ? null : allies;
    }
}
