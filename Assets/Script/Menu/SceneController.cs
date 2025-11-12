using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class SceneController : MonoBehaviour
{
    public static SceneController instance;
    private int sceneToContinue;

    [Header("Transition + Loading UI")]
    [SerializeField] private Animator transitionAnim;
    [SerializeField] private GameObject transitionUI;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TMP_Text loadingText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.root.gameObject);

            if (transitionUI != null)
            {
                transitionUI.SetActive(true);
                Canvas c = transitionUI.GetComponent<Canvas>();
                if (c != null)
                {
                    c.overrideSorting = true;
                    c.sortingOrder = 100;
                }
            }

            if (loadingSlider != null)
                loadingSlider.gameObject.SetActive(false);
        }
        else
        {
            if (instance.transitionUI != null)
                instance.transitionUI.SetActive(false);
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (transitionUI != null && transitionUI.activeSelf)
        {
            StartCoroutine(HideTransitionUIAfterDelay(1.5f));
        }
    }


    public void ContinueGame()
    {
        string lastScene = PlayerPrefs.GetString("LastScene", "");

        if (!string.IsNullOrEmpty(lastScene))
        {
            LoadScene(lastScene);
        }
        else
        {
            Debug.LogWarning("Belum ada progress yang disimpan!");
        }
    }


    public void LoadPreviousScene()
    {
        int previousSceneIndex = PlayerPrefs.GetInt("SavedScene", -1);

        if (previousSceneIndex >= 0)
        {
            Debug.Log($"[SceneController] Loading previous scene index {previousSceneIndex}...");
            StartCoroutine(LoadLevelByIndex(previousSceneIndex));
        }
        else
        {
            Debug.LogWarning("Tidak ada scene sebelumnya yang tersimpan!");
        }
    }


    private System.Collections.IEnumerator HideTransitionUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (transitionUI != null)
        {
            transitionUI.SetActive(false);
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadLevelByName(sceneName));
    }

    public void NextLevel()
    {
        StartCoroutine(LoadLevelByIndex(SceneManager.GetActiveScene().buildIndex + 1));
    }

    private System.Collections.IEnumerator LoadLevelByIndex(int index)
    {
        if (transitionUI != null)
            transitionUI.SetActive(true);

        if (transitionAnim != null)
            transitionAnim.SetTrigger("End");

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(index);

        if (transitionAnim != null)
            transitionAnim.SetTrigger("Start");

        yield return new WaitForSeconds(1f);

        if (transitionUI != null)
            transitionUI.SetActive(false);
    }

    private System.Collections.IEnumerator LoadLevelByName(string name)
    {
        if (transitionUI != null)
            transitionUI.SetActive(true);

        if (transitionAnim != null)
            transitionAnim.SetTrigger("End");

        yield return new WaitForSeconds(1f);

        if (loadingSlider != null)
        {
            loadingSlider.gameObject.SetActive(true);
            loadingSlider.value = 0f;
        }

        AsyncOperation sceneLoadOperation = SceneManager.LoadSceneAsync(name);
        sceneLoadOperation.allowSceneActivation = false;

        while (!sceneLoadOperation.isDone)
        {
            float progress = Mathf.Clamp01(sceneLoadOperation.progress / 0.9f);

            if (loadingSlider != null)
                loadingSlider.value = progress;

            if (loadingText != null)
                loadingText.text = Mathf.RoundToInt(progress * 100f) + "%";

            if (sceneLoadOperation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(0.5f);
                sceneLoadOperation.allowSceneActivation = true;
            }

            yield return null;
        }

        if (transitionAnim != null)
            transitionAnim.SetTrigger("Start");

        yield return new WaitForSeconds(1f);

        if (loadingSlider != null)
            loadingSlider.gameObject.SetActive(false);

        if (transitionUI != null)
            transitionUI.SetActive(false);

    }
}
