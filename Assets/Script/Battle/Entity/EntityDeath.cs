/*
 * EntityDeath.cs
 * -----------------
 * Handles the death behavior of an EntityMaster in a turn-based strategy game.
 *
 * Responsibilities:
 * - Play death animations including sinking into the ground and fading out.
 * - Mark the entity as dead in the EntityState component.
 * - Remove the entity from its respective manager (PlayerManager or EnemyManager).
 * - Clean up attack/move highlights after death.
 * - Destroy the entity GameObject after the death sequence is complete.
 *
 * Usage:
 * - Attach this script to the same GameObject as EntityMaster.
 * - Initialize with EntityDeath.Initialize(entityMaster) after Awake.
 * - Trigger the death sequence by calling StartCoroutine(entityDeath.DieAnim()).
 *
 * Notes:
 * - Death animation speed is controlled by `deathSinkSpeed` and `deathFadeSpeed`.
 * - The entity’s materials are modified for fade effects, so ensure all renderers are cached in EntityMaster.
 * - This class interacts with PlayerManager and EnemyManager to maintain proper game state.
 */

using UnityEngine;
using System.Collections;

public class EntityDeath : MonoBehaviour
{

    [Header("Death Settings")]
    [SerializeField] private float deathSinkSpeed = 5f;
    [SerializeField] private float deathFadeSpeed = 5f;

    private EntityMaster _e;

    public void Initialize(EntityMaster e)
    {
        _e = e;
    }

    public IEnumerator DieAnim()
    {
        if (_e.status.IsDead) yield break;
        _e.status.SetDeath();

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