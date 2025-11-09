using UnityEngine;

public class EntityState : MonoBehaviour
{
    private int stunDur = 0;
    private int poisonDur = 0;
    private int regenDur = 0;
    private int poisonDmg = 0;
    private int regenAmount = 0;

    private EntityMaster _e;
    public void Initialize(EntityMaster e)
    {
        _e = e;
    }
    public void SetStun(int stunDur)
    {
        this.stunDur = stunDur;
    }

    public void SetPoison(int poisonDur, int poisonDmg)
    {
        this.poisonDur = poisonDur;
        this.poisonDmg = poisonDmg;
    }

    public void SetRegen(int regenDur, int regenAmount)
    {
        this.regenDur = regenDur;
        this.regenAmount = regenAmount;
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
        _e.movementManager.SetHadMove(true);
    }

    void PoisonEffect()
    {
        _e.healthManager.TakeDamage(poisonDmg);
    }
    
    void RegenEffect()
    {
        _e.healthManager.Heal(regenAmount);
    }
}