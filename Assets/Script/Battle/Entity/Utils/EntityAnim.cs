using UnityEngine;
using System.Collections;

public class EntityAnim
{
    private EntityMaster _e;

    public float moveSpeed = 50f;
    private float summonRiseSpeed = 5f;
    private float summonFadeSpeed = 5f;

    private float deathSinkSpeed = 5f;
    private float deathFadeSpeed = 5f;

    private Animator _animator;

    public EntityAnim(EntityMaster e, Animator animator)
    {
        _e = e;
        _animator = animator;
    }


    public IEnumerator SummonAnim()
    {
        if (_e.status.IsDead) yield break;
        if (_e.currentTile == null) yield break;

        Debug.Log($"[EntityMaster] Summoning at grid ({_e.pos.GridX},{_e.pos.GridZ})");

        Vector3 tileCenter = _e.currentTile.transform.position + Vector3.up * _e.heightAboveTile;
        Vector3 startPos = tileCenter - Vector3.up * 2f;
        Vector3 targetPos = tileCenter;

        _e.transform.position = startPos;

        if (_e.materials != null)
        {
            foreach (var mat in _e.materials)
            {
                if (mat.HasProperty("_Color"))
                {
                    Color c = mat.color;
                    c = Color.black;
                    mat.color = c * 0.1f;
                }
            }
        }

        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * summonRiseSpeed;

            _e.transform.position = Vector3.Lerp(startPos, targetPos, elapsed);

            if (_e.materials != null)
            {
                foreach (var mat in _e.materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        Color c = mat.color;
                        c = Color.Lerp(Color.black, Color.white, elapsed);
                        mat.color = c;
                    }
                }
            }

            yield return null;
        }

        _e.transform.position = targetPos;
        Debug.Log("[EntityMaster] Summon complete!");
    }


    public IEnumerator DieAnim()
    {
        if (_e.status.IsDead) yield break;
        _e.status.SetDeath();

        Debug.Log($"[EntityMaster] Dying... at grid ({_e.pos.GridX},{_e.pos.GridZ})");

        Vector3 startPos = _e.transform.position;
        Vector3 targetPos = startPos - Vector3.up * 2f;

        float sinkElapsed = 0f;
        float fadeElapsed = 0f;

        while (sinkElapsed < 1f)
        {
            sinkElapsed += Time.deltaTime * deathSinkSpeed;
            fadeElapsed += Time.deltaTime * deathFadeSpeed;

            _e.transform.position = Vector3.Lerp(startPos, targetPos, sinkElapsed);

            if (_e.materials != null)
            {
                foreach (var mat in _e.materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        Color c = mat.color;
                        c = Color.Lerp(c, Color.black, fadeElapsed);
                        mat.color = c * Mathf.Clamp01(1f - fadeElapsed * 0.3f);
                    }
                }
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        if (_e.data.faction == Faction.PLAYER)
        {
            PlayerManager.Instance?.RemoveEntity(_e);
            PlayerManager.Instance?.ClearAllAttackAreas();
        }
        else if (_e.data.faction == Faction.ENEMY)
        {
            EnemyManager.Instance?.RemoveEntity(_e);
            PlayerManager.Instance?.ClearAllAttackAreas();
        }

        UnityEngine.Object.Destroy(_e.gameObject);
    }

    public void AttackAnim()
    {
        _animator.Play("Attack");
    }

    public void IdleAnim()
    {
        _animator.Play("Idle");
    }
}