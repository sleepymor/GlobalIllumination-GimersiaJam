using UnityEngine;

public class EntityHealth : MonoBehaviour
{
    private EntityMaster _e;
    public void Initialize(EntityMaster e)
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

    public void TakeDamage(int amount)
    {
        _e.data.currentHP -= amount;
        Debug.Log($"[EntityMaster] {_e.data.entityName} took {amount} damage! HP left: {_e.data.currentHP}");

        if (_e.data.currentHP <= 0)
        {
            StartCoroutine(_e.deathManager.DieAnim());
        }
    }
}

