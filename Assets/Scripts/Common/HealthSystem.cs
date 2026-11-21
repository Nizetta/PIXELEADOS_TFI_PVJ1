using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invulnerabilityDuration = 0.5f;

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

    private void Awake()
    {
        int initialHealth = maxHealth;

        if (GameManager.Instance != null)
        {
            initialHealth = GameManager.Instance.GetSessionHealth(maxHealth);
        }

        SetHealth(initialHealth, notifyManager: true);
    }

    private void Update()
    {
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;
        }
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
        }

        OnDamaged?.Invoke(actualDamage);
        NotifyHealthChanged(previousHealth);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReportDamage(actualDamage, CurrentHealth, maxHealth);
        }

        if (CurrentHealth <= 0)
        {
            OnDied?.Invoke();
            GameManager.Instance?.NotifyPlayerDead();
        }
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0 || CurrentHealth >= maxHealth) return;

        int previousHealth = CurrentHealth;
        int actualHeal = Mathf.Min(amount, maxHealth - CurrentHealth);
        CurrentHealth += actualHeal;

        OnHealed?.Invoke(actualHeal);
        NotifyHealthChanged(previousHealth);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReportHealth(CurrentHealth, maxHealth);
        }
    }

    public void SetMaxHealth(int newMaxHealth, bool fillToMax = false)
    {
        maxHealth = Mathf.Max(1, newMaxHealth);
        int previousHealth = CurrentHealth;
        CurrentHealth = fillToMax ? maxHealth : Mathf.Clamp(CurrentHealth, 0, maxHealth);

        NotifyHealthChanged(previousHealth);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReportHealth(CurrentHealth, maxHealth);
        }
    }

    private void SetHealth(int value, bool notifyManager)
    {
        int previousHealth = CurrentHealth;
        CurrentHealth = Mathf.Clamp(value, 0, maxHealth);
        NotifyHealthChanged(previousHealth);

        if (notifyManager && GameManager.Instance != null)
        {
            GameManager.Instance.ReportHealth(CurrentHealth, maxHealth);
        }
    }

    private void NotifyHealthChanged(int previousHealth)
    {
        if (CurrentHealth == previousHealth) return;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        OnHealthNormalizedChanged?.Invoke(NormalizedHealth);
    }
}
