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

    // --- Setters untuk status efek ---

    public void SetStun(int stunDmg, int stunDur)
    {
        _e.health.TakeDamage(stunDmg);
        this.stunDur = stunDur;

        // tampilkan status di UI
        _e.charStatHandler.ShowFreeze();
        _e.charStatHandler.SetFreeze(stunDur);
    }

    public void SetPoison(int poisonDmg, int poisonDur)
    {
        this.poisonDur = poisonDur;
        this.poisonDmg = poisonDmg;

        // tampilkan status di UI
        _e.charStatHandler.ShowPoison();
        _e.charStatHandler.SetPoison(poisonDur);
    }

    public void SetRegen(int regenAmount, int regenDur)
    {
        this.regenDur = regenDur;
        this.regenAmount = regenAmount;
        // abaikan untuk UI
    }

    public void SetDeath()
    {
        isDead = true;
    }

    // --- Dipanggil di awal giliran entity ---
    public void StartTurnEffect()
    {
        // efek STUN
        if (stunDur > 0)
        {
            stunDur--;
            StunEffect();

            // update atau sembunyikan dari UI
            if (stunDur > 0)
                _e.charStatHandler.SetFreeze(stunDur);
            else
                _e.charStatHandler.HideFreeze();
        }

        // efek POISON
        if (poisonDur > 0)
        {
            poisonDur--;
            PoisonEffect();

            if (poisonDur > 0)
                _e.charStatHandler.SetPoison(poisonDur);
            else
                _e.charStatHandler.HidePoison();
        }
        else poisonDmg = 0;

        // efek REGEN (tidak di-UI-kan)
        if (regenDur > 0)
        {
            regenDur--;
            RegenEffect();
        }
        else regenAmount = 0;
    }

    // --- Efek individual ---
    void StunEffect()
    {
        _e.move.SetHadMove(true);
    }

    void PoisonEffect()
    {
        _e.health.TakeDamage(poisonDmg);
        _e.charStatHandler.ShowDamage(poisonDmg);
    }

    void RegenEffect()
    {
        _e.health.Heal(regenAmount);
    }
}
