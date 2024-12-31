using UnityEngine;
using System.Collections.Generic;

public class EnemyBattleSystem : BattleSystem
{
    void Start()
    {
        currentHealth = maxHealth;
    }

    protected override void Die(){
        foreach(var enemy in EnemyMovement.enemies)
        {
            if (enemy == gameObject)
            {
                EnemyMovement.enemies.Remove(enemy); 
                break; 
            }
        }

        base.Die();
    }
}
