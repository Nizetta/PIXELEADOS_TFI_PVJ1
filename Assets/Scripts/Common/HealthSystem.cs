using System;
using System.Collections;
using UnityEngine;
public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invulnerabilityDuration = 0.5f;
    [Header("Invulnerability Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float blinkInterval = 0.1f;
    [SerializeField] private float blinkAlpha = 0.25f;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;
    public float NormalizedHealth => maxHealth > 0 ? Mathf.Clamp01((float)CurrentHealth / maxHealth) : 0f;

    public event Action<int, int> OnHealthChanged;       // (current, max)
    public event Action<float> OnHealthNormalizedChanged; // 0..1
    public event Action<int> OnDamaged;                 // damage amount
    public event Action<int> OnHealed;                  // heal amount
    public event Action OnDied;

    private float invulnerabilityTimer;
    private Coroutine blinkCoroutine;
    private Color originalColor;

    private void Awake()
    {
        if (playerStats != null) maxHealth = playerStats.MaxHealth;
        else Debug.LogError("PlayerStats no est� asignado en HealthSystem.");
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        blinkInterval = Mathf.Max(0.02f, blinkInterval);
        blinkAlpha = Mathf.Clamp01(blinkAlpha);
        int initialHealth = maxHealth;

        if (GameManager.Instance != null) initialHealth = GameManager.Instance.GetSessionHealth(maxHealth);

        SetHealth(initialHealth, notifyManager: true);
    }
    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnUpgradesChanged += UpdateMaxHealth;
        }
    }
    private void OnDisable()
    {
        if (playerStats != null) playerStats.OnUpgradesChanged -= UpdateMaxHealth;
        StopBlink();

    }

    private void Update()
    {
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;
        }
    }
    private void UpdateMaxHealth()
    {
        if (playerStats == null) return;

        SetMaxHealth(playerStats.MaxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0 || invulnerabilityTimer > 0f) return;

        int previousHealth = CurrentHealth;
        int actualDamage = Mathf.Min(amount, CurrentHealth);
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

        if (invulnerabilityDuration > 0f)
        {
            invulnerabilityTimer = invulnerabilityDuration;
            StartBlink();
        }

        OnDamaged?.Invoke(actualDamage);
        NotifyHealthChanged(previousHealth);

    GameManager.Instance?.ReportDamage(actualDamage, CurrentHealth, maxHealth);
       

        if (CurrentHealth <= 0)
        {
            StopBlink();
            OnDied?.Invoke();
            GameManager.Instance?.NotifyPlayerDead();
        }
    }
    private void StartBlink()
    {
        if (spriteRenderer == null) return;

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        blinkCoroutine = StartCoroutine(BlinkEffect());
    }

    private IEnumerator BlinkEffect()
    {
        float elapsed = 0f;
        bool transparent = false;

        while (elapsed < invulnerabilityDuration)
        {
            transparent = !transparent;

            Color color = originalColor;
            color.a = transparent ? blinkAlpha : originalColor.a;

            spriteRenderer.color = color;

            float waitTime = Mathf.Min(
                blinkInterval,
                invulnerabilityDuration - elapsed
            );

            yield return new WaitForSeconds(waitTime);
            elapsed += waitTime;
        }

        RestoreSprite();
        blinkCoroutine = null;
    }

    private void StopBlink()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        RestoreSprite();
    }

    private void RestoreSprite()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0 || CurrentHealth >= maxHealth) return;

        int previousHealth = CurrentHealth;
        int actualHeal = Mathf.Min(amount, maxHealth - CurrentHealth);
        CurrentHealth += actualHeal;

        OnHealed?.Invoke(actualHeal);
        NotifyHealthChanged(previousHealth);
        GameManager.Instance?.ReportHealth(CurrentHealth, maxHealth);
       
    }

    public void SetMaxHealth(int newMaxHealth, bool fillToMax = false)
    {
        maxHealth = Mathf.Max(1, newMaxHealth);
        int previousHealth = CurrentHealth;
        CurrentHealth = fillToMax ? maxHealth : Mathf.Clamp(CurrentHealth, 0, maxHealth);

        NotifyHealthChanged(previousHealth);

        GameManager.Instance?.ReportHealth(CurrentHealth, maxHealth);
        
    }

    private void SetHealth(int value, bool notifyManager)
    {
        int previousHealth = CurrentHealth;
        CurrentHealth = Mathf.Clamp(value, 0, maxHealth);
        NotifyHealthChanged(previousHealth);
        if (notifyManager) GameManager.Instance?.ReportHealth(CurrentHealth, maxHealth);
    }

    private void NotifyHealthChanged(int previousHealth)
    {
        if (CurrentHealth == previousHealth) return;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        OnHealthNormalizedChanged?.Invoke(NormalizedHealth);
    }
}
