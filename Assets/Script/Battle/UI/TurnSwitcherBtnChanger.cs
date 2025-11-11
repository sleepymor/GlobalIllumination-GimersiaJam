using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnSwitcherBtnChanger : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image imageComponent;
    [SerializeField] private TextMeshProUGUI textComponent;

    [Header("Image Assets")]
    [SerializeField] private Sprite playerCircle;
    [SerializeField] private Sprite enemyCircle;

    public static TurnSwitcherBtnChanger Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void SetPlayerTurn()
    {
        if (imageComponent != null)
            imageComponent.sprite = playerCircle;

        if (textComponent != null)
            textComponent.text = "End\nTurn";
    }

    public void SetEnemyTurn()
    {
        if (imageComponent != null)
            imageComponent.sprite = enemyCircle;

        if (textComponent != null)
            textComponent.text = "Enemy\nTurn";
    }
}
