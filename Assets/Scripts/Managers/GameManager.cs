using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Start,
    Playing,
    Paused,
    Victory,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Data")]
    public GameState State { get; private set; } = GameState.Start;
    public int Score { get; private set; }
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }

    private readonly List<ItemData> items = new List<ItemData>();
    public IReadOnlyList<ItemData> Items => items;

    [Header("Scene Configurattion")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string[] levelSceneName;

    //-- Eventos para desacoplar ---
    public event Action<GameState> OnStateChanged;
    public event Action<int> OnScoreChanged;
    public event Action<int, int> OnHealthChanged;
    public event Action<int> OnPlayerDamaged;
    public event Action<ItemData> OnItemCollected;

    
    private bool sessionStarted;

    //--- Transicion con Fade entre niveles ---
    private SceneTransition sceneTransition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;

    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }
    private void Start()
    {
        ApplySceneState(SceneManager.GetActiveScene());
    }
    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (State == GameState.Playing || State == GameState.Paused)
            {
                Paused();
            }
        }
    }

    // ---------- ESTADO SEGÚN LA ESCENA ----------
    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneTransition = null;
        ApplySceneState(scene);
    }

    private void ApplySceneState(Scene scene)
    {
        string sceneName = scene.name;

        if (sceneName == mainMenuSceneName)
        {
            SetState(GameState.Start);
        }
        else
        {
            if (!sessionStarted) BeginSession();
            SetState(GameState.Playing);
            Debug.Log($"State actual: {State}");
        }
    }

    private void SetState(GameState newState)
    {
        if (State == newState) return;
        State = newState;

        Time.timeScale = (State == GameState.Paused || State == GameState.Victory || State == GameState.GameOver ) ? 0f : 1f;

        Debug.Log($"[GameManager] Nuevo estado: {State}");
        OnStateChanged?.Invoke(State);
    }

    private void BeginSession()
    {
        Score = 0;
        CurrentHealth = 100;
        MaxHealth = 100;
        items.Clear();
        sessionStarted = true;
        OnScoreChanged?.Invoke(Score);
    }

    public void Paused()
    {
        if (State == GameState.Playing) SetState(GameState.Paused);
        else if (State == GameState.Paused) SetState(GameState.Playing);
    }

  
    public void StartNewGame(string firstLevelName)
    {
        BeginSession();
        StartCoroutine(FadeAndLoadScene(firstLevelName));
    }

    public void RestartCurrentLevel()
    {
        BeginSession();
        Time.timeScale = 1f;
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }

    public void LoadMainMenu()
    {
        sessionStarted = false;
        LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Pasa al siguiente nivel conservando puntaje, vida e ítems; si era el último, gana.
    public void CompleteLevel()
    {
        if (State != GameState.Playing) return;

        string current = SceneManager.GetActiveScene().name;
        int index = Array.IndexOf(levelSceneName, current);

        if (index >= 0 && index + 1 < levelSceneName.Length)
        {
            Debug.Log("[GameManager] Nivel completado: " + current + " -> " + levelSceneName[index + 1]);
            StartCoroutine(FadeAndLoadScene(levelSceneName[index + 1]));
        }
        else
        {
            Win();
        }
    }

    public void Win()
    {
        if (State != GameState.Playing) return;
        SetState(GameState.Victory);
    }

    public void Lose()
    {
        if (State != GameState.Playing) return;
        SetState(GameState.GameOver);
    }

    public void NotifyPlayerDead()
    {
        Lose();
    }

    // -- Datos del jugador e items --
    public void AddScore(int points)
    {
        if (State != GameState.Playing) return;

        Score += points;
        OnScoreChanged?.Invoke(Score);
    }

    public void CollectItem(ItemData item)
    {
        if (State != GameState.Playing || item == null) return;

        items.Add(item);
        //Debug.Log("[GameManager] Ítem recolectado: " + item.DisplayName);
        OnItemCollected?.Invoke(item);
        //AddScore(item.Points);
    }

    public void ReportHealth(int current, int max)
    {
        CurrentHealth = current;
        MaxHealth = max;
        OnHealthChanged?.Invoke(current, max);
    }

    public void ReportDamage(int amount, int current, int max)
    {
        OnPlayerDamaged?.Invoke(amount);
        ReportHealth(current, max);
    }

    public int GetSessionHealth(int maxHealth)
    {
        return CurrentHealth > 0 ? Mathf.Min(CurrentHealth, maxHealth) : maxHealth;
    }

    // -- Transición con Fade entre niveles --

    private void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        AudioManager.Instance?.FadeMusicOut();
        GetSceneTransition().FadeOut();
        yield return GetSceneTransition().changeAlpha; // Espera a que termine el fade out
        LoadScene(sceneName);
    }
    private SceneTransition GetSceneTransition()
    {
        if (sceneTransition == null)
        {
            sceneTransition = FindAnyObjectByType<SceneTransition>();
            if (sceneTransition == null)
            {
                Debug.LogWarning("[GameManager] No se encontró un SceneTransition en la escena.");
            }
        }
        return sceneTransition;
    }
}