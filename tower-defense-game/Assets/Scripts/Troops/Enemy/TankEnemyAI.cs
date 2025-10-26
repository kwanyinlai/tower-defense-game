using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;



public class TankEnemyAI : IAttackBehaviour
{
    public float slowEffectDecimal = 0.10f;

    protected override void ApplyBuffOnStart()
    {
        selfCombatSystem.ApplyEffect("slow", slowEffectDecimal, -1);
    }

}
