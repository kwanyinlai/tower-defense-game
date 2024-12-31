using UnityEngine;
using System.Collections.Generic;

public class PlayerBattleSystem : BattleSystem
{
    private Barracks barracks;

    public void setBarracks(Barracks barracks){
        this.barracks = barracks;
    }

    void Start()
    {
        currentHealth = maxHealth;
    }
     
    protected override void Die(){
        
        barracks.DecrementTroops();
        foreach (GameObject troop in PlayerMovement.troops)
        {
            if (troop == gameObject)
            {
                PlayerMovement.troops.Remove(troop);
                GameObject temp = gameObject.GetComponent<PlayerMovement>().waypoint;
                if (temp!=null){
                    temp.GetComponent<Waypoint>().troopsBound.Remove(gameObject);
                }
                    

                foreach(GameObject player in Player.players){
                    List<GameObject> selected = player.GetComponent<TroopManagment>().selected;
                    if(selected.Contains(troop)){
                        selected.Remove(troop);
                    }
                }
                
                break; 
            }
        }

        base.Die();
    }
}
