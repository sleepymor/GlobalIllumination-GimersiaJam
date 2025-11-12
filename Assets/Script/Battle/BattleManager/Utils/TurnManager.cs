/*
 * TurnManager.cs
 * ------------------------
 * Singleton manager that controls the flow of turns in a turn-based strategy game.
 *
 * Responsibilities:
 * - Track the current turn's faction using a static variable.
 * - Provide static methods to switch turns between PLAYER, ENEMY, ALLY, and WILD factions.
 * - Trigger enemy AI turn via coroutine when switching to ENEMY turn.
 * - Reset movement flags for units at the start of their respective turns.
 *
 * Core Features:
 * - PlayerTurn(): Sets the current turn to PLAYER and resets enemy movements.
 * - EnemyTurn(): Sets the current turn to ENEMY, triggers enemy AI actions, and resets player movements.
 * - AllyTurn(): Sets the current turn to ALLY (placeholder for ally logic).
 * - WildTurn(): Sets the current turn to WILD (placeholder for neutral/wild entity logic).
 * - GetCurrentTurn(): Returns the faction whose turn is currently active.
 *
 * Notes:
 * - Designed as a singleton for easy global access.
 * - Relies on EnemyManager and PlayerManager singletons to coordinate entity actions.
 *
 * Usage:
 * - Call TurnManager.PlayerTurn(), TurnManager.EnemyTurn(), etc. to change turns.
 * - Use TurnManager.GetCurrentTurn() to check whose turn it currently is.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    private static Faction _currentTurn;

    public void Awake()
    {
        Instance = this;
    }
    public static void PlayerTurn()
    {
        List<EntityMaster> unit = PlayerManager.Instance.TeamList;
        foreach (var i in unit)
        {
            i.status.StartTurnEffect();
        }
        _currentTurn = Faction.PLAYER;
        EnemyManager.Instance.ResetEntityMoves();
        SoulCountManager.Instance.SetSoul(PlayerManager.Instance.GetSummoner().soul.GetSoulCount(), PlayerManager.Instance.GetSummoner().data.maxSoul);
        PlayerManager.Instance.ClearAllMoveAndAttackAreas();
        PlayerDeckManager.Instance.DrawCard();
        TurnSwitcherBtnChanger.Instance.SetPlayerTurn();
    }

    public static void EnemyTurn()
    {
        List<EntityMaster> unit = EnemyManager.Instance.TeamList;
        foreach (var i in unit)
        {
            i.status.StartTurnEffect();
        }

        Debug.Log("Change to enemy turn");
        _currentTurn = Faction.ENEMY;
        Instance.StartCoroutine(EnemyManager.Instance.RunEnemyTurn());
        PlayerManager.Instance.ResetEntityMoves();
        PlayerManager.Instance.ClearAllMoveAndAttackAreas();
        if (EnemyManager.Instance.GetSummoner() != null) SoulCountManager.Instance.SetSoul(EnemyManager.Instance.GetSummoner().soul.GetSoulCount(), PlayerManager.Instance.GetSummoner().data.maxSoul);
        TurnSwitcherBtnChanger.Instance.SetEnemyTurn();
    }

    public static void AllyTurn()
    {
        _currentTurn = Faction.ALLY;
    }

    public static void WildTurn()
    {
        _currentTurn = Faction.WILD;
    }

    public static Faction GetCurrentTurn()
    {
        return _currentTurn;
    }

    public static void PlayerWin()
    {
        PlayerPrefs.SetInt(BattleKey.Instance.key, 1);
        SceneController.instance.LoadPreviousScene();
    }

    public static void PlayerLose()
    {
        LoadScene("Main_menu");
    }

    public static void LoadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            if (SceneController.instance != null)
            {
                SceneController.instance.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning("SceneController instance tidak ditemukan!");
            }
        }
        else
        {
            Debug.LogWarning("Nama scene belum diisi!");
        }
    }
}