using UnityEngine;

public class FlyingEnemyController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private FlyingEnemyStats stats;
    [SerializeField] private Transform firePoint;
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    private static readonly int HashShoot = Animator.StringToHash("shoot");

    [Header("Fallback Movement & Shooting")]
    [SerializeField] private float fallbackFrequency = 2f;
    [SerializeField] private float fallbackAmplitude = 2.5f;
    [SerializeField] private float fallbackVerticalBobAmplitude = 0.25f;
    [SerializeField] private float fallbackVerticalBobFrequency = 3f;
    [SerializeField] private ProjectileAimDirection fallbackAimDirection = ProjectileAimDirection.Down;
    [SerializeField] private float fallbackShootInterval = 2.0f;
    [SerializeField] private float fallbackInitialDelay = 0.5f;

    private FlyingEnemyHealth health;
    private Vector3 initPosition;
    private float shootTimer;
    private float hurtKnockbackTimer;
    private Vector2 knockbackOffset;
    private int facingDirection = 1; // 1 = derecha, -1 = izquierda

    private float Frequency => stats != null ? stats.Frequency : fallbackFrequency;
    private float Amplitude => stats != null ? stats.Amplitude : fallbackAmplitude;
    private float VerticalBobAmplitude => stats != null ? stats.VerticalBobAmplitude : fallbackVerticalBobAmplitude;
    private float VerticalBobFrequency => stats != null ? stats.VerticalBobFrequency : fallbackVerticalBobFrequency;
    private ProjectileAimDirection AimDirection => stats != null ? stats.AimDirection : fallbackAimDirection;
    private float ShootInterval => stats != null ? stats.ShootInterval : fallbackShootInterval;
    private float InitialDelay => stats != null ? stats.InitialDelay : fallbackInitialDelay;

    private void Awake()
    {
        health = GetComponent<FlyingEnemyHealth>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        initPosition = transform.position;
        shootTimer = InitialDelay;
        if (animator == null) animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDamaged += HandleDamaged;
            health.OnDied += HandleDied;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDamaged -= HandleDamaged;
            health.OnDied -= HandleDied;
        }
    }

    private void Update()
    {
        if (health != null && health.IsDead) return;

        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

        UpdateFlightPosition();
        UpdateShooting();
    }

    private void UpdateFlightPosition()
    {
        float avanceX = Mathf.Sin(Time.time * Frequency) * Amplitude;
        float avanceY = Mathf.Sin(Time.time * VerticalBobFrequency) * VerticalBobAmplitude;

        float cosDir = Mathf.Cos(Time.time * Frequency);
        facingDirection = cosDir >= 0f ? 1 : -1;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = facingDirection < 0;
        }

        if (hurtKnockbackTimer > 0f)
        {
            hurtKnockbackTimer -= Time.deltaTime;
            knockbackOffset = Vector2.Lerp(knockbackOffset, Vector2.zero, Time.deltaTime * 10f);
        }
        else
        {
            knockbackOffset = Vector2.zero;
        }

        transform.position = new Vector3(
            initPosition.x + avanceX + knockbackOffset.x,
            initPosition.y + avanceY + knockbackOffset.y,
            initPosition.z
        );
    }

    private void UpdateShooting()
    {
        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f)
        {
            shootTimer = ShootInterval;
            ShootProjectile();
        }
    }

    private void ShootProjectile()
    {
        if (projectilePrefab == null) return;
        if (animator != null)
        {
            animator.SetTrigger(HashShoot);
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector2 dir = AimDirection == ProjectileAimDirection.Down
            ? Vector2.down
            : Vector2.right * facingDirection;

        EnemyProjectile proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        proj.Initialize(dir, stats);
    }

    private void HandleDamaged(Transform attacker)
    {
        float dirX = 1f;
        if (attacker != null)
        {
            dirX = transform.position.x >= attacker.position.x ? 1f : -1f;
        }
        else
        {
            dirX = -facingDirection;
        }

        Vector2 kb = health != null ? health.KnockbackForce : new Vector2(2.5f, 1.5f);
        knockbackOffset = new Vector2(dirX * kb.x * 0.25f, kb.y * 0.25f);
        hurtKnockbackTimer = health != null ? health.HurtFlashDuration : 0.15f;
    }

    private void HandleDied()
    {
        // Caida de monstruo
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 basePos = Application.isPlaying ? initPosition : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(basePos, new Vector3(Amplitude * 2f, VerticalBobAmplitude * 2f + 0.2f, 0.1f));

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        Vector2 dir = AimDirection == ProjectileAimDirection.Down
            ? Vector2.down
            : Vector2.right * (Application.isPlaying ? facingDirection : 1);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin, (Vector3)dir * 1.5f);
    }
}
