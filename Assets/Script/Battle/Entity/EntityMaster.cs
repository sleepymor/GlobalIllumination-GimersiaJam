using System.Collections;
using System.Collections.Generic;
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
    private bool isAlreadyAttacking = false;
    private bool isDead = false;
    private bool hasMoved = false;
    private Renderer[] renderers;
    private Material[] materials;

    public int GridX => gridX;
    public int GridZ => gridZ;
    public bool IsDead => isDead;
    public int MoveRange => data.moveRange;
    public bool HasMoved => hasMoved;
    public bool IsAlreadyAttacking => isAlreadyAttacking;
    public int AttackRange => data.attackRange;
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
        data.setCurrentHP(data.maxHP);
    }

    private void SetManager()
    {
        if (data.faction == Faction.PLAYER) PlayerManager.Instance.AddEntity(this);
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

        List<Tile> path = FindPathTo(targetX, targetZ);
        if (path == null || path.Count == 0)
        {
            Debug.Log($"[EntityMaster] No valid path to ({targetX},{targetZ})");
            yield break;
        }

        isMoving = true;

        foreach (Tile stepTile in path)
        {
            if (stepTile == null) continue;
            yield return MoveStepAnim(stepTile);
        }

        isMoving = false;
        hasMoved = true;

        Debug.Log($"[EntityMaster] Finished moving to ({gridX},{gridZ})");
    }

    private IEnumerator MoveStepAnim(Tile targetTile)
    {
        if (targetTile == null) yield break;

        Vector3 startPos = transform.position;
        Vector3 targetPos = targetTile.transform.position + Vector3.up * heightAboveTile;

        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }

        transform.position = targetPos;

        // Update occupancy
        currentTile?.SetOccupyingEntity(null);
        currentTile = targetTile;
        currentTile.SetOccupyingEntity(this);

        gridX = currentTile.gridX;
        gridZ = currentTile.gridZ;
    }


    private List<Tile> FindPathTo(int targetX, int targetZ)
    {
        if (gridManager == null) return null;

        Tile start = gridManager.GetTileAt(gridX, gridZ);
        Tile goal = gridManager.GetTileAt(targetX, targetZ);
        if (start == null || goal == null)
        {
            Debug.LogWarning("[EntityMaster] Pathfinding failed: invalid start or goal tile!");
            return null;
        }

        // BFS with cost tracking
        Queue<(Tile tile, int costSoFar)> queue = new Queue<(Tile, int)>();
        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
        HashSet<Tile> visited = new HashSet<Tile>();

        queue.Enqueue((start, 0));
        visited.Add(start);

        while (queue.Count > 0)
        {
            var (current, costSoFar) = queue.Dequeue();
            if (current == goal)
                break;

            foreach (Tile neighbor in gridManager.GetNeighbors(current))
            {
                if (neighbor == null || visited.Contains(neighbor) || neighbor.isOccupied)
                    continue;

                int newCost = costSoFar + neighbor.moveCost;
                if (newCost > data.moveRange)
                    continue;

                visited.Add(neighbor);
                cameFrom[neighbor] = current;
                queue.Enqueue((neighbor, newCost));
            }
        }

        if (!cameFrom.ContainsKey(goal))
        {
            Debug.Log($"[EntityMaster] No path found to target ({targetX},{targetZ})");
            return null;
        }

        // Reconstruct path
        List<Tile> path = new List<Tile>();
        Tile step = goal;

        while (step != start)
        {
            path.Add(step);
            step = cameFrom[step];
        }

        path.Reverse();
        return path;
    }


    public void SetHadMove(bool hasMoved)
    {
        this.hasMoved = hasMoved;
    }

    public void SetHadAttacking(bool hadAttacking)
    {
        this.isAlreadyAttacking = hadAttacking;
    }

    public bool CanAttack(EntityMaster target)
    {
        if (target == null || target.IsDead) return false;
        if (target.Faction == this.Faction) return false;

        int distance = Mathf.Abs(target.GridX - GridX) + Mathf.Abs(target.GridZ - GridZ);
        return distance <= data.attackRange;
    }

    public void Attack(EntityMaster target)
    {
        if (!CanAttack(target)) return;

        Debug.Log($"[EntityMaster] {data.entityName} attacks {target.data.entityName} for {data.attack} damage!");

        target.TakeDamage(data.attack);
        isAlreadyAttacking = true;
    }

    public void TakeDamage(int amount)
    {
        data.currentHP -= amount;
        Debug.Log($"[EntityMaster] {data.entityName} took {amount} damage! HP left: {data.currentHP}");

        if (data.currentHP <= 0)
        {
            StartCoroutine(DieAnim());
        }
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

        // 🔴 Remove from team and clear attack areas
        if (Faction == Faction.PLAYER)
        {
            PlayerManager.Instance?.RemoveEntity(this);
            PlayerManager.Instance?.ClearAllAttackAreas();
        }
        else if (Faction == Faction.ENEMY)
        {
            EnemyManager.Instance?.RemoveEntity(this);
            PlayerManager.Instance?.ClearAllAttackAreas();
        }

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
