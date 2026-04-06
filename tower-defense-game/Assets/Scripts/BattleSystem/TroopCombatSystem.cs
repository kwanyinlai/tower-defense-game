using UnityEngine;
using System.Collections.Generic;

public class TroopCombatSystem : CombatSystem
{
    [Header("Combat Attributes")]
    [SerializeField] private int atk = 10;
    public int Attack 
    { 
        get => atk; 
        set => atk = value; 
    }

    [SerializeField] private float attackRange = 5.0f;
    public float AttackRange 
    { 
        get => attackRange; 
        set => attackRange = value; 
    }

    [SerializeField] private float atkCooldown = 1.5f;
    private float atkTimer = 0f;

    protected override void Start()
    {
        base.Start();
    }

    public void UpdateCombat(float deltaTime)
    {
        // attack timer
        if (atkTimer > 0f)
        {
            atkTimer -= deltaTime;
        }

        // status effects
        UpdateEffects(deltaTime);
    }

    public bool CanAttack() => atkTimer <= 0f && !HasEffect(StatusClass.Stun);

    public void ResetAttackCooldown() => atkTimer = atkCooldown;

    public int GetModifiedAttack()
    {
        float buffed = Attack * (1f + GetEffectStrength(StatusClass.AttackBuff));
        float weakened = buffed * (1f - GetEffectStrength(StatusClass.AttackDebuff));
        return Mathf.RoundToInt(weakened);
    }

    public float GetModifiedSpeed(float baseSpeed)
    {
        if (HasEffect(StatusClass.Stun)) return 0f;
        
        float slow = GetEffectStrength(StatusClass.Slow);
        float haste = GetEffectStrength(StatusClass.Haste);
        return baseSpeed * (1f - slow + haste);
    }

    protected override void Die()
    {
        TroopAI troopAI = GetComponent<TroopAI>();
        if (troopAI != null && FactionManager.Instance != null)
        {
            FactionManager.Instance.UnregisterTroop(transform, troopAI.GetFaction());
        }

        foreach (GameObject player in PlayerManager.players)
        {
            List<GameObject> selectedTroops = player.GetComponent<TroopManagement>().SelectedTroops;
            if (selectedTroops.Contains(gameObject))
            {
                selectedTroops.Remove(gameObject);
                break;
            }
        }

        base.Die();
    }
}