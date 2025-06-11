using UnityEngine;
using System.Collections.Generic;

public abstract class CombatSystem : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public string[] viewableTagList; // A Viewable List To Easily Modify Tags

    protected HashSet<string> tagList;  // Uses a Set for Efficiency When Looking For Overlaps

    public float GetPercentageHP()
    {
        return currentHealth / maxHealth;
    }
    
    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;

       
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} has died!");
        Destroy(gameObject);  
    }

    public HashSet<string> GetTagList() 
    {
        return tagList;
    }

}


