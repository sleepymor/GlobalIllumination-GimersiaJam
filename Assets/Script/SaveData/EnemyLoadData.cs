using UnityEngine;

public class EnemyLoadData : MonoBehaviour
{
    [Header("Unique Save Key for This Enemy")]
    public string key; 

    void Start()
    {
        // Check if this enemy has been defeated before
        int isDefeated = PlayerPrefs.GetInt(key, 0); // default 0 = not defeated

        if (isDefeated == 1)
        {
            // Disable this enemy if already defeated
            gameObject.SetActive(false);
            Debug.Log($"{key} already defeated, hiding enemy.");
        }
    }
}
