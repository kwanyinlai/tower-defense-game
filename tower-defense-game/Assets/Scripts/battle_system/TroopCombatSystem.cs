using UnityEngine;
using System.Collections.Generic;

public class TroopCombatSystem : CombatSystem
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
        foreach (GameObject troop in TroopAI.troops)
        {
            if (troop == gameObject)
            {
                TroopAI.troops.Remove(troop);
                GameObject temp = gameObject.GetComponent<TroopAI>().waypoint;
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

        /*TroopAI temp = gameObject.GetComponent<TroopAI>();

        float tempRange = temp.aggroRange;
        temp.aggroRange += 50f; // large enough to encapsulate any enemy but to avoid
                                // damage from artillery (in case we implement super
                                // long-range units)
        temp.enemyTarget = temp.GetClosestEnemyInRange();
        temp.aggroRange = tempRange;
        */

        // commenting this out for now because I don't know if we want this actually
        // just to make the player do more and have more control rather than having
        // AI go berserk

        
        


    }
    
}
