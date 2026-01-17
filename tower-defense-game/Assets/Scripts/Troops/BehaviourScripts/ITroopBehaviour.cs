using UnityEngine;
using System.Collections.Generic;

public abstract class ITroopBehaviour : ScriptableObject
{
    [Header("Basic Properties")]
    public string behaviourName;
    public float cooldown = 0f;
    
    [Header("Targeting")]
    public TargetType targetType = TargetType.Enemy;
    public bool requiresLineOfSight = true;
    
    /// <summary>
    /// Execute the behavior and return True if success
    /// </summary>
    public abstract bool Execute(TroopCombatSystem self, CombatSystem target);
    
    /// <summary>
    /// Called on unit spawn
    /// </summary>
    public virtual void OnUnitSpawned(TroopCombatSystem self) { }
    
    /// <summary>
    /// Check if this behavior can be used right now
    /// </summary>
    public virtual bool CanExecute(TroopCombatSystem self, CombatSystem target)
    {
        return target != null;
    }
}

public enum TargetType
{
    Enemy,
    Ally,
    Self,
    Ground
}