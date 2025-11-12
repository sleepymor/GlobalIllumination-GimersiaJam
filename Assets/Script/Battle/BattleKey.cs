using UnityEngine;

public class BattleKey : MonoBehaviour
{
    public string key;
    public static BattleKey Instance;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        Instance = this;
    }
}