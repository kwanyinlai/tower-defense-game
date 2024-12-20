using UnityEngine;
using System.Collections.Generic;

public class EnemyBattleSystem : BattleSystem
{
    void Start()
    {
        currentHealth = maxHealth;
    }

    protected override void Die(){
        LinkedListNode<GameObject> currentNode = EnemyMovement.enemies.First;
        while (currentNode != null)
        {
            if (currentNode.Value == gameObject)
            {
                EnemyMovement.enemies.Remove(currentNode); 
                break; 
            }
            currentNode = currentNode.Next;
        }

        base.Die();
    }
}
