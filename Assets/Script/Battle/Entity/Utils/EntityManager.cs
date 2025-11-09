public class EntityManager
{

    EntityMaster _e;
    public EntityManager(EntityMaster e)
    {
        _e = e;
        SetManager();
    }

    private void SetManager()
    {
        if (_e.data.faction == Faction.PLAYER) PlayerManager.Instance.AddEntity(_e);
        if (_e.data.faction == Faction.ENEMY) EnemyManager.Instance.AddEntity(_e);
    }
}