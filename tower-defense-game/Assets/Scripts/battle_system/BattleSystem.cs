using UnityEngine;

public abstract class BattleSystem : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public abstract void DecreaseHP(float damage);

    public float GetPercentageHP()
    {
        return currentHealth / maxHealth;
    }

}
