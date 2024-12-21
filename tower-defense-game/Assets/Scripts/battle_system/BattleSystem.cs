using UnityEngine;

public abstract class BattleSystem : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    

    public float GetPercentageHP()
    {
        return currentHealth / maxHealth;
    }
     public void TakeDamage(int damage)
    {
        currentHealth -= damage;
       
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Handle the target's death
    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} has died!");
        Destroy(gameObject);  
    }
}


