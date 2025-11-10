using UnityEngine;
using TMPro;

public class HealthStatHandler : MonoBehaviour
{
    // Can hold either TextMeshPro or TextMeshProUGUI
    [Header("Text Reference")]
    [SerializeField] private TMP_Text tmpText; // 👈 TMP_Text is the base class

    private void Awake()
    {
        // Auto-detect any TextMeshPro component if not assigned
        if (tmpText == null)
            tmpText = GetComponent<TMP_Text>();
    }

    /// <summary>
    /// Sets custom text on the TextMeshPro component.
    /// </summary>
    public void SetText(string text)
    {
        if (tmpText != null)
            tmpText.text = text;
    }

    /// <summary>
    /// Sets text to show current and max health, e.g. "75 / 100".
    /// </summary>
    public void SetHealth(float current, float max = 0)
    {
        if (tmpText != null)
            tmpText.text = $"{current:0}";
    }
}
