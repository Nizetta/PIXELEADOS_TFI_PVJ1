using UnityEngine;
using System.Collections;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Fallback Settings")]
    [SerializeField] private float fallbackSpeed = 6f;
    [SerializeField] private float fallbackLifeTime = 3.5f;
    [SerializeField] private int fallbackDamage = 15;
    [SerializeField] private float hitDestroyDelay = 0.25f;

    [Header("Optional Visuals")]
    [SerializeField] private Animator animator;

    private Vector2 moveDirection = Vector2.down;
    private float speed;
    private float lifeTime;
    private int damage;
    private bool hasHit;
    private Collider2D col;
    private Rigidbody2D rb;

    private static readonly int HashHit = Animator.StringToHash("Hit");

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();

        speed = fallbackSpeed;
        lifeTime = fallbackLifeTime;
        damage = fallbackDamage;
    }

    public void Initialize(Vector2 direction, FlyingEnemyStats stats)
    {
        moveDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.down;

        if (stats != null)
        {
            speed = stats.ProjectileSpeed;
            lifeTime = stats.ProjectileLifeTime;
            damage = stats.ProjectileDamage;
        }

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Start()
    {
        StartCoroutine(LifeTimeRoutine());
    }

    private void Update()
    {
        if (hasHit) return;

        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerController>() != null)
        {
            HealthSystem playerHealth = other.GetComponent<HealthSystem>() ?? other.GetComponentInParent<HealthSystem>();
            if (playerHealth != null && !playerHealth.IsDead)
            {
                playerHealth.TakeDamage(damage);
            }
            TriggerHit();
            return;
        }

        // Si choca con el suelo u obstáculos (y no con otro enemigo o trigger no sólido)
        if (!other.isTrigger && other.GetComponentInParent<FlyingEnemyController>() == null && other.GetComponentInParent<MeleeEnemyController>() == null)
        {
            TriggerHit();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return;

        if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponentInParent<PlayerController>() != null)
        {
            HealthSystem playerHealth = collision.gameObject.GetComponent<HealthSystem>() ?? collision.gameObject.GetComponentInParent<HealthSystem>();
            if (playerHealth != null && !playerHealth.IsDead)
            {
                playerHealth.TakeDamage(damage);
            }
        }

        TriggerHit();
    }

    private void TriggerHit()
    {
        hasHit = true;
        speed = 0f;

        if (col != null)
        {
            col.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (animator != null)
        {
            animator.SetTrigger(HashHit);
        }

        StartCoroutine(DestroyAfterHit());
    }

    private IEnumerator DestroyAfterHit()
    {
        yield return new WaitForSeconds(hitDestroyDelay);
        Destroy(gameObject);
    }

    private IEnumerator LifeTimeRoutine()
    {
        yield return new WaitForSeconds(lifeTime);
        if (!hasHit)
        {
            Destroy(gameObject);
        }
    }
}
