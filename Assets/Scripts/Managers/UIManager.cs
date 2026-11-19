using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;   
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    private GameObject[] AllPanels => new[] { mainMenuPanel, pausePanel, victoryPanel, gameOverPanel, settingsPanel, creditsPanel };

    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Slider healthSlider;

    [Header("Audio-Settings")]
    [SerializeField] private AudioMixer mainAudioMixer;
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    //[Header("Player Reference")]
    //[SerializeField] private HealthSystem healthSystemPlayer;

    private Resolution[] resolutions;
    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            GameManager.Instance.OnScoreChanged -= UpdateScoreHUD;

            UpdateScoreHUD(GameManager.Instance.Score);

        }

        /*if  (healthSystemPlayer != null)
        {
            healthSystemPlayer.OnChangeHealthPlayer += UpdateHealthBar;
        }

        SetupResolutions();
        */
        
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            GameManager.Instance.OnScoreChanged += UpdateScoreHUD;
        }

         /*if  (healthSystemPlayer != null)
        {
            healthSystemPlayer.OnChangeHealthPlayer += UpdateHealthBar;
        }*/
    }
    private void HandleStateChanged(GameState newState)
    {
        if (pausePanel) pausePanel.SetActive(false);
        if (victoryPanel) victoryPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);

        switch (newState)
        {
            case GameState.Start:
                LockCursor(false);
                break;

            case GameState.Playing:
                LockCursor(true);
                break;

            case GameState.Paused:
                if (pausePanel) pausePanel.SetActive(true);
                LockCursor(false);
                break;

            case GameState.Victory:
                if (victoryPanel) victoryPanel.SetActive(true);
                LockCursor(false);
                break;

            case GameState.GameOver:
                if (gameOverPanel) gameOverPanel.SetActive(true);
                LockCursor(false);
                break;
        }
    }

    private void LockCursor(bool isLocked)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;
    }

    private void UpdateScoreHUD(int currentScore)
    {
        if (scoreText != null && GameManager.Instance != null)
        {
            scoreText.text = $"{currentScore}";
        }
    }

    private void UpdateHealthBar(float healthNormalized)
    {
        if (healthSlider != null)
        {
            healthSlider.value = healthNormalized;
        }
    }

    public void ChangeSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void CloseAllPanels()
    {
        foreach (var panel in AllPanels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }

    public void OpenPanel(GameObject panelToOpen)
    {
        CloseAllPanels();
        if (panelToOpen != null)
        {
            panelToOpen.SetActive(true);
        }
    }

    public void SetMasterVolume(float sliderValue)
    {
        if (mainAudioMixer != null)
        {
            // Convierte el valor lineal del Slider (0.0001 a 1) a Decibelios (-80dB a 0dB)
            float db = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
            mainAudioMixer.SetFloat("MasterVolume", db);
        }
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    private void SetupResolutions()
    {
        if (resolutionDropdown == null) return;

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width} x {resolutions[i].height} @ {resolutions[i].refreshRateRatio.value:F0}Hz";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
