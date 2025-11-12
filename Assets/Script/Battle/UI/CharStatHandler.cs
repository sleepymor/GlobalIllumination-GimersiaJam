using UnityEngine;
using TMPro;
using System.Collections;

public class CharStatHandler : MonoBehaviour
{
    [Header("Stat UI Objects")]
    [SerializeField] private GameObject healthUI;
    [SerializeField] private GameObject attackUI;
    [SerializeField] private GameObject defenseUI;
    [SerializeField] private GameObject critUI;
    [SerializeField] private GameObject poisonUI;
    [SerializeField] private GameObject freezeUI;
    [SerializeField] private GameObject damageUI;

    [Header("Text References")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text defenseText;
    [SerializeField] private TMP_Text critText;
    [SerializeField] private TMP_Text poisonText;
    [SerializeField] private TMP_Text freezeText;
    [SerializeField] private TMP_Text damageText;

    [Header("Damage Text Settings")]
    [SerializeField] private float damageDisplayDuration = 1.2f;
    [SerializeField] private float fadeDuration = 0.4f;

    private EntityMaster _e;

    void Awake()
    {
        HideAllStats();
        ShowStat(healthUI); // default
    }

    public void Initialize(EntityMaster e)
    {
        _e = e;
    }

    // --- Set teks masing-masing stat ---
    public void SetHealth(int current, int max = 0)
    {
        if (healthText != null)
            healthText.text = max > 0 ? $"{current}/{max}" : $"{current}";
    }

    public void SetAttack(int value)
    {
        if (attackText != null)
            attackText.text = $"{value}";
    }

    public void SetDefense(int value)
    {
        if (defenseText != null)
            defenseText.text = $"{value}";
    }

    public void SetCrit(float value)
    {
        if (critText != null)
            critText.text = $"{value:0.#}%";
    }

    public void SetPoison(float value)
    {
        if (poisonText != null)
            poisonText.text = $"{value:0.#}";
    }

    public void SetFreeze(float value)
    {
        if (freezeText != null)
            freezeText.text = $"{value:0.#}";
    }

    // --- Show per-stat (untuk akses luar) ---
    public void ShowHealth() => ShowStat(healthUI);
    public void ShowAttack() => ShowStat(attackUI);
    public void ShowDefense() => ShowStat(defenseUI);
    public void ShowCrit() => ShowStat(critUI);
    public void ShowPoison() => ShowStat(poisonUI);
    public void ShowFreeze() => ShowStat(freezeUI);
    public void ShowDamageUI() => ShowStat(damageUI);

    // --- Hide per-stat (optional) ---
    public void HideHealth() => HideStat(healthUI);
    public void HideAttack() => HideStat(attackUI);
    public void HideDefense() => HideStat(defenseUI);
    public void HideCrit() => HideStat(critUI);
    public void HidePoison() => HideStat(poisonUI);
    public void HideFreeze() => HideStat(freezeUI);
    public void HideDamageUI() => HideStat(damageUI);

    // --- Damage text: show briefly and bounce + fade ---
    public void ShowDamage(int amount)
    {
        if (damageText == null) return;

        damageText.gameObject.SetActive(true);
        damageText.alpha = 1f;
        damageText.text = $"-{amount}";

        StopCoroutine(nameof(DamageBounceEffect));
        StartCoroutine(DamageBounceEffect());
    }

    private IEnumerator DamageBounceEffect()
    {
        RectTransform rt = damageText.rectTransform;

        Vector2 startPos = rt.anchoredPosition;
        Vector2 upPos = startPos + new Vector2(0, 0.3f);
        Vector2 bouncePos = startPos + new Vector2(0, 0.05f);

        float upDuration = 0.18f;
        float downDuration = 0.12f;
        float fadeDelay = 0.25f;
        float fadeDuration = 0.3f;

        float t = 0f;
        while (t < upDuration)
        {
            t += Time.deltaTime;
            float normalized = t / upDuration;
            float ease = 1f - Mathf.Pow(1f - normalized, 2f);
            rt.anchoredPosition = Vector2.Lerp(startPos, upPos, ease);
            yield return null;
        }

        t = 0f;
        while (t < downDuration)
        {
            t += Time.deltaTime;
            float normalized = t / downDuration;
            float ease = Mathf.Sin(normalized * Mathf.PI * 0.5f);
            rt.anchoredPosition = Vector2.Lerp(upPos, bouncePos, ease);
            yield return null;
        }

        yield return new WaitForSeconds(fadeDelay);

        t = 0f;
        float startAlpha = damageText.alpha;
        Vector2 fadeStartPos = rt.anchoredPosition;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float normalized = t / fadeDuration;
            rt.anchoredPosition = Vector2.Lerp(fadeStartPos, startPos, normalized);
            damageText.alpha = Mathf.Lerp(startAlpha, 0f, normalized);
            yield return null;
        }

        rt.anchoredPosition = startPos;
        damageText.alpha = 0f;
        damageText.gameObject.SetActive(false);
    }

    // --- Hide / Show Helper ---
    public void HideStat(GameObject statUI)
    {
        if (statUI != null)
            statUI.SetActive(false);
    }

    public void ShowStat(GameObject statUI)
    {
        if (statUI != null)
            statUI.SetActive(true);
    }

    public void HideAllStats()
    {
        HideStat(healthUI);
        HideStat(attackUI);
        HideStat(defenseUI);
        HideStat(critUI);
        HideStat(poisonUI);
        HideStat(freezeUI);
        HideStat(damageUI);
    }
}
