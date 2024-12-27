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
        LinkedListNode<GameObject> currentNode = PlayerMovement.troops.First;
        barracks.DecrementTroops();
        while (currentNode != null)
        {
            if (currentNode.Value == gameObject)
            {
                PlayerMovement.troops.Remove(currentNode); 
                TroopManagment.selected.Remove(currentNode.Value);
                break; 
            }
            currentNode = currentNode.Next;
        }

        base.Die();
    }
}
