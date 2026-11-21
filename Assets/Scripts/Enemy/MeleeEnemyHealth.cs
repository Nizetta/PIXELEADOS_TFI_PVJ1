using System;
using UnityEngine;

public class MeleeEnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private MeleeEnemyStats stats;

    [Header("Fallback Settings (If Stats is not assigned)")]
    [SerializeField] private int fallbackMaxHealth = 60;
    [SerializeField] private float fallbackDeathDestroyDelay = 0.8f;
    [SerializeField] private Vector2 fallbackKnockbackForce = new Vector2(4f, 2.5f);
    [SerializeField] private float fallbackHurtDuration = 0.25f;
  
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private MeleeEnemyController enemyController;

    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    public int MaxHealth => stats != null ? stats.MaxHealth : fallbackMaxHealth;
    public float DeathDestroyDelay => stats != null ? stats.DeathDestroyDelay : fallbackDeathDestroyDelay;
    public Vector2 KnockbackForce => stats != null ? stats.KnockbackForce : fallbackKnockbackForce;
    public float HurtDuration => stats != null ? stats.HurtDuration : fallbackHurtDuration;

    public event Action<Transform> OnDamaged;
    public event Action OnDied;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (enemyController == null)
        {
            enemyController = GetComponent<MeleeEnemyController>();
        }

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

        ApplyKnockback(attacker);
        OnDamaged?.Invoke(attacker);
    }

    private void ApplyKnockback(Transform attacker)
    {
        if (rb == null) return;

        float dir;
        if (attacker != null)
        {
            dir = transform.position.x >= attacker.position.x ? 1f : -1f;
        }
        else
        {
            dir = transform.localScale.x >= 0f ? -1f : 1f;
        }

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(dir * KnockbackForce.x, KnockbackForce.y), ForceMode2D.Impulse);
    }

    private void Die()
    {
        OnDied?.Invoke();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        Destroy(gameObject, DeathDestroyDelay);
    }
}
