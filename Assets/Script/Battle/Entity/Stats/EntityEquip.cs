public class EntityEquip
{
    private EntityMaster _e;
    private int attackBuff, defenseBuff, critBuff, healBuff;


    public EntityEquip(EntityMaster e)
    {
        _e = e;
    }

    public void AttackBuff(int buff)
    {
        _e.data.attack += buff;
    }

    public void DefenseBuff(int buff)
    {
        _e.data.defense += buff;
    }

    public void CritBuff(int buff)
    {
        _e.data.critChance += buff;
    }

    public void Heal(int buff)
    {
        _e.health.Heal(buff);
    }
}