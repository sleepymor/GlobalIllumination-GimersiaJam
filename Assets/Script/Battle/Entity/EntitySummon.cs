using UnityEngine;
using System.Collections;

public class EntitySummon : MonoBehaviour
{

    [Header("Summon Settings")]
    [SerializeField] private float summonRiseSpeed = 5f;
    [SerializeField] private float summonFadeSpeed = 5f;

    private EntityMaster _e;

    public void Initialize(EntityMaster e)
    {
        _e = e;
        _e.attackManager.SetHadAttacking(true);
    }

    public IEnumerator Summon()
    {
        if (_e.deathManager.IsDead) yield break;
        if (_e.currentTile == null) yield break;

        Debug.Log($"[EntityMaster] Summoning at grid ({_e.GridX},{_e.GridZ})");

        Vector3 tileCenter = _e.currentTile.transform.position + Vector3.up * _e.heightAboveTile;
        Vector3 startPos = tileCenter - Vector3.up * 2f;
        Vector3 targetPos = tileCenter;

        transform.position = startPos;

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

            transform.position = Vector3.Lerp(startPos, targetPos, elapsed);

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

        transform.position = targetPos;
        Debug.Log("[EntityMaster] Summon complete!");
    }
}