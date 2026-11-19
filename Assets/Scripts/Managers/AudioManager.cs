using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicObject;
    [SerializeField] private AudioSource soundFXObject;

    [Header("Music & UI Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip playingMusic;
    [SerializeField] private AudioClip victoryMusic;
    [SerializeField] private AudioClip gameOverMusic;
    [SerializeField] private AudioClip clickClip;

    [Header("Gameplay SFX Clips")]
    [SerializeField] private AudioClip[] potionClip;
    [SerializeField] private AudioClip[] coinClip;
    [SerializeField] private AudioClip[] hitClip;
    [SerializeField] private AudioClip[] playerHurtClip;

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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleGameStateChanged;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleGameStateChanged;
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            // Suscripción aunque GameManager se haya creado después
            GameManager.Instance.OnStateChanged -= HandleGameStateChanged;
            GameManager.Instance.OnStateChanged += HandleGameStateChanged;
            HandleGameStateChanged(GameManager.Instance.State);
        }
    }

    private void HandleGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Start:
                PlayMusic(menuMusic);
                break;

            case GameState.Playing:
                PlayMusic(playingMusic);
                break;

            case GameState.Victory:
                PlayMusic(victoryMusic);
                break;

            case GameState.GameOver:
                PlayMusic(gameOverMusic);
                break;
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicObject == null || clip == null) return;
        if (musicObject.clip == clip && musicObject.isPlaying) return;

        musicObject.clip = clip;
        musicObject.loop = true;
        musicObject.Play();
    }

    public void PlayClick()
    {
        if (clickClip == null) return;
        // Usamos el transform del AudioManager ya que es un sonido de UI global
        PlaySFX(clickClip, transform, 1f);
    }

    public void PlaySFX(AudioClip audioClip, Transform spawnTransform, float volume = 1f)
    {
        if (soundFXObject == null || audioClip == null || spawnTransform == null) return;
        //spawn in gameObject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();
        StartCoroutine(DestroyAfterClip(audioSource.gameObject, audioClip.length));

    }
    private IEnumerator DestroyAfterClip(GameObject target, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (target != null)
        {
            Destroy(target);
        }
    }

    public void PlayRandomSFX(AudioClip[] audioClips, Transform spawnTransform, float volume = 1f)
    {
        if (soundFXObject == null || audioClips == null || spawnTransform == null) return;
        int rand = Random.Range(0, audioClips.Length);
        PlaySFX(audioClips[rand], spawnTransform, volume);

    }

    // Atajos
    public void PlayPotion(Transform spawnTransform, float volume = 1f)
    {
        PlayRandomSFX(potionClip, spawnTransform, volume);
    }

    public void PlayHitEnemy(Transform spawnTransform, float volume = 1f)
    {
        PlayRandomSFX(hitClip, spawnTransform, volume);
    }

    public void PlayCoin(Transform spawnTransform, float volume = 1f)
    {
        PlayRandomSFX(coinClip, spawnTransform, volume);
    }

    public void PlayPlayerHurt(Transform spawnTransform, float volume = 1f)
    {
        PlayRandomSFX(playerHurtClip, spawnTransform, volume);
    }
}
