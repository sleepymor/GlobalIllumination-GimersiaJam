public class EntityEquip
{
    private EntityMaster _e;

    // simpan nilai dasar
    private int baseAttack;
    private int baseDefense;
    private int baseCrit;

    public EntityEquip(EntityMaster e)
    {
        _e = e;

        baseAttack = _e.data.attack;
        baseDefense = _e.data.defense;
        baseCrit = _e.data.critDmg;
    }

    // --- Attack Buff ---
    public void AttackBuff(int buff)
    {
        _e.data.attack += buff;

        if (_e.data.attack != baseAttack)
        {
            _e.charStatHandler.SetAttack(_e.data.attack);
            _e.charStatHandler.ShowAttack();
        }
        else
        {
            _e.charStatHandler.HideAttack();
        }
    }

    // --- Defense Buff ---
    public void DefenseBuff(int buff)
    {
        _e.data.defense += buff;

        if (_e.data.defense != baseDefense)
        {
            _e.charStatHandler.SetDefense(_e.data.defense);
            _e.charStatHandler.ShowDefense();
        }
        else
        {
            _e.charStatHandler.HideDefense();
        }
    }

    // --- Crit Buff ---
    public void CritBuff(int buff)
    {
        _e.data.critChance += buff;
        _e.data.critDmg += buff;

        if (_e.data.critChance != baseCrit)
        {
            _e.charStatHandler.SetCrit(_e.data.critChance);
            _e.charStatHandler.ShowCrit();
        }
        else
        {
            _e.charStatHandler.HideCrit();
        }
    }

    // --- Heal ---
    public void Heal(int amount)
    {
        _e.health.Heal(amount);
        _e.charStatHandler.SetHealth(_e.data.currentHP, _e.data.health);
        _e.charStatHandler.ShowHealth();
    }

}
