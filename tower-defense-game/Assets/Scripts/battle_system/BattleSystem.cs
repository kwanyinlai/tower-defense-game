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
        Debug.Log($"{gameObject.name} took {damage} damage. HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Handle the target's death
    void Die()
    {
        Debug.Log($"{gameObject.name} has died!");
        Destroy(gameObject);  
    }
}


