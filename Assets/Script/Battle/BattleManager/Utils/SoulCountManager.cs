using UnityEngine;
using TMPro;

public class SoulCountManager : MonoBehaviour
{
    public static SoulCountManager Instance;

    [Header("Text Reference")]
    [SerializeField] private TMP_Text tmpText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (tmpText == null)
            tmpText = GetComponent<TMP_Text>();
    }

    public void SetText(string text)
    {
        if (tmpText != null)
            tmpText.text = text;
    }

    public void SetSoul(int current, int max = 0)
    {
        if (tmpText != null)
            tmpText.text = $"{current:0}";
    }
}