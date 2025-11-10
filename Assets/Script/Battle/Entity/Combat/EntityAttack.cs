/*
 * EntityAttack.cs
 * -----------------
 * Handles attacking behavior for an EntityMaster in a turn-based strategy game.
 *
 * Responsibilities:
 * - Determine if a target is within attack range and valid to attack.
 * - Trigger attack animations and apply damage to the target.
 * - Track whether the entity has already attacked this turn.
 * - Support initialization from the EntityMaster for data and animator references.
 *
 * Usage:
 * - Attach this script to the same GameObject as EntityMaster.
 * - Initialize with EntityAttack.Initialize(entityMaster) after Awake.
 * - Call Attack(target) to perform an attack on a valid target.
 * - CanAttack(target) can be used to check if an attack is possible without performing it.
 *
 * Notes:
 * - Uses a coroutine to handle attack timing and animation playback.
 * - Animation names ("Attack", "Idle") should match the Animator controller.
 * - Interacts with the EntityHealth component of the target to apply damage.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class EntityAttack
{
    private EntityMaster _e;
    private bool isAlreadyAttacking = false;

    public bool IsAlreadyAttacking => isAlreadyAttacking;
    [HideInInspector] public int AttackRange;

    public EntityAttack(EntityMaster e)
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
        if (target.data.faction == _e.data.faction) return false;

        int distance = Mathf.Abs(target.pos.GridX - _e.pos.GridX) + Mathf.Abs(target.pos.GridZ - _e.pos.GridZ);
        return distance <= _e.data.attackRange;
    }
    public void Attack(EntityMaster target)
    {
        if (!CanAttack(target)) return;
        target.StartCoroutine(AttackRoutine(target));
    }

    private IEnumerator AttackRoutine(EntityMaster target)
    {
        _e.anim.AttackAnim();

        yield return new WaitForSeconds(1f);

        _e.anim.IdleAnim();

        int scaledDamage = _e.data.attack * _e.data.currentHP / _e.data.maxHP;
        target.health.TakeDamage(scaledDamage, _e.data.critChance);
        isAlreadyAttacking = true;

        int distance = Mathf.Abs(target.pos.GridX - _e.pos.GridX) + Mathf.Abs(target.pos.GridZ - _e.pos.GridZ);
        if (!target.status.IsDead && distance == 1)
        {
            yield return new WaitForSeconds(0.3f);
            target.attack.CounterAttack(_e);
        }

    }

    public void CounterAttack(EntityMaster attacker)
    {
        attacker.StartCoroutine(CounterAttackRoutine(attacker));
    }

    private IEnumerator CounterAttackRoutine(EntityMaster target)
    {
        _e.anim.AttackAnim();

        yield return new WaitForSeconds(1f);

        _e.anim.IdleAnim();

        int scaledDamage = _e.data.attack * _e.data.currentHP / _e.data.maxHP;
        target.health.TakeDamage(scaledDamage, _e.data.critChance);
    }


    public void BackShot()
    {
        // Cumi hitam pak kris
    }
}