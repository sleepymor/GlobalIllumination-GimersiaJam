using UnityEngine;
using System.Collections;

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
        if (target == null || target.status.IsDead) return false;
        if (target.Faction == _e.Faction) return false;

        int distance = Mathf.Abs(target.GridX - _e.GridX) + Mathf.Abs(target.GridZ - _e.GridZ);
        return distance <= _e.data.attackRange;
    }
    public void Attack(EntityMaster target)
    {
        if (!CanAttack(target)) return;
        StartCoroutine(AttackRoutine(target));
    }

    private IEnumerator AttackRoutine(EntityMaster target)
    {
        _e._animator.Play("Attack");

        // Wait for attack to reach the hit frame
        yield return new WaitForSeconds(1f); // adjust to your animation

        _e._animator.Play("Idle");

        target.health.TakeDamage(_e.data.attack, _e.data.critChance);
        isAlreadyAttacking = true;
    }

}