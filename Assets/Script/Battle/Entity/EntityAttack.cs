using UnityEngine;

public class EntityAttack : MonoBehaviour
{
    private EntityMaster _e;
    private bool isAlreadyAttacking = false;

    public bool IsAlreadyAttacking => isAlreadyAttacking;
    public int AttackRange;

    public void Initialize(EntityMaster e)
    {
        _e = e;
        AttackRange = _e.data.attackRange;
    }
    public void SetHadAttacking(bool hadAttacking)
    {
        this.isAlreadyAttacking = hadAttacking;
    }

    public bool CanAttack(EntityMaster target)
    {
        if (target == null || target.deathManager.IsDead) return false;
        if (target.Faction == _e.Faction) return false;

        int distance = Mathf.Abs(target.GridX - _e.GridX) + Mathf.Abs(target.GridZ - _e.GridZ);
        return distance <= _e.data.attackRange;
    }

    public void Attack(EntityMaster target)
    {
        if (!CanAttack(target)) return;

        Debug.Log($"[EntityMaster] {_e.data.entityName} attacks {target.data.entityName} for {_e.data.attack} damage!");

        target.healthManager.TakeDamage(_e.data.attack);
        isAlreadyAttacking = true;
    }

}