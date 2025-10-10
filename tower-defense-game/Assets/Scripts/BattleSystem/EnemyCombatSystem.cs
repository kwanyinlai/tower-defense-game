using UnityEngine;
using System.Collections.Generic;

public class EnemyCombatSystem : CombatSystem
{
    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth;
        tagList = new HashSet<string>(viewableTagList);
    }

    protected override void Die(){
        
        EnemyAI.enemies.Remove(gameObject);
        foreach(GameObject player in Player.players){
            List<GameObject> list = player.GetComponent<TroopManagment>().SelectedTroops;
            if (list.Contains(gameObject)){
                list.Remove(gameObject);
                break;
            }
        }

        base.Die();
    }
}
