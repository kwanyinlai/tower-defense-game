using UnityEngine;
using System.Collections.Generic;

public class EnemyBattleSystem : BattleSystem
{
    void Start()
    {
        currentHealth = maxHealth;
    }

    protected override void Die(){
        
        EnemyMovement.enemies.Remove(gameObject);
        foreach(GameObject player in Player.players){
            List<GameObject> list = player.GetComponent<TroopManagment>().selectedTroops;
            if (list.Contains(gameObject)){
                list.Remove(gameObject);
                break;
            }
        } 
        EnemyMovement.enemies.Remove(gameObject);

        base.Die();
    }
}
