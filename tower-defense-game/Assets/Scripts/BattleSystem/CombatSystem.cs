using UnityEngine;
using System.Collections.Generic;

public abstract class CombatSystem : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float shield = 0f;

    [Header("Target Tags")]
    public string[] viewableTagList;
    protected HashSet<string> tagList;
    
    // storing dictionary with strongest effect only
    private Dictionary<StatusClass, StatusEffect> activeEffects = new Dictionary<StatusClass, StatusEffect>();

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        tagList = new HashSet<string>(viewableTagList);
        InitializeEffects();
    }

    #region Health & Damage

    public virtual void TakeDamage(float damage)
    {
        DamageVFX();

        // Shield absorbs damage first
        if (shield > 0)
        {
            float damageAfterShield = damage - shield;
            shield = Mathf.Max(0, shield - damage);
            
            if (damageAfterShield > 0)
            {
                currentHealth -= damageAfterShield;
            }
        }
        else
        {
            currentHealth -= damage;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public void AddShield(float amount)
    {
        shield += amount;
    }

    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} has died!");
        Destroy(gameObject);
    }

    #endregion


    protected virtual void DamageVFX()
    {
        int blockCount = 5;
        float blockSize = 0.2f;
        float destroyTime = 4f;

        for (int i = 0; i < blockCount; i++)
        {
            GameObject blood = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blood.transform.localScale = new Vector3(blockSize, blockSize, blockSize);
            blood.transform.position = transform.position + new Vector3(0, 1.5f, 0) + Random.insideUnitSphere;
            blood.transform.rotation = Random.rotation;

            blood.GetComponent<Renderer>().material.color = Color.red;
            Rigidbody rb = blood.AddComponent<Rigidbody>();
            rb.AddForce(Random.onUnitSphere, ForceMode.Impulse);
            Destroy(blood, destroyTime);
        }
    }


    #region Status Effects

    protected virtual void InitializeEffects()
    {
        // register the effect; can be overriden in subclasses for subclass 
        // specific effects (only for organisation, not performance differences)
        RegisterEffect(StatusClass.Heal);
        RegisterEffect(StatusClass.Slow);
        RegisterEffect(StatusClass.Haste);
        RegisterEffect(StatusClass.Burn);
        RegisterEffect(StatusClass.AttackBuff);
        RegisterEffect(StatusClass.AttackDebuff);
        RegisterEffect(StatusClass.Stun);
    }

    private void RegisterEffect(StatusClass effectName)
    {
        if (!activeEffects.ContainsKey(effectName))
        {
            activeEffects[effectName] = new StatusEffect { name = effectName };
        }
    }

    /// <summary>
    /// Apply a status effect. If effect exists, takes the stronger one.
    /// </summary>
    /// <param name="effectName">Name of the status effect (register in InitializeEffects in CombatSystem.cs)</param>
    /// <param name="strength">Effect strength (0-1)</param>
    /// <param name="duration">Duration in seconds. Use -1 for permanent.</param>
    public void ApplyEffect(StatusClass effectName, float strength, float duration)
    {
        if (!activeEffects.ContainsKey(effectName))
        {
            Debug.LogError($"Effect '{effectName}' not registered! Add it to InitializeEffects()", this);
            return;
        }

        StatusEffect effect = activeEffects[effectName];
        
        // keep stronger effect
        if (strength > effect.strength || effect.IsExpired())
        {
            effect.strength = strength;
            effect.duration = duration;
            effect.startTime = Time.time;
        }
        // extend duration if same effect
        else if (strength == effect.strength && duration > effect.GetRemainingDuration())
        {
            effect.duration = duration;
            effect.startTime = Time.time;
        }
    }

    public float GetEffectStrength(StatusClass effectName)
    {
        if (!activeEffects.ContainsKey(effectName))
            return 0f; // 0 if doesn't possess effect

        StatusEffect effect = activeEffects[effectName];
        return effect.IsExpired() ? 0f : effect.strength;
    }

    public bool HasEffect(StatusClass effectName)
    {
        return GetEffectStrength(effectName) > 0f;
    }

    public void RemoveEffect(StatusClass effectName)
    {
        if (activeEffects.ContainsKey(effectName))
        {
            activeEffects[effectName].Clear();
        }
    }

    // all effects that trigger per frame
    public void UpdateEffects(float deltaTime)
    {
        float burnDamage = GetEffectStrength(StatusClass.Burn);
        if (burnDamage > 0)
        {
            TakeDamage(burnDamage * deltaTime);
        }
        
        float healAmount = GetEffectStrength(StatusClass.Heal);
        if (healAmount > 0)
        {
            Heal(healAmount * deltaTime);
        }
    }

    #endregion

    #region Targeting

    public HashSet<string> GetTagList()
    {
        return tagList;
    }

    #endregion

    #region Debug

    public void PrintEffects()
    {
        string output = $"=== Effects on {gameObject.name} ===\n";
        foreach ((StatusClass name, StatusEffect effect) in activeEffects)
        {
            if (effect.strength > 0)
            {
                string duration = effect.duration < 0 ? "∞" : effect.GetRemainingDuration().ToString("F1") + "s";
                output += $"{name}: {effect.strength:F2} ({duration})\n";
            }
        }
        Debug.Log(output);
    }

    #endregion

    private class StatusEffect
    {
        public StatusClass name;
        public float strength = 0f;
        public float duration = 0f;  // -1 = permanent
        public float startTime = 0f;

        public bool IsExpired()
        {
            if (strength == 0) return true;
            if (duration < 0) return false; // permanent
            return Time.time > startTime + duration;
        }

        public float GetRemainingDuration()
        {
            if (duration < 0) return Mathf.Infinity;
            return Mathf.Max(0, (startTime + duration) - Time.time);
        }

        public void Clear()
        {
            strength = 0f;
            duration = 0f;
            startTime = 0f;
        }
    }

    public enum StatusClass
    {
        Heal,
        Slow, // additive
        Haste, // additive
        Burn,
        AttackBuff,
        AttackDebuff,
        Stun
    }
}