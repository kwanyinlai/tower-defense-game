using UnityEngine;
using System.Collections.Generic;
public abstract class ITroopBehaviour : ScriptableObject
{
    public abstract void InteractWithTarget(TroopCombatSystem selfCombatSystem, TroopCombatSystem enemyCombatSystem);

    protected virtual void ApplyBuffOnStart(TroopCombatSystem selfCombatSystem)
    {
        ;
    }
}

public abstract class IAttackBehaviour : ITroopBehaviour
{
    public override void InteractWithTarget(TroopCombatSystem selfCombatSystem, TroopCombatSystem enemyCombatSystem)
    {
        enemyCombatSystem.TakeDamage((int)(selfCombatSystem.Attack * (1 + selfCombatSystem.GetEffectStrength("attackBuff") - selfCombatSystem.GetEffectStrength("attackWeaken"))));

    }
}

public abstract class ISupportBehaviour: ITroopBehaviour
{
    public List<GameObject> GetAlliesInRange(TroopCombatSystem selfCombatSystem)
    {        
        List<GameObject> allies = new List<GameObject>();

        // adds allies within range
        foreach (var ally in EnemyAI.enemies)
        {
            CombatSystem targetCombat = ally.GetComponent<CombatSystem>();
            float distance = Vector3.Distance(selfCombatSystem.transform.position, ally.transform.position);

            if (distance <= selfCombatSystem.aggroRange && targetCombat.currentHealth != targetCombat.maxHealth)
            {
                allies.Add(ally);
            }
        }
        
        return allies.Count == 0 ? null : allies;
    }


}
