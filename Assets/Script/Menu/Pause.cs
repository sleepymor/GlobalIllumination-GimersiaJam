using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    [Header("Main Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject resumeButton;
    [SerializeField] private GameObject kontrolPanel;
    [SerializeField] private GameObject settingPanel; // <-- tambahkan variabel yang hilang

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (kontrolPanel.activeSelf || settingPanel.activeSelf)
            {
                BackToPause();
            }
            else if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void OnPauseButtonPressed()
    {
        PlayButtonSound();
        if (!isPaused)
            PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        pausePanel.SetActive(true);
        resumeButton.SetActive(true);
        kontrolPanel.SetActive(false);
        settingPanel.SetActive(false);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        pausePanel.SetActive(false);
        kontrolPanel.SetActive(false);
        settingPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        PlayButtonSound();

        pausePanel.SetActive(false);
        kontrolPanel.SetActive(false);
        settingPanel.SetActive(true);
    }

    public void OpenKontrol()
    {
        PlayButtonSound();

        pausePanel.SetActive(false);
        settingPanel.SetActive(false);
        kontrolPanel.SetActive(true);
    }

    public void BackToPause()
    {
        PlayButtonSound();

        settingPanel.SetActive(false);
        kontrolPanel.SetActive(false);

        pausePanel.SetActive(true);
        resumeButton.SetActive(true);
    }

    public void GoToMainMenu()
    {
        PlayButtonSound();

        // Simpan scene terakhir
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        PlayerPrefs.SetInt("SavedScene", currentSceneIndex);

        Time.timeScale = 1f;

        if (SceneController.instance != null)
            SceneController.instance.LoadScene("Main_menu");
        else
            SceneManager.LoadScene("Main_menu");
    }

    private void PlayButtonSound()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("Button");
    }
}
