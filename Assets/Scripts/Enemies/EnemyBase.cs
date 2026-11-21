using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Enemy Data")]
    [SerializeField] protected EnemyData enemyData;
    [Header("Stomp Settings")]
    [SerializeField] private float bounceForce = 8f;
    [SerializeField] private float stompTolerance = 0.15f;

    protected Rigidbody2D rb;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;

    protected bool isDead;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || !collision.gameObject.CompareTag("Player"))
            return;

        TryStompOrDamage(collision);
    }
    private void TryStompOrDamage(Collision2D collision)
    {
        ContactPoint2D contact = collision.GetContact(0);

        Rigidbody2D playerRb = collision.rigidbody;
        Collider2D playerCollider = collision.collider;

        if (playerRb == null || playerCollider == null)
            return;

        float playerBottom = playerCollider.bounds.min.y;

        bool falling = playerRb.linearVelocity.y <= 0f;
        bool touchingFromAbove = contact.normal.y < -0.5f;
        bool aboveEnemy = playerBottom >= transform.position.y - stompTolerance;

        if (falling && touchingFromAbove && aboveEnemy)
        {
            Die();

            playerRb.linearVelocity = new Vector2(
                playerRb.linearVelocity.x,
                bounceForce
            );

            return;
        }

        Inmune immunity = collision.gameObject.GetComponent<Inmune>();

        if (immunity != null)
        {
            int damage = enemyData != null ? enemyData.Damage : 10;
            immunity.TryTakeDamage(damage);
        }
    }

    protected virtual void Die()
    {
        if (isDead) return;

        isDead = true;

        if (GameManager.Instance != null && enemyData != null)
        {
            GameManager.Instance.AddScore(enemyData.ScoreValue);
        }
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        Collider2D enemyCollider = GetComponent<Collider2D>();

        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }
        if (animator != null)
        {
            animator.Play("Mushroom_die");
            float animationDuration = animator.GetCurrentAnimatorStateInfo(0).length;

            Destroy(gameObject, animationDuration);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
