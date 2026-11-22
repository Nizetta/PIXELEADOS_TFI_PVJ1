using System;
using UnityEngine;

public class FlyingEnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private FlyingEnemyStats stats;

    [Header("Fallback Settings")]
    [SerializeField] private int fallbackMaxHealth = 45;
    [SerializeField] private float fallbackDeathDestroyDelay = 0.5f;
    [SerializeField] private Vector2 fallbackKnockbackForce = new Vector2(2.5f, 1.5f);
    [SerializeField] private float fallbackHurtFlashDuration = 0.15f;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    public int MaxHealth => stats != null ? stats.MaxHealth : fallbackMaxHealth;
    public float DeathDestroyDelay => stats != null ? stats.DeathDestroyDelay : fallbackDeathDestroyDelay;
    public Vector2 KnockbackForce => stats != null ? stats.KnockbackForce : fallbackKnockbackForce;
    public float HurtFlashDuration => stats != null ? stats.HurtFlashDuration : fallbackHurtFlashDuration;

    public event Action<Transform> OnDamaged;
    public event Action OnDied;

    private static readonly int HashHurt = Animator.StringToHash("hurt");
    private static readonly int HashDead = Animator.StringToHash("dead");

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();

        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(int amount, Transform attacker = null)
    {
        if (IsDead || amount <= 0) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        AudioManager.Instance?.PlayHitEnemy(transform);

        if (CurrentHealth <= 0)
        {
            Die();
            return;
        }

        if (animator != null)
        {
            animator.SetTrigger(HashHurt);
        }

        OnDamaged?.Invoke(attacker);
    }

    private void Die()
    {
        OnDied?.Invoke();

        if (animator != null)
        {
            animator.SetTrigger(HashDead);
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        Destroy(gameObject, DeathDestroyDelay);
    }
}
