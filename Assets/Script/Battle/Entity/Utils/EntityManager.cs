public class EntityManager
{
    public void SetManager(EntityMaster _e)
    {
        if (_e.data.faction == Faction.PLAYER) PlayerManager.Instance.AddEntity(_e);
        if (_e.data.faction == Faction.ENEMY) EnemyManager.Instance.AddEntity(_e);
    }
}