using System.Collections;
using UnityEngine;

public class EntityMaster : MonoBehaviour
{

    [Header("Entity Data")]
    public EntityData data;

    [Header("Entity Grid Position")]
    [SerializeField] private int gridX;
    [SerializeField] private int gridZ;
    [SerializeField] private float heightAboveTile = 1f;
    [SerializeField] private float moveSpeed = 3f;

    [Header("Death Settings")]
    [SerializeField] private float deathSinkSpeed = 5f;
    [SerializeField] private float deathFadeSpeed = 5f;

    [Header("Summon Settings")]
    [SerializeField] private float summonRiseSpeed = 5f;
    [SerializeField] private float summonFadeSpeed = 5f;

    private GridManager gridManager;
    private Tile currentTile;
    private bool isMoving = false;
    private bool isDead = false;
    private bool hasMoved = false;
    private Renderer[] renderers;
    private Material[] materials;

    public int GridX => gridX;
    public int GridZ => gridZ;
    public bool IsDead => isDead;
    public int MoveRange => data.moveRange;
    public bool HasMoved => hasMoved;
    public Faction Faction => data.faction;

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();

        if (gridManager == null)
        {
            Debug.LogError("[EntityMaster] No GridManager found in scene!");
            return;
        }

        renderers = GetComponentsInChildren<Renderer>();
        CacheMaterials();

        SnapToGridPosition(gridX, gridZ);
    }

    private void CacheMaterials()
    {
        if (renderers == null) return;

        materials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            materials[i] = renderers[i].material;
    }

    public void SnapToGridPosition(int x, int z)
    {
        if (gridManager == null) return;

        Tile tile = gridManager.GetTileAt(x, z);
        if (tile == null)
        {
            Debug.LogWarning($"[EntityMaster] Invalid grid position ({x},{z})!");
            return;
        }

        currentTile?.SetOccupyingEntity(null);

        currentTile = tile;
        currentTile.SetOccupyingEntity(this);

        gridX = x;
        gridZ = z;

        Vector3 tileCenter = tile.transform.position + Vector3.up * heightAboveTile;
        transform.position = tileCenter;
    }

    public IEnumerator MoveToGridPosition(int targetX, int targetZ)
    {
        if (isMoving || isDead) yield break;
        if (gridManager == null) yield break;

        bool isHorizontalFirst = Random.value > 0.5f;
        isMoving = true;

        if (isHorizontalFirst && targetX != gridX)
            yield return MoveStepAnim(targetX, gridZ);

        if (targetZ != gridZ)
            yield return MoveStepAnim(gridX, targetZ);

        if (!isHorizontalFirst && targetX != gridX)
            yield return MoveStepAnim(targetX, gridZ);

        isMoving = false;

        Debug.Log($"[EntityMaster] Finished moving to ({gridX},{gridZ})");
    }

    private IEnumerator MoveStepAnim(int targetX, int targetZ)
    {
        Tile tile = gridManager.GetTileAt(targetX, targetZ);
        if (tile == null) yield break;

        Vector3 startPos = transform.position;
        Vector3 targetPos = tile.transform.position + Vector3.up * heightAboveTile;

        float elapsed = 0f;
        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / moveSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }

        transform.position = targetPos;

        if (currentTile != null)
            currentTile.SetOccupyingEntity(null);

        currentTile = tile;
        currentTile.SetOccupyingEntity(this);

        gridX = targetX;
        gridZ = targetZ;

        hasMoved = true;
    }

    public IEnumerator DieAnim()
    {
        if (isDead) yield break;
        isDead = true;
        isMoving = false;

        Debug.Log($"[EntityMaster] Dying... at grid ({gridX},{gridZ})");

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos - Vector3.up * 2f;

        float sinkElapsed = 0f;
        float fadeElapsed = 0f;

        while (sinkElapsed < 1f)
        {
            sinkElapsed += Time.deltaTime * deathSinkSpeed;
            fadeElapsed += Time.deltaTime * deathFadeSpeed;

            transform.position = Vector3.Lerp(startPos, targetPos, sinkElapsed);

            if (materials != null)
            {
                foreach (var mat in materials)
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
        Destroy(gameObject);
        Debug.Log("[EntityMaster] Entity destroyed.");
    }

    public IEnumerator SummonAnim()
    {
        if (isDead) yield break;
        if (currentTile == null) yield break;

        Debug.Log($"[EntityMaster] Summoning at grid ({gridX},{gridZ})");

        Vector3 tileCenter = currentTile.transform.position + Vector3.up * heightAboveTile;
        Vector3 startPos = tileCenter - Vector3.up * 2f;
        Vector3 targetPos = tileCenter;

        transform.position = startPos;

        if (materials != null)
        {
            foreach (var mat in materials)
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

            if (materials != null)
            {
                foreach (var mat in materials)
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

    // private IEnumerator TestRoutine()
    // {
    //     // wait 3s -> move
    //     yield return new WaitForSeconds(3f);
    //     StartCoroutine(MoveToGridPosition(gridX + 2, gridZ + 1));

    //     // wait 7s -> die
    //     yield return new WaitForSeconds(7f);
    //     StartCoroutine(Die());
    // }
}
