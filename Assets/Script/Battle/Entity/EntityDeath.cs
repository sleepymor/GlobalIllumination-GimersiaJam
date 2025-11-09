using UnityEngine;
using System.Collections;

public class EntityDeath : MonoBehaviour
{

    [Header("Death Settings")]
    [SerializeField] private float deathSinkSpeed = 5f;
    [SerializeField] private float deathFadeSpeed = 5f;

    private bool isDead = false;
    public bool IsDead => isDead;

    private EntityMaster _e;

    public void Initialize(EntityMaster e)
    {
        _e = e;
    }

    public IEnumerator DieAnim()
    {
        if (isDead) yield break;
        isDead = true;

        Debug.Log($"[EntityMaster] Dying... at grid ({_e.GridX},{_e.GridZ})");

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos - Vector3.up * 2f;

        float sinkElapsed = 0f;
        float fadeElapsed = 0f;

        while (sinkElapsed < 1f)
        {
            sinkElapsed += Time.deltaTime * deathSinkSpeed;
            fadeElapsed += Time.deltaTime * deathFadeSpeed;

            transform.position = Vector3.Lerp(startPos, targetPos, sinkElapsed);

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

        if (_e.Faction == Faction.PLAYER)
        {
            PlayerManager.Instance?.RemoveEntity(_e);
            PlayerManager.Instance?.ClearAllAttackAreas();
        }
        else if (_e.Faction == Faction.ENEMY)
        {
            EnemyManager.Instance?.RemoveEntity(_e);
            PlayerManager.Instance?.ClearAllAttackAreas();
        }

        Destroy(gameObject);
        Debug.Log("[EntityMaster] Entity destroyed.");
    }


}