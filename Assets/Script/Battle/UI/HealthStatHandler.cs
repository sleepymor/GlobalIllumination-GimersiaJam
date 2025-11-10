using UnityEngine;
using TMPro;

public class HealthStatHandler : MonoBehaviour
{

    [Header("Text Reference")]
    [SerializeField] private TMP_Text tmpText;

    private void Awake()
    {
        if (tmpText == null)
            tmpText = GetComponent<TMP_Text>();
    }

    public void SetText(string text)
    {
        if (tmpText != null)
            tmpText.text = text;
    }

    public void SetHealth(int current, int max = 0)
    {
        if (tmpText != null)
            tmpText.text = $"{current:0}";
    }
}
