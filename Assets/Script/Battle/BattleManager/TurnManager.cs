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
        
        _currentTurn = Faction.PLAYER;
        EnemyManager.Instance.ResetEntityMoves();
    }

    public static void EnemyTurn()
    {
        Debug.Log("Change to enemy turn");
        _currentTurn = Faction.ENEMY;
        Instance.StartCoroutine(EnemyManager.Instance.RunEnemyTurn());
        PlayerManager.Instance.ResetEntityMoves();
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

}