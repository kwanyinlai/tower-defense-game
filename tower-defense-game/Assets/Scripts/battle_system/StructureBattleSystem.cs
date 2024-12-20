using UnityEngine;

public class StructureBattleSystem : BattleSystem
{
    void Start()
    {
        currentHealth = maxHealth;
    }

    public override void DecreaseHP(float damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0f)
            currentHealth = 0f;  
    }
}
