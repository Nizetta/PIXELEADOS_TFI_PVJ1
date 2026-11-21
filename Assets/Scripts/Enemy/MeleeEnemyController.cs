using UnityEngine;

public class MeleeEnemyController : MonoBehaviour
{
    public enum PatrolMode
    {
        TwoPoints,
        WallOrEdgeTurn
    }

    [Header("Stats & References")]
    [SerializeField] private MeleeEnemyStats stats;
    [SerializeField] private Animator animator;

    [Header("Patrol Mode & Scene Points")]
    [SerializeField] private PatrolMode patrolMode = PatrolMode.TwoPoints;
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Wall / Edge Checks")]
    [SerializeField] private Transform wallCheckPoint;
    [SerializeField] private Transform edgeCheckPoint;
    [SerializeField] private LayerMask groundLayer;

    [Header("Combat & Detection")]
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private LayerMask playerLayer;

    [Header("Fallback Settings (If Stats is not assigned)")]
    [SerializeField] private float fallbackMoveSpeed = 2.5f;
    [SerializeField] private float fallbackIdleDuration = 1.0f;
    [SerializeField] private float fallbackWaypointReachThreshold = 0.15f;
    [SerializeField] private float fallbackWallCheckDistance = 0.6f;
    [SerializeField] private float fallbackEdgeCheckDistance = 0.8f;
    [SerializeField] private float fallbackDetectionRadius = 3.5f;
    [SerializeField] private float fallbackAttackRange = 1.1f;
    [SerializeField] private int fallbackAttackDamage = 15;
    [SerializeField] private float fallbackAttackCooldown = 1.2f;
    [SerializeField] private float fallbackAttackDuration = 0.45f;

    public EnemyState CurrentState { get; private set; } = EnemyState.Idle;

    private float MoveSpeed => stats != null ? stats.MoveSpeed : fallbackMoveSpeed;
    private float IdleDuration => stats != null ? stats.IdleDuration : fallbackIdleDuration;
    private float WaypointReachThreshold => stats != null ? stats.WaypointReachThreshold : fallbackWaypointReachThreshold;
    private float WallCheckDistance => stats != null ? stats.WallCheckDistance : fallbackWallCheckDistance;
    private float EdgeCheckDistance => stats != null ? stats.EdgeCheckDistance : fallbackEdgeCheckDistance;
    private float DetectionRadius => stats != null ? stats.DetectionRadius : fallbackDetectionRadius;
    private float AttackRange => stats != null ? stats.AttackRange : fallbackAttackRange;
    private int AttackDamage => stats != null ? stats.AttackDamage : fallbackAttackDamage;
    private float AttackCooldown => stats != null ? stats.AttackCooldown : fallbackAttackCooldown;
    private float AttackDuration => stats != null ? stats.AttackDuration : fallbackAttackDuration;

    private Rigidbody2D rb;
    private MeleeEnemyHealth health;

    private Transform targetWaypoint;
    private Transform detectedPlayer;
    private int facingDirection = 1; // 1 = derecha, -1 = izquierda
    private float stateTimer;
    private float attackCooldownTimer;
    private bool hasHitInCurrentAttack;

    // Animator parameters
    private static readonly int HashState = Animator.StringToHash("state");
    private static readonly int HashSpeed = Animator.StringToHash("speed");
    private static readonly int HashAttackTrigger = Animator.StringToHash("attack");
    private static readonly int HashHurtTrigger = Animator.StringToHash("hurt");
    private static readonly int HashDeadTrigger = Animator.StringToHash("dead");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<MeleeEnemyHealth>();
        if (animator == null) animator = GetComponent<Animator>();

        facingDirection = transform.localScale.x >= 0f ? 1 : -1;

        if (pointA != null && pointB != null)
        {
            targetWaypoint = pointB;
        }
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

    private void Start()
    {
        ChangeState(EnemyState.Walk);
    }

