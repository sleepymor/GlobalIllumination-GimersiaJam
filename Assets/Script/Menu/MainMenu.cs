using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject kontrolPanel;
    [SerializeField] private GameObject keluarPanel;
    [SerializeField] private List<string> battleList = new List<string>();

    private int sceneToContinue;

    [Header("Menu State Flags")]
    private bool isMainMenuPanel = false;
    private bool isSettingsOpen = false;
    private bool isKontrolOpen = false;
    private bool isKeluarConfirmOpen = false;

    public bool IsMenuOpen => isMainMenuPanel || isSettingsOpen || isKontrolOpen || isKeluarConfirmOpen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscape();
        }
    }

    private void HandleEscape()
    {
        if (isKeluarConfirmOpen)
        {
            CancelKeluar();
        }
        else if (isSettingsOpen)
        {
            CloseSettings();
        }
        else if (isKontrolOpen)
        {
            CloseKontrol();
        }
    }

    public void NewGame()
    {
        PlayerPrefs.SetFloat("PlayerX", -45.74f);
        PlayerPrefs.SetFloat("PlayerY", 0.948f);
        PlayerPrefs.SetFloat("PlayerZ", 1.31f);
        foreach (var i in battleList)
        {
            PlayerPrefs.SetInt(i, 0);
        }
        SceneManager.LoadScene("Level_1");
    }

    public void ContinueGame()
    {
        sceneToContinue = PlayerPrefs.GetInt("SavedScene", 0);

        if (sceneToContinue != 0)
        {
            SceneManager.LoadScene(sceneToContinue);
        }
        else
        {
            Debug.LogWarning("Tidak ada save game yang ditemukan!");
        }
    }

    public void OpenMainMenuPanel()
    {
        isMainMenuPanel = true;
        mainMenuPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        isSettingsOpen = true;
        settingsPanel?.SetActive(true);

        isMainMenuPanel = false;
        mainMenuPanel?.SetActive(false);
    }

    public void OpenKontrol()
    {
        isKontrolOpen = true;
        kontrolPanel?.SetActive(true);

        isMainMenuPanel = false;
        mainMenuPanel?.SetActive(false);
    }

    public void OpenKeluarPanel()
    {
        keluarPanel?.SetActive(true);
        isKeluarConfirmOpen = true;

        isMainMenuPanel = false;
        mainMenuPanel?.SetActive(false);
    }

    public void CancelKeluar()
    {
        keluarPanel?.SetActive(false);
        isKeluarConfirmOpen = false;

        isMainMenuPanel = true;
        mainMenuPanel?.SetActive(true);
    }

    public void ConfirmKeluar()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
#endif
    }

    public void CloseSettings()
    {
        isSettingsOpen = false;
        settingsPanel?.SetActive(false);

        isMainMenuPanel = true;
        mainMenuPanel?.SetActive(true);
    }

    public void CloseKontrol()
    {
        isKontrolOpen = false;
        kontrolPanel?.SetActive(false);

        isMainMenuPanel = true;
        mainMenuPanel?.SetActive(true);
    }
}
