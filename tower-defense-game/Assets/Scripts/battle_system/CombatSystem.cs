using UnityEngine;
using System.Collections.Generic;

public abstract class CombatSystem : MonoBehaviour
{
    /*
    Contains all types of effects
    If strength == 0, it means that there is no real effect being applied 
    If durationSec == -1, it means effect lasts forever
    */
    private Dictionary<string, List<(float strength, float startTime, float durationSec)>> appliedEffects = new Dictionary<string, List<(float strength, float startTime, float durationSec)>>();

    public float maxHealth = 100f;
    public float currentHealth;

    public float shield = 0f;

    public string[] viewableTagList; // A Viewable List To Easily Modify Tags

    protected HashSet<string> tagList;  // Uses a Set for Efficiency When Looking For Overlaps

    protected void Start()
    {
        InitializeEffects();
    }

    protected void Update()
    {
        List<(float strength, float startTime, float durationSec)> effect;
        float currentTime = Time.realtimeSinceStartup;

        foreach(string key in appliedEffects.Keys) 
        {
            effect = appliedEffects[key];
            if(effect.Count > 0 && (effect[0].startTime + effect[0].durationSec) < currentTime)
            {
                FilterEffect(key);
            }
        }
    }

    public float GetPercentageHP()
    {
        return currentHealth / maxHealth;
    }

    public virtual void TakeDamage(int damage)
    {
        shield -= damage;

        if (shield <= 0)
        {
            currentHealth += shield;
            shield = 0f;
        }


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

    public void AddHealth(float healthToAdd)
    {
        currentHealth = Mathf.Min(currentHealth + healthToAdd, maxHealth);
    }

    public void AddShield(float shieldToAdd)
    {
        shield += shieldToAdd;
    }

    public void InitializeEffects()
    {
        // please initialize all effects hear, this is just to prevent situations where duplicate effects are added like "health" and "Health" by accident
        appliedEffects.Add("heal", new List<(float strength, float startTime, float durationSec)>());
        appliedEffects.Add("slow", new List<(float strength, float startTime, float durationSec)>());
        appliedEffects.Add("haste", new List<(float strength, float startTime, float durationSec)>());
        appliedEffects.Add("burn", new List<(float strength, float startTime, float durationSec)>());

    }

    protected void FilterEffect(string effectName)
    {
        List<(float strength, float startTime, float durationSec)> effects = appliedEffects[effectName];

        if(effects.Count != 0)
        {
            float currentTime = Time.realtimeSinceStartup;

            // Find the index of the best effect based on strenght & that the time is still valid
            int bestEffectIndex = 0;
            for (int i = 0; i < effects.Count; i++)
            {
                if(effects[bestEffectIndex].strength < effects[i].strength && (effects[i].durationSec >= currentTime || effects[i].durationSec == -1))
                {
                    bestEffectIndex = i;
                }
            }

            // Places best effect in the front
            (float strength, float startTime, float durationSec) effect = effects[bestEffectIndex];
            effects.RemoveAt(bestEffectIndex);
            effects.Insert(0, effect);

            // Removes any outdated effects or effects shorter than the best effect
            for (int i = effects.Count - 1; i >= 1; i--)
            {
                if ((effects[i].startTime + effects[i].durationSec) < (effects[0].startTime + effects[0].durationSec) && effects[i].durationSec != -1)
                {
                    effects.RemoveAt(i);
                }
            }

            // Special case if all effects were outdated, including the "best effect"
            if((effects[0].startTime + effects[0].durationSec) < currentTime && effects[0].durationSec != -1)
            {
                effects.RemoveAt(0);   
            }
        }
    }

    // Prints the effect dictionary using Debug.Log
    public void PrintEffects()
    {
        string fullPrint = "";
        string printThis = "";
        foreach(var (key, value) in appliedEffects) 
        {
            printThis = key + " = ";
            foreach((float strength, float startTime, float durationSec) detail in value)
            {
                printThis += detail.strength + ", " + detail.startTime + ", " + detail.durationSec + "; ";
            }
            fullPrint += printThis + "\n\n";
        }
        Debug.Log(fullPrint);
    }

    public void ApplyEffect(string name, float strength, float durationSec)
    {
        if (!appliedEffects.ContainsKey(name))
        {
            Debug.LogError("Effect name has not been initialized! Make sure the effect is spelled correctly and that it is initialized in InitializeEffects()", this);
        }
        else
        {
            List<(float strength, float startTime, float durationSec)> effects = appliedEffects[name];
            float currentTime = Time.realtimeSinceStartup;
            // Checks if matches any pre-existing effect with similar strength
            for(int i = 0; i < effects.Count; i++)
            {
                if(effects[i].strength == strength)
                {
                    // Checks to see if the new effect will even extend the old one
                    if((effects[i].startTime + effects[i].durationSec < currentTime + durationSec || durationSec == -1) && effects[i].durationSec != -1)
                    {
                        effects[i] = (strength, currentTime, durationSec);
                        FilterEffect(name);
                    }
                    return;
                }
            }
            effects.Add((strength, currentTime, durationSec));
            FilterEffect(name);
        }
    }

    public float GetEffectStrength(string name)
    {
        if(appliedEffects[name].Count == 0)
        {
            return 0;
        }
        return appliedEffects[name][0].strength;
    }
}