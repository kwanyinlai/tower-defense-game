using UnityEngine;

[System.Serializable]
public class TroopStats
{
    public string troopName;
    public TroopFaction faction;

    [Header("Movement")]
    public float maxSpeed = 3.5f;
    public float acceleration = 5f;

    [Header("Combat")]
    public float aggroRange = 10.0f;
    public float attackRange = 2.0f;
}

public enum TroopFaction // we can add more factions later
{
    Player,
    Enemy,
    Neutral
}

public enum TroopState
{
    Idle,
    MovingToTarget,
    InCombat,
    Retreating,
    Dead
}