using UnityEngine;
using System.Collections.Generic;

public class TroopBattleSystem : BattleSystem
{
    private Barracks barracks;

    public void setBarracks(Barracks barracks){
        this.barracks = barracks;
    }

    void Start()
    {
        currentHealth = maxHealth;
        tagList = new HashSet<string>(viewableTagList);
    }
     
    protected override void Die(){
        
        barracks.DecrementTroops();
        foreach (GameObject troop in TroopMovement.troops)
        {
            if (troop == gameObject)
            {
                TroopMovement.troops.Remove(troop);
                GameObject temp = gameObject.GetComponent<TroopMovement>().waypoint;
                if (temp!=null){
                    temp.GetComponent<Waypoint>().troopsBound.Remove(gameObject);
                }
                    

                foreach(GameObject player in Player.players){
                    List<GameObject> selected = player.GetComponent<TroopManagment>().selectedTroops;
                    if(selected.Contains(troop)){
                        selected.Remove(troop);
                    }
                }
                
                break; 
            }
        }

        base.Die();
    }

    public override void TakeDamage(int damage)
    {

        base.TakeDamage(damage);

        TroopMovement temp = gameObject.GetComponent<TroopMovement>();

        float tempRange = temp.aggroRange;
        temp.aggroRange += 50f; // large enough to encapsulate any enemy but to avoid
                                // damage from artillery (in case we implement super
                                // long-range units)
        temp.enemyTarget = temp.GetClosestEnemyInRange();
        temp.aggroRange = tempRange;

        
        


    }
    
}
