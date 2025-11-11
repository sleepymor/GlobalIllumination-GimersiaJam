/*
 * EnemyManager.cs
 * ------------------------
 * Singleton class that manages all enemy entities in a turn-based strategy game.
 * Inherits from BattleEntityManager to leverage generic team and tile management.
 *
 * Responsibilities:
 * - Maintain a list of all active enemy units in the scene.
 * - Execute the enemy turn logic: move towards the nearest player and attack if in range.
 * - Handle turn transitions, ending the enemy turn and passing control to the player.
 *
 * Core Features:
 * - RunEnemyTurn(): Coroutine that iterates over all enemies and performs movement and attacks.
 * - IsEnemyValid(): Checks if an enemy can act (not dead, has not moved).
 * - FindNearestTarget(): Finds the closest player-controlled entity for targeting.
 * - FindClosestTileTowardsTarget(): Calculates which tile the enemy can move to that brings it closest to its target.
 * - TryAttackTarget(): Executes an attack if the target is within range.
 * - EndTurn(): Ends the enemy's turn and switches to the player’s turn.
 *
 * Notes:
 * - Designed to work with GridManager, PlayerManager, and EntityMaster classes.
 * - Automatically skips dead units and units that have already moved.
 * - Uses BFS-based movement calculations to find reachable tiles.
 *
 * Usage:
 * - Attach this script to a singleton EnemyManager GameObject in the scene.
 * - Call RunEnemyTurn() when the enemy phase begins.
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : BattleEntityManager
{
    public static EnemyManager Instance { get; private set; }

    private AIAttack ai_attack;
    private AIEquip ai_equip;
    private AISpell ai_spell;
    private AISummon ai_summon;
    public GridManager grid;

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();

        ai_attack = new AIAttack(this);
        ai_equip = new AIEquip(this);
        ai_spell = new AISpell(this);
        ai_summon = new AISummon(this);

    }

    public void Start()
    {
        EnemyDeckManager.Instance.InitDeck();
        EnemyDeckManager.Instance.DrawMultiple(5); // Enemy draws initial hand
    }

    protected override Faction GetFactionType() => Faction.ENEMY;

    public IEnumerator RunEnemyTurn()
    {
        Debug.Log("<color=orange>[EnemyManager]</color> Enemy turn started...");
        EnemyDeckManager.Instance.DrawCard();

        RefreshTeam();

        // 🧠 Coba summon dan equip sebelum mulai aksi unit
        ai_summon.TryAutoSummon();
        yield return new WaitForSeconds(0.5f);

        ai_equip.TryAutoEquip();
        yield return new WaitForSeconds(0.5f);

        // 🧙‍♂️ Musuh coba cast spell
        ai_spell.TryAutoSpell();
        yield return new WaitForSeconds(0.5f);


        if (TeamList.Count == 0)
        {
            Debug.LogWarning("[EnemyManager] No enemies to act. Ending turn early.");
            TurnManager.PlayerTurn();
            yield break;
        }

        foreach (var enemy in TeamList)
        {
            if (!IsEnemyValid(enemy)) continue;

            EntityMaster target = ai_spell.FindNearestTarget(enemy, PlayerManager.Instance.TeamList);
            if (target == null) continue;

            float distToTarget = Vector3.Distance(enemy.transform.position, target.transform.position);
            int attackRange = enemy.data.attackRange;

            if (distToTarget > attackRange)
            {
                GridManager grid = FindObjectOfType<GridManager>();

                Tile moveTile = ai_attack.FindClosestTileTowardsTarget(enemy, target, grid);
                if (moveTile != null)
                {
                    yield return enemy.StartCoroutine(
                        enemy.move.MoveToGridPosition(moveTile.gridX, moveTile.gridZ)
                    );
                }
            }

            yield return new WaitForSeconds(0.2f);
            ai_attack.TryAttackTarget(enemy, target);
            yield return new WaitForSeconds(0.2f);
        }

        Debug.Log("<color=orange>[EnemyManager]</color> Enemy turn ended.");
        TurnManager.PlayerTurn();
    }

    private bool IsEnemyValid(EntityMaster enemy)
    {
        if (enemy == null)
        {
            Debug.LogWarning("[EnemyManager] Null enemy found — skipping.");
            return false;
        }

        if (enemy.status.IsDead)
        {
            Debug.Log($"[EnemyManager] {enemy.name} is dead. Skipping.");
            return false;
        }

        if (enemy.move.HasMoved)
        {
            Debug.Log($"[EnemyManager] {enemy.name} has already moved. Skipping.");
            return false;
        }

        return true;
    }

    public override void EndTurn()
    {
        if (TurnManager.GetCurrentTurn() != Faction.ENEMY)
        {
            Debug.LogWarning("[EnemyManager] Tried to end turn, but it's not the enemy's turn!");
            return;
        }

        Debug.Log("<color=orange>[EnemyManager]</color> Ending enemy turn, switching to player.");
        TurnManager.PlayerTurn();
        PlayerManager.Instance.ClearAllMoveAndAttackAreas();
    }


}
