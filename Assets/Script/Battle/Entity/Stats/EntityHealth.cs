/*
 * EntityHealth.cs
 * -----------------
 * Manages the health system for an EntityMaster in a turn-based strategy game.
 *
 * Responsibilities:
 * - Track and modify the entity’s current HP.
 * - Apply healing and damage, including logging damage events.
 * - Handle death when HP drops to zero by triggering the EntityMaster's death sequence.
 * - Provide a method to reset the entity to its maximum HP.
 *
 * Usage:
 * - Attach this script to the same GameObject as EntityMaster.
 * - Initialize with EntityHealth.Initialize(entityMaster) after Awake.
 * - Use Heal(int hp) to restore health.
 * - Use TakeDamage(int amount, int critChance = 0) to reduce health.
 * - Use SetMaxHP() to fully restore health.
 *
 * Notes:
 * - Death is handled via the EntityMaster's death component, which plays death animations and removes the entity from the scene.
 * - Crit chance parameter is included for future extensions (e.g., critical hits).
 */

using UnityEngine;

public class EntityHealth
{
    private EntityMaster _e;
    public EntityHealth(EntityMaster e)
    {
        _e = e;
    }
    public void Heal(int hp)
    {
        _e.data.currentHP += hp;
    }

    public void SetMaxHP()
    {
        _e.data.currentHP = _e.data.maxHP;
    }

    public void TakeDamage(int amount, int critChance = 0)
    {
        _e.data.currentHP -= amount;

        if (_e.data.currentHP <= 0)
        {
            Debug.Log($"[{_e.name}] has died!");

            _e.StartCoroutine(_e.anim.DieAnim());

        }
    }
}