    private void Update()
    {
        if (CurrentState == EnemyState.Dead)
        {
            UpdateAnimator();
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            UpdateAnimator();
            return;
        }

        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        if (CurrentState == EnemyState.Hurt)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                ChangeState(EnemyState.Walk);
            }
            UpdateAnimator();
            return;
        }

        if (CurrentState == EnemyState.Attack)
        {
            stateTimer -= Time.deltaTime;

            if (!hasHitInCurrentAttack && stateTimer <= AttackDuration * 0.5f)
            {
                DealMeleeDamage();
                hasHitInCurrentAttack = true;
            }

            if (stateTimer <= 0f)
            {
                ChangeState(EnemyState.Idle);
                stateTimer = 0.2f;
            }

            UpdateAnimator();
            return;
        }

        DetectPlayer();

        if (detectedPlayer != null)
        {
            FaceTarget(detectedPlayer.position.x);
            float distanceToPlayer = Vector2.Distance(transform.position, detectedPlayer.position);

            if (distanceToPlayer <= AttackRange && attackCooldownTimer <= 0f)
            {
                StartAttack();
                UpdateAnimator();
                return;
            }
        }

        if (CurrentState == EnemyState.Idle)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                ChangeState(EnemyState.Walk);
            }
        }

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (CurrentState == EnemyState.Dead)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (CurrentState == EnemyState.Hurt || CurrentState == EnemyState.Attack || CurrentState == EnemyState.Idle)
        {
            if (CurrentState != EnemyState.Hurt)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            return;
        }

        // Estado Walk
        if (detectedPlayer != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, detectedPlayer.position);
            if (distanceToPlayer > AttackRange * 0.85f)
            {
                FaceTarget(detectedPlayer.position.x);
                rb.linearVelocity = new Vector2(facingDirection * MoveSpeed, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            return;
        }

        if (patrolMode == PatrolMode.TwoPoints && pointA != null && pointB != null)
        {
            PatrolBetweenPoints();
        }
        else
        {
            PatrolWallOrEdge();
        }
    }

    private void PatrolBetweenPoints()
    {
        if (targetWaypoint == null) targetWaypoint = pointB;

        FaceTarget(targetWaypoint.position.x);
        rb.linearVelocity = new Vector2(facingDirection * MoveSpeed, rb.linearVelocity.y);

        if (Mathf.Abs(transform.position.x - targetWaypoint.position.x) <= WaypointReachThreshold)
        {
            targetWaypoint = targetWaypoint == pointA ? pointB : pointA;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            ChangeState(EnemyState.Idle);
            stateTimer = IdleDuration;
        }
    }

    private void PatrolWallOrEdge()
    {
        bool hitWall = wallCheckPoint != null &&
                       Physics2D.Raycast(wallCheckPoint.position, Vector2.right * facingDirection, WallCheckDistance, groundLayer);

        bool noEdge = edgeCheckPoint != null &&
                      !Physics2D.Raycast(edgeCheckPoint.position, Vector2.down, EdgeCheckDistance, groundLayer);

        if (hitWall || noEdge)
        {
            Flip();
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            ChangeState(EnemyState.Idle);
            stateTimer = IdleDuration;
            return;
        }

        rb.linearVelocity = new Vector2(facingDirection * MoveSpeed, rb.linearVelocity.y);
    }

    private void DetectPlayer()
    {
        Vector2 origin = attackOrigin != null ? (Vector2)attackOrigin.position : (Vector2)transform.position;
        Collider2D hit = Physics2D.OverlapCircle(origin, DetectionRadius, playerLayer);
        detectedPlayer = hit != null ? hit.transform : null;
    }

    private void StartAttack()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        attackCooldownTimer = AttackCooldown;
        stateTimer = AttackDuration;
        hasHitInCurrentAttack = false;
        ChangeState(EnemyState.Attack);

        if (animator != null)
        {
            animator.SetTrigger(HashAttackTrigger);
        }
    }

    public void DealMeleeDamage()
    {
        Vector2 origin = attackOrigin != null ? (Vector2)attackOrigin.position : (Vector2)transform.position;
        Collider2D[] targets = Physics2D.OverlapCircleAll(origin, AttackRange, playerLayer);

        foreach (Collider2D target in targets)
        {
            if (target == null) continue;

            HealthSystem playerHealth = target.GetComponent<HealthSystem>() ?? target.GetComponentInParent<HealthSystem>();
            if (playerHealth != null && !playerHealth.IsDead)
            {
                playerHealth.TakeDamage(AttackDamage);
            }
        }
    }

    private void HandleDamaged(Transform attacker)
    {
        if (CurrentState == EnemyState.Dead) return;

        stateTimer = health != null ? health.HurtDuration : 0.25f;
        ChangeState(EnemyState.Hurt);

        if (animator != null)
        {
            animator.SetTrigger(HashHurtTrigger);
        }
    }

    private void HandleDied()
    {
        ChangeState(EnemyState.Dead);
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (animator != null)
        {
            animator.SetTrigger(HashDeadTrigger);
        }
    }

    private void FaceTarget(float targetX)
    {
        int desiredDir = targetX >= transform.position.x ? 1 : -1;
        if (desiredDir != facingDirection)
        {
            SetFacingDirection(desiredDir);
        }
    }

    private void Flip()
    {
        SetFacingDirection(-facingDirection);
    }

    private void SetFacingDirection(int newDir)
    {
        facingDirection = newDir;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * facingDirection;
        transform.localScale = scale;
    }

    private void ChangeState(EnemyState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetInteger(HashState, (int)CurrentState);
        animator.SetFloat(HashSpeed, Mathf.Abs(rb.linearVelocity.x));
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 origin = attackOrigin != null ? (Vector2)attackOrigin.position : (Vector2)transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, DetectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, AttackRange);

        int dir = Application.isPlaying ? facingDirection : (transform.localScale.x >= 0f ? 1 : -1);

        if (wallCheckPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wallCheckPoint.position, wallCheckPoint.position + Vector3.right * dir * WallCheckDistance);
        }

        if (edgeCheckPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(edgeCheckPoint.position, edgeCheckPoint.position + Vector3.down * EdgeCheckDistance);
        }
    }
}
