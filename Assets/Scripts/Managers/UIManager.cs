using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //Para PlayerPrefs
    private const string MasterVolKey = "MasterVolume";
    private const string MusicVolKey = "MusicVolume";
    private const string SfxVolKey = "SFXVolume";
    private const string QualityKey = "QualityLevel";

    private bool isLoadingSettings;

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;   
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    private GameObject[] AllPanels => new[] { mainMenuPanel, pausePanel, victoryPanel, gameOverPanel, settingsPanel, creditsPanel };
    
    [Header("UI Controls (Opcional para sincronizar visuales)")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    //[SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI gemsText;
    [SerializeField] private Image gemsIcon;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI victoryScoreText;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;

    [Header("Audio-Settings")]
    [SerializeField] private AudioMixer mainAudioMixer;


    [Header("Player Reference")]
    [SerializeField] private HealthSystem healthSystemPlayer;

    [Header("Auto Button Click Sound")]
    [SerializeField] private bool autoHookButtonSounds = true;
    private Resolution[] resolutions;
    private Button[] sceneButtons;

    private void OnEnable()
    {
        SubscribeToGameManager();
        HookButtonSounds();
    }

    private void OnDisable()
    {
        UnsubscribeFromGameManager();
        UnhookButtonSounds();
    }
    void Start()
    {
        SubscribeToGameManager();

        if (GameManager.Instance != null)
        {
            UpdateScoreHUD(GameManager.Instance.Score);
            UpdateGemsCountHUD();

            HandleStateChanged(GameManager.Instance.State);
            HandleHealthChanged(GameManager.Instance.CurrentHealth, GameManager.Instance.MaxHealth);
        }

        /*if  (healthSystemPlayer != null)
        {
            healthSystemPlayer.OnChangeHealthPlayer += UpdateHealthBar;
        }*/

        //SetupResolutions();
        
        LoadSettings();
    }

    private void SubscribeToGameManager()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnStateChanged -= HandleStateChanged;
        GameManager.Instance.OnStateChanged += HandleStateChanged;

        GameManager.Instance.OnScoreChanged -= UpdateScoreHUD;
        GameManager.Instance.OnScoreChanged += UpdateScoreHUD;

        GameManager.Instance.OnHealthChanged -= HandleHealthChanged;
        GameManager.Instance.OnHealthChanged += HandleHealthChanged;

        GameManager.Instance.OnItemCollected -= HandleItemCollected;
        GameManager.Instance.OnItemCollected += HandleItemCollected;
    }

    private void UnsubscribeFromGameManager()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnStateChanged -= HandleStateChanged;
        GameManager.Instance.OnScoreChanged -= UpdateScoreHUD;
        GameManager.Instance.OnHealthChanged -= HandleHealthChanged;
        GameManager.Instance.OnItemCollected -= HandleItemCollected;
    }

    private void HookButtonSounds()
    {
        if (!autoHookButtonSounds) return;

        Canvas parentCanvas = GetComponentInParent<Canvas>();
        sceneButtons = parentCanvas != null ? parentCanvas.GetComponentsInChildren<Button>(true) : GetComponentsInChildren<Button>(true);

        foreach (Button btn in sceneButtons)
        {
            if (btn == null) continue;
            btn.onClick.RemoveListener(PlayButtonClickSound);
            btn.onClick.AddListener(PlayButtonClickSound);
        }
    }
    private void UnhookButtonSounds()
    {
        if (sceneButtons == null) return;

        foreach (Button btn in sceneButtons)
        {
            if (btn == null) continue;
            btn.onClick.RemoveListener(PlayButtonClickSound);
        }
    }

    private void PlayButtonClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClick();
        }
    }

    // ---------------- HUD ----------------

    private void UpdateScoreHUD(int currentScore)
    {
        if (scoreText != null)
        {
            scoreText.text = $"{currentScore}";
        }
    }

    private void HandleItemCollected(ItemData item)
    {
        UpdateGemsCountHUD();
    }

    private void UpdateGemsCountHUD()
    {
        if (gemsText != null && GameManager.Instance != null)
        {
            gemsText.text = $"x{GameManager.Instance.Items.Count}";
        }
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        float normalized = maxHealth > 0
            ? Mathf.Clamp01((float)currentHealth / maxHealth)
            : 0f;

        UpdateHealthBar(normalized);
    }

    private void UpdateHealthBar(float healthNormalized)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = 1f;
            healthSlider.value = healthNormalized;
        }
    }

    private void UpdateEndScoreText(TextMeshProUGUI targetText)
    {
        if (targetText == null) return;

        int finalScore = GameManager.Instance != null
            ? GameManager.Instance.Score
            : 0;

        targetText.text = $"Puntaje final: {finalScore}";
    }

    // ---------------- GAME STATES ----------------

    private void HandleStateChanged(GameState newState)
    {
        CloseAllPanels();

        switch (newState)
        {
            case GameState.Start:
                if (mainMenuPanel != null)
                    mainMenuPanel.SetActive(true);

                LockCursor(false);
                break;

            case GameState.Playing:
                LockCursor(false);
                break;

            case GameState.Paused:
                if (pausePanel != null)
                    pausePanel.SetActive(true);

                LockCursor(false);
                break;

            case GameState.Victory:
                if (victoryPanel != null)
                    victoryPanel.SetActive(true);

                UpdateEndScoreText(victoryScoreText);
                LockCursor(false);
                break;

            case GameState.GameOver:
                if (gameOverPanel != null)
                    gameOverPanel.SetActive(true);

                UpdateEndScoreText(gameOverScoreText);
                LockCursor(false);
                break;
        }
    }

    private void LockCursor(bool isLocked)
    {
        Cursor.lockState = isLocked
            ? CursorLockMode.Locked
            : CursorLockMode.None;

        Cursor.visible = !isLocked;
    }

    // ---------------- PANELS ----------------

    public void CloseAllPanels()
    {
        foreach (GameObject panel in AllPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }
    }

    public void OpenPanel(GameObject panelToOpen)
    {
        CloseAllPanels();

        if (panelToOpen != null)
            panelToOpen.SetActive(true);
    }

    // ---------------- SETTINGS ----------------

    public void LoadSettings()
    {
        isLoadingSettings = true;

        float masterVal = PlayerPrefs.GetFloat(MasterVolKey, 0.8f);
        float musicVal = PlayerPrefs.GetFloat(MusicVolKey, 0.8f);
        float sfxVal = PlayerPrefs.GetFloat(SfxVolKey, 0.8f);

        int qualityIdx = PlayerPrefs.GetInt(
            QualityKey,
            QualitySettings.GetQualityLevel()
        );

        SetMasterVolume(masterVal);
        SetMusicVolume(musicVal);
        SetSFXVolume(sfxVal);
        SetQuality(qualityIdx);

        if (masterSlider != null)
            masterSlider.SetValueWithoutNotify(masterVal);

        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(musicVal);

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(sfxVal);

        if (qualityDropdown != null)
            qualityDropdown.SetValueWithoutNotify(qualityIdx);

        isLoadingSettings = false;
    }

    public void SetMasterVolume(float sliderValue)
    {
        if (mainAudioMixer != null)
        {
            float volume = sliderValue > 0f
                ? Mathf.Log10(sliderValue) * 20f
                : -80f;

            mainAudioMixer.SetFloat("MasterVolume", volume);
        }

        if (!isLoadingSettings)
        {
            PlayerPrefs.SetFloat(MasterVolKey, sliderValue);
            PlayerPrefs.Save();
        }
    }

    public void SetSFXVolume(float sliderValue)
    {
        if (mainAudioMixer != null)
        {
            float volume = sliderValue > 0f
                ? Mathf.Log10(sliderValue) * 20f
                : -80f;

            mainAudioMixer.SetFloat("SFX", volume);
        }

        if (!isLoadingSettings)
        {
            PlayerPrefs.SetFloat(SfxVolKey, sliderValue);
            PlayerPrefs.Save();
        }
    }

    public void SetMusicVolume(float sliderValue)
    {
        if (mainAudioMixer != null)
        {
            float volume = sliderValue > 0f
                ? Mathf.Log10(sliderValue) * 20f
                : -80f;

            mainAudioMixer.SetFloat("Music", volume);
        }

        if (!isLoadingSettings)
        {
            PlayerPrefs.SetFloat(MusicVolKey, sliderValue);
            PlayerPrefs.Save();
        }
    }

    public void SetQuality(int qualityIndex)
    {
        qualityIndex = Mathf.Clamp(
            qualityIndex,
            0,
            QualitySettings.names.Length - 1
        );

        QualitySettings.SetQualityLevel(qualityIndex);

        if (!isLoadingSettings)
        {
            PlayerPrefs.SetInt(QualityKey, qualityIndex);
            PlayerPrefs.Save();
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    // ---------------- GAME ACTIONS ----------------

    public void StartGame(string firstLevelName)
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.StartNewGame(firstLevelName);
        else
            SceneManager.LoadScene(firstLevelName);
    }

    public void ChangeSceneByName(string sceneName)
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.StartNewGame(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }

    public void ResumeGame()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.State == GameState.Paused)
        {
            GameManager.Instance.Paused();
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.RestartCurrentLevel();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.LoadMainMenu();
        else
            SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
        else
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
