using UnityEngine;
public abstract class ITroopBehaviour : ScriptableObject
{
    public abstract void InteractWithTarget();

}
public abstract class IAttackBehaviour : ITroopBehaviour
{
    public void InteractWithTarget(CombatSystem combatSystem)
    {
        combatSystem.TakeDamage((int)(dmg * (1 + combatSystem.GetEffectStrength("attackBuff") - combatSystem.GetEffectStrength("attackWeaken"))));

    }
}

public abstract class ISupportBehaviour: ITroopBehaviour
{
    public List<GameObject> GetAlliesInRange()
    {        
        List<GameObject> allies = new List<GameObject>();

        // adds allies within range
        foreach (var ally in TroopAI.troops)
        {
            CombatSystem targetCombat = ally.GetComponent<CombatSystem>();
            float distance = Vector3.Distance(transform.position, ally.transform.position);

            if (distance <= aggroRange)
            {
                allies.Add(ally);
            }
        }
        
        return allies.Count == 0 ? null : allies;
    }


    public Transform GetBestAllyInRange()
    {
        if (TroopAI.troops.Count == 0)
        {
            return null;
        }

        List<GameObject> potentialAllies = new List<GameObject>();

        // adds allies within range
        foreach (var ally in TroopAI.troops)
        {
            float distance = Vector3.Distance(transform.position, ally.transform.position);

            if (distance <= aggroRange)
            {
                potentialAllies.Add(ally);
            }
        }

        GameObject bestAlly = null;

        // remove from potential if ally at 100% or is on cooldown
        for (int i = potentialAllies.Count - 1; i >= 0; i--)
        {
            float currHealth = potentialAllies[i].GetComponent<CombatSystem>().currentHealth;
            float currMaxHealth = potentialAllies[i].GetComponent<CombatSystem>().maxHealth;

            if (currMaxHealth == currHealth || potentialAllies[i].GetComponent<CombatSystem>().GetEffectStrength("heal") != 0)
            {
                potentialAllies.RemoveAt(i);
            }
        }


        if (potentialAllies.Count > 0)
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
}
