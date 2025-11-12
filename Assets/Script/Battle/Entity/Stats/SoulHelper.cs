using UnityEngine;

public class SoulHelper
{
    private EntityMaster _e;
    public SoulHelper(EntityMaster e)
    {
        _e = e;
        SetSoulCount(_e.data.maxSoul);
    }
    public int GetSoulCount()
    {
        return _e.data.currentSoul;
    }

    public void SetSoulCount(int value)
    {
        if (!_e.data.canSummon) return;

        _e.data.currentSoul = value;
        SoulCountManager.Instance.SetSoul(value, _e.data.maxSoul);
    }

    public void IncreaseSoul(int amount)
    {
        int x = GetSoulCount() + amount;
        if (x > _e.data.maxSoul)
        {
            SetSoulCount(_e.data.maxSoul);
            return;
        }
        SetSoulCount(GetSoulCount() + amount);

    }

    public void ReduceSoul(int amount)
    {
        SetSoulCount(GetSoulCount() - amount);
    }
}
