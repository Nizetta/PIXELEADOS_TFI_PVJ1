using TMPro;
using UnityEngine;
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

    [Header("Auto Button Click Sound")]
    [SerializeField] private bool autoHookButtonSounds = true;
    private Resolution[] resolutions;
    private Button[] sceneButtons;

    private void Awake()
    {
        if (autoHookButtonSounds)
        {
            sceneButtons = GetComponentsInChildren<Button>(true);
        }
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            GameManager.Instance.OnScoreChanged += UpdateScoreHUD;
        }

        if (sceneButtons != null)
        {
            foreach (Button btn in sceneButtons)
            {
                if (btn != null)
                {
                    btn.onClick.AddListener(PlayButtonClickSound);
                }
            }
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
            GameManager.Instance.OnScoreChanged -= UpdateScoreHUD;
        }

        if (sceneButtons != null)
        {
            foreach (Button btn in sceneButtons)
            {
                if (btn != null)
                {
                    btn.onClick.RemoveListener(PlayButtonClickSound);
                }
            }
        }
    }
    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            GameManager.Instance.OnScoreChanged -= UpdateScoreHUD;
            GameManager.Instance.OnScoreChanged += UpdateScoreHUD;

            UpdateScoreHUD(GameManager.Instance.Score);
            HandleStateChanged(GameManager.Instance.State);
        }

        /*if  (healthSystemPlayer != null)
        {
            healthSystemPlayer.OnChangeHealthPlayer += UpdateHealthBar;
        }

        SetupResolutions();
        */

    }

    private void PlayButtonClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClick();
        }
    }

    private void HandleStateChanged(GameState newState)
    {
        CloseAllPanels();

        switch (newState)
        {
            case GameState.Start:
                if (mainMenuPanel) mainMenuPanel.SetActive(true);
                LockCursor(false);
                break;

            case GameState.Playing:
                LockCursor(false);
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
        Time.timeScale = 1f;
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
            mainAudioMixer.SetFloat("MasterVolume", Mathf.Log10(sliderValue)*20);
        }
    }

    public void SetSFXVolume(float sliderValue)
    {
        if (mainAudioMixer != null)
        {
            mainAudioMixer.SetFloat("SFX", Mathf.Log10(sliderValue) * 20);
        }
    }

    public void SetMusicVolume(float sliderValue)
    {
        if (mainAudioMixer != null)
        {
            mainAudioMixer.SetFloat("Music", Mathf.Log10(sliderValue) * 20);
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

    /*private void SetupResolutions()
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
    }*/

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
