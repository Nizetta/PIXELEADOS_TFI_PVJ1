using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Configuration")]
    [SerializeField] private EnemyData enemyData;
    [Header("Movement")]
    [SerializeField] private Transform[] patrolPoints;
    [Header("Physics")]
    [SerializeField] private Rigidbody2D rb;
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private int currentPoint;
    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void FixedUpdate()
    {
        if (patrolPoints.Length == 0 || enemyData == null) return;

        Transform target = patrolPoints[currentPoint];
        float direction = target.position.x - rb.position.x;

        if (direction != 0) spriteRenderer.flipX = direction > 0;

        Vector2 targetPosition = new Vector2(target.position.x, rb.position.y);
        Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, enemyData.MovementSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);
        bool isMoving = Mathf.Abs(target.position.x - rb.position.x) > 0.1f;

        if (animator != null) animator.SetBool("IsMoving", isMoving);

        if (!isMoving)
        {
            currentPoint++;

            if (currentPoint >= patrolPoints.Length) currentPoint = 0;
        }
    }
}
