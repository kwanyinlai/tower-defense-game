using UnityEngine;


/// Adapter layer for CombatSystem
[RequireComponent(typeof(TroopCombatSystem))]
public class TroopCombat : MonoBehaviour
{
    private TroopCombatSystem combatSystem;
    private TroopStats stats;

    [Header("Attack Behavior")]
    [SerializeField] private ITroopBehaviour troopBehaviour;

    private CombatSystem currentTarget;

    #region Initialization

    public void Initialize(TroopStats troopStats)
    {
        stats = troopStats;
        combatSystem = GetComponent<TroopCombatSystem>();

        if (troopBehaviour == null)
        {
            Debug.LogError($"{name} has no primary TroopBehaviour assigned.", this);
            return;
        }

        troopBehaviour.OnUnitSpawned(combatSystem);
    }

    #endregion

    #region Combat State

    public void EnterCombat(CombatSystem target)
    {
        currentTarget = target;
    }

    public void ExitCombat()
    {
        currentTarget = null;
    }

    public bool IsInCombat => currentTarget != null && currentTarget.IsAlive();

    public CombatSystem CurrentTarget => currentTarget;

    #endregion

    #region Attack Logic

    public void TryAttack(CombatSystem target)
    {
        if (!CanAttack(target))
            return;

        FaceTarget(target.transform);
        ExecuteAttack(target);
        combatSystem.ResetAttackCooldown();
    }

    private bool CanAttack(CombatSystem target)
    {
        if (combatSystem == null || target == null)
            return false;

        if (!combatSystem.CanAttack())
            return false;

        if (!target.IsAlive())
            return false;

        return true;
    }

    private void ExecuteAttack(CombatSystem target)
    {
        if (troopBehaviour == null)
        {
            Debug.LogWarning($"{name} attempted to attack without a behaviour.", this);
            return;
        }

        troopBehaviour.Execute(combatSystem, target);
    }

    #endregion

    #region Facing

    private void FaceTarget(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = rotation;
    }

    #endregion

    #region Update

    public void UpdateCombat(float deltaTime)
    {
        combatSystem?.UpdateCombat(deltaTime);
    }

    #endregion

    #region Getters

    public bool IsAlive => combatSystem != null && combatSystem.IsAlive();

    public float HealthPercent =>
        combatSystem != null ? combatSystem.GetHealthPercent() : 0f;

    public float AttackRange =>
        combatSystem != null ? combatSystem.AttackRange : stats.attackRange;

    public CombatSystem CombatSystem => combatSystem;

    public TroopStats Stats => stats;

    #endregion
}
