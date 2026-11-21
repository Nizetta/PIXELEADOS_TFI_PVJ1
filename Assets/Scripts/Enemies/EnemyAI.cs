using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyType{Mushroom,Mosquito,Bat}

    [Header("Enemy Type")]
    [SerializeField] private EnemyType enemyType;
    [Header("Enemy Data")]
    [SerializeField] private EnemyData enemyData;
    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [Header("Patrol Points")]
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;
    [Header("Stomp Settings")]
    [SerializeField] private float bounceForce = 8f;
    [SerializeField] private float stompTolerance = 0.15f;
    [Header("Flight Settings")]
    [SerializeField] private float verticalAmplitude = 0.3f;
    [SerializeField] private float verticalFrequency = 2f;
    [SerializeField] private float stopDistance = 3f;
    [Header("Ranged Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackCooldown = 2f;
    [Header("Attack Animation")]
    [SerializeField] private float chargeDuration = 0.6f;
    [SerializeField] private float attackDuration = 0.4f;

    private bool isAttacking;

    private float nextAttackTime;
    private Transform player;
    [Header("Physics")]
    private Rigidbody2D rb;
    [Header("Animations")]
    [SerializeField] private string deathAnimationName = "Mushroom_die";
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private int currentPoint;
    private bool isDead;
    private bool movingRight = false;
    private float initialY;
    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        initialY = transform.position.y;

        if (enemyData == null)
        {
            Debug.LogError("EnemyData no está asignado en " + gameObject.name);
        }
    }
    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("No se encontró un objeto con Tag Player.");
        }
    }
    private void FixedUpdate()
    {
        if (isDead) return;

        switch (enemyType)
        {
            case EnemyType.Mushroom:
                MoveMushroom();
                break;

            case EnemyType.Mosquito:
                MoveMosquito();
                DetectAndAttackPlayer();
                break;

            case EnemyType.Bat:
                break;
        }
    }

    private void MoveMushroom()
    {
        if (leftPoint == null || rightPoint == null) return;

        float direction = movingRight ? 1f : -1f;

        float movementSpeed = enemyData != null ? enemyData.MovementSpeed: speed;

        rb.linearVelocity = new Vector2(direction * movementSpeed, rb.linearVelocity.y);
        if (!movingRight && rb.position.x <= leftPoint.position.x)
        {
            movingRight = true;
        }
        else if (movingRight && rb.position.x >= rightPoint.position.x)
        {
            movingRight = false;
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = movingRight;
        }
    }
    private void MoveMosquito()
    {
        if (leftPoint == null || rightPoint == null) return;

        float movementSpeed = enemyData != null ? enemyData.MovementSpeed : speed;
        bool playerDetected = player != null && enemyData != null && Vector2.Distance(transform.position, player.position) <= enemyData.DetectionRadius;

        if (playerDetected)
        {
            FacePlayer();

            float distance = Vector2.Distance(transform.position, player.position);
            if (distance > stopDistance)
            {
                Vector2 direction = ((Vector2)player.position - rb.position).normalized;
                rb.MovePosition(rb.position + direction * movementSpeed * Time.fixedDeltaTime);
            }
        }
        else
        {
            float direction = movingRight ? 1f : -1f;

            Vector2 newPosition = rb.position;
            newPosition.x += direction * movementSpeed * Time.fixedDeltaTime;

            float verticalMovement = Mathf.Sin(Time.time * verticalFrequency) * verticalAmplitude;
            newPosition.y = initialY + verticalMovement;
            rb.MovePosition(newPosition);
            if (!movingRight && rb.position.x <= leftPoint.position.x) movingRight = true;

            else if (movingRight && rb.position.x >= rightPoint.position.x) movingRight = false;

            if (spriteRenderer != null) spriteRenderer.flipX = movingRight;

            if (firePoint != null)
            {
                Vector3 position = firePoint.localPosition;
                position.x = Mathf.Abs(position.x) * (movingRight ? 1f : -1f);
                firePoint.localPosition = position;
            }
        }
    }
    private void FacePlayer()
    {
        if (player == null) return;

        movingRight = player.position.x > transform.position.x;

        if (spriteRenderer != null) spriteRenderer.flipX = movingRight;

        if (firePoint != null)
        {
            Vector3 position = firePoint.localPosition;
            position.x = Mathf.Abs(position.x) * (movingRight ? 1f : -1f);
            firePoint.localPosition = position;
        }
    }
    private void DetectAndAttackPlayer()
    {
        if (player == null || firePoint == null || projectilePrefab == null || enemyData == null) return;
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= enemyData.DetectionRadius && Time.time >= nextAttackTime && !isAttacking)
        {
            FacePlayer();
            StartCoroutine(AttackPlayer());
            nextAttackTime = Time.time + attackCooldown;
        }
    }
    private IEnumerator AttackPlayer()
    {
        isAttacking = true;
        if (animator != null) animator.Play("Mosquito_charge");

        yield return new WaitForSeconds(chargeDuration);
        if (animator != null) animator.Play("Mosquito_attack");
        
        ShootProjectile();

        yield return new WaitForSeconds(attackDuration);

        if (animator != null) animator.Play("Mosquito_flyidle");

        isAttacking = false;
    }
    private void ShootProjectile()
    {
        GameObject projectileObject = Instantiate(projectilePrefab,firePoint.position,Quaternion.identity);

        Projectiles projectile = projectileObject.GetComponent<Projectiles>();

        if (projectile != null) projectile.SetDirection(player.position);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || !collision.gameObject.CompareTag("Player")) return;
        TryStompOrDamage(collision);
    }

    private void TryStompOrDamage(Collision2D collision)
    {
        ContactPoint2D contact = collision.GetContact(0);

        Rigidbody2D playerRb = collision.rigidbody;
        Collider2D playerCollider = collision.collider;

        if (playerRb == null || playerCollider == null) return;

        float playerBottom = playerCollider.bounds.min.y;

        bool falling = playerRb.linearVelocity.y <= 0f;
        bool touchingFromAbove = contact.normal.y < -0.5f;
        bool aboveEnemy = playerBottom >= transform.position.y - stompTolerance;

        if (falling && touchingFromAbove && aboveEnemy)
        {
            Die();
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, bounceForce);
            return;
        }

        Inmune immunity = collision.gameObject.GetComponent<Inmune>();

        if (immunity != null && enemyData != null) immunity.TryTakeDamage(enemyData.Damage);
    }
    private void Die()
    {
        if (isDead) return;

        isDead = true;

        if (GameManager.Instance != null && enemyData != null) GameManager.Instance.AddScore(enemyData.ScoreValue);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Collider2D enemyCollider = GetComponent<Collider2D>();

        if (enemyCollider != null) enemyCollider.enabled = false;
 
        if (animator != null)
        {
            animator.Play(deathAnimationName);
            float animationDuration = animator.GetCurrentAnimatorStateInfo(0).length;
            Destroy(gameObject, animationDuration);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (enemyData == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyData.DetectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.AttackRadius);
    }
}
