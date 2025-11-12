using UnityEngine;
using System.Collections;

public class EntityAttack
{
    private EntityMaster _e;
    private bool isAlreadyAttacking = false;
    public bool IsAlreadyAttacking => isAlreadyAttacking;

    [HideInInspector] public int AttackRange;
    private SpriteRenderer spriteRenderer;

    public EntityAttack(EntityMaster e)
    {
        _e = e;
        AttackRange = _e.data.attackRange;

        // cari SpriteRenderer dari entity atau child-nya
        spriteRenderer = _e.GetComponentInChildren<SpriteRenderer>();
    }

    public void SetHadAttacking(bool hadAttacking)
    {
        this.isAlreadyAttacking = hadAttacking;
        UpdateSpriteColor();
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
        _e.StartCoroutine(AttackRoutine(target)); // 👈 ubah ke _e.StartCoroutine (bukan target)
    }

    private IEnumerator AttackRoutine(EntityMaster target)
    {
        // Saat menyerang, ubah ke warna normal (putih)
        SetSpriteColor(Color.white);

        _e.anim.AttackAnim();
        PlayerManager.Instance.ClearAllMoveAndAttackAreas();

        yield return new WaitForSeconds(1f);

        _e.anim.IdleAnim();

        int scaledDamage = _e.data.attack * _e.data.currentHP / _e.data.health;
        target.health.TakeDamage(scaledDamage, _e.data.critDmg, _e.data.critChance);

        isAlreadyAttacking = true;
        UpdateSpriteColor(); // 👈 ubah warna ke abu-abu setelah menyerang

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

        int scaledDamage = _e.data.attack * _e.data.currentHP / _e.data.health;
        target.health.TakeDamage(scaledDamage, _e.data.critDmg, _e.data.critChance);
    }

    private void UpdateSpriteColor()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.color = isAlreadyAttacking ? Color.grey : Color.white;
    }

    private void SetSpriteColor(Color color)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = color;
    }

    public void BackShot()
    {
        // Cumi hitam pak kris
    }
}
