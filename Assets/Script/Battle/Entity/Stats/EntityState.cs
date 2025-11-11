/*
 * EntityState.cs
 * -----------------
 * This script manages the status effects and general state of a battle entity (player or enemy) 
 * within the turn-based battle system. It acts as a modular component that tracks ongoing effects 
 * like stun, poison, and regeneration, as well as the entity's death state.
 *
 * Responsibilities:
 * - Track whether the entity is dead.
 * - Track and manage durations for status effects:
 *   • Stun: prevents movement for a set number of turns.
 *   • Poison: deals damage over multiple turns.
 *   • Regeneration: heals the entity over multiple turns.
 * - Apply the effects automatically at the start of each turn through StartTurnEffect().
 * - Communicate with other entity components (EntityMovement, EntityHealth) to enforce effects.
 *
 * Usage:
 * - Attach this script to any GameObject representing a battle entity.
 * - Initialize with the parent EntityMaster via Initialize(EntityMaster e).
 * - Use SetStun, SetPoison, and SetRegen to apply status effects.
 * - Call StartTurnEffect() at the beginning of the entity’s turn to automatically process active effects.
 * - Call SetDeath() to mark the entity as dead.
 *
 * Notes:
 * - Stun disables the entity's movement for the current turn.
 * - Poison damage and regeneration are applied each turn while their durations are active.
 * - Effects durations decrement automatically each turn.
 */

using UnityEngine;

public class EntityState
{
    private bool isDead = false;
    public bool IsDead => isDead;
    private int stunDur = 0;
    private int poisonDur = 0;
    private int regenDur = 0;
    private int poisonDmg = 0;
    private int regenAmount = 0;

    private EntityMaster _e;
    public EntityState(EntityMaster e)
    {
        _e = e;
    }
    public void SetStun(int stunDmg, int stunDur)
    {
        _e.health.TakeDamage(stunDmg);
        this.stunDur = stunDur;
    }

    public void SetPoison(int poisonDmg, int poisonDur)
    {
        this.poisonDur = poisonDur;
        this.poisonDmg = poisonDmg;
    }

    public void SetRegen(int regenAmount, int regenDur)
    {
        this.regenDur = regenDur;
        this.regenAmount = regenAmount;
    }

    public void SetDeath()
    {
        isDead = true;
    }

    public void StartTurnEffect()
    {
        if (stunDur > 0)
        {
            stunDur--;
            StunEffect();
        }

        if (poisonDur > 0)
        {
            poisonDur--;
            PoisonEffect();
        }
        else poisonDmg = 0;

        if (regenDur > 0)
        {
            regenDur--;
            RegenEffect();
        }
        else regenAmount = 0;
    }

    void StunEffect()
    {
        _e.move.SetHadMove(true);
    }

    void PoisonEffect()
    {
        _e.health.TakeDamage(poisonDmg);
    }

    void RegenEffect()
    {
        _e.health.Heal(regenAmount);
    }
}