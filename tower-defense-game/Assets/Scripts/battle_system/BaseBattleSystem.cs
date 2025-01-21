
using UnityEngine;

public class BaseBattleSystem : BattleSystem
{

    void Start()
    {
        currentHealth = maxHealth;
    }
    protected override void Die()
    {
        transform.localScale = Vector3.zero; // temp fix for now, this just hides the base when destroyed
    }
}
