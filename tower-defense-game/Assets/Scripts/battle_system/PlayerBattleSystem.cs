using UnityEngine;
using System.Collections.Generic;

public class PlayerBattleSystem : BattleSystem
{
    void Start()
    {
        currentHealth = maxHealth;
    }
     
    protected override void Die(){
        LinkedListNode<GameObject> currentNode = PlayerMovement.troops.First;
        while (currentNode != null)
        {
            if (currentNode.Value == gameObject)
            {
                PlayerMovement.troops.Remove(currentNode); 
                break; 
            }
            currentNode = currentNode.Next;
        }

        base.Die();
    }
}
