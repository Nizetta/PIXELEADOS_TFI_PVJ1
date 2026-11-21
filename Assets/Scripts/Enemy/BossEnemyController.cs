using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemyController : MonoBehaviour
{
    private enum BossState
    {
        IdleAtPost,
        Chase,
        Attack,
        Hurt,
        Teleporting,
        ReturningHome,
        Dead
    }

    private enum AttackVariant
    {
        Light,
        Heavy
    }

    [Header("Stats & References")]
    [SerializeField] private BossEnemyStats stats;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private LayerMask playerLayer;

    [Header("Portal Teleport (Using Boss Own Sprite/Animator)")]
    [SerializeField] private Transform[] teleportPoints;

    [Header("Fallback Settings")]
    [Min(0f)][SerializeField] private float fallbackDetectionRadius = 7f;
    [Min(0f)][SerializeField] private float fallbackLoseTargetRadius = 10f;
    [Min(0f)][SerializeField] private float fallbackChaseSpeed = 3.5f;
    [SerializeField] private bool fallbackReturnHomeWhenLost = true;
    [Min(0.05f)][SerializeField] private float fallbackHomeReachThreshold = 0.2f;

    [Min(0f)][SerializeField] private float fallbackLightAttackRange = 1.4f;
    [Min(1)][SerializeField] private int fallbackLightAttackDamage = 15;
    [Min(0.05f)][SerializeField] private float fallbackLightAttackDuration = 0.4f;

    [Min(0f)][SerializeField] private float fallbackHeavyAttackRange = 2.2f;
    [Min(1)][SerializeField] private int fallbackHeavyAttackDamage = 30;
    [Min(0.05f)][SerializeField] private float fallbackHeavyAttackDuration = 0.7f;

    [Min(0.1f)][SerializeField] private float fallbackAttackCooldown = 1.3f;

    [SerializeField] private bool fallbackCanTeleport = true;
    [Min(0.5f)][SerializeField] private float fallbackTeleportCooldown = 4.5f;
    [Min(0.05f)][SerializeField] private float fallbackVanishDuration = 0.35f;
    [Min(0.05f)][SerializeField] private float fallbackReappearDuration = 0.35f;
    [SerializeField] private float fallbackTeleportOffsetFromPlayer = 2.0f;

    private float DetectionRadius => stats != null ? stats.DetectionRadius : fallbackDetectionRadius;
    private float LoseTargetRadius => stats != null ? stats.LoseTargetRadius : Mathf.Max(fallbackLoseTargetRadius, fallbackDetectionRadius);
    private float ChaseSpeed => stats != null ? stats.ChaseSpeed : fallbackChaseSpeed;
    private bool ReturnHomeWhenLost => stats != null ? stats.ReturnHomeWhenLost : fallbackReturnHomeWhenLost;
    private float HomeReachThreshold => stats != null ? stats.HomeReachThreshold : fallbackHomeReachThreshold;

    private float LightAttackRange => stats != null ? stats.LightAttackRange : fallbackLightAttackRange;
    private int LightAttackDamage => stats != null ? stats.LightAttackDamage : fallbackLightAttackDamage;
    private float LightAttackDuration => stats != null ? stats.LightAttackDuration : fallbackLightAttackDuration;

    private float HeavyAttackRange => stats != null ? stats.HeavyAttackRange : fallbackHeavyAttackRange;
    private int HeavyAttackDamage => stats != null ? stats.HeavyAttackDamage : fallbackHeavyAttackDamage;
    private float HeavyAttackDuration => stats != null ? stats.HeavyAttackDuration : fallbackHeavyAttackDuration;

    private float AttackCooldown => stats != null ? stats.AttackCooldown : fallbackAttackCooldown;

    private bool CanTeleport => stats != null ? stats.CanTeleport : fallbackCanTeleport;
    private float TeleportCooldown => stats != null ? stats.TeleportCooldown : fallbackTeleportCooldown;
    private float VanishDuration => stats != null ? stats.VanishDuration : fallbackVanishDuration;
    private float ReappearDuration => stats != null ? stats.ReappearDuration : fallbackReappearDuration;
    private float TeleportOffsetFromPlayer => stats != null ? stats.TeleportOffsetFromPlayer : fallbackTeleportOffsetFromPlayer;

    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private BossEnemyHealth health;
    private BossState currentState = BossState.IdleAtPost;
    private AttackVariant currentAttackVariant = AttackVariant.Light;

    private Vector3 homePosition;
    private Transform targetPlayer;
    private int facingDirection = 1;
    private float stateTimer;
    private float attackCooldownTimer;
    private float teleportCooldownTimer;
    private bool hasHitInCurrentAttack;
    private bool nextAttackIsHeavy;
    private Coroutine teleportCoroutine;

    private static readonly int HashSpeed = Animator.StringToHash("speed");
    private static readonly int HashIsChasing = Animator.StringToHash("isChasing");
    private static readonly int HashAttackLight = Animator.StringToHash("attackLight");
    private static readonly int HashAttackHeavy = Animator.StringToHash("attackHeavy");
    private static readonly int HashHurt = Animator.StringToHash("hurt");
    private static readonly int HashDead = Animator.StringToHash("dead");
    private static readonly int HashPortalOut = Animator.StringToHash("portalOut");
    private static readonly int HashPortalIn = Animator.StringToHash("portalIn");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        health = GetComponent<BossEnemyHealth>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        homePosition = transform.position;
        facingDirection = transform.localScale.x >= 0f ? 1 : -1;
        teleportCooldownTimer = TeleportCooldown;
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
        if (currentState == BossState.Dead)
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

        if (teleportCooldownTimer > 0f)
        {
            teleportCooldownTimer -= Time.deltaTime;
        }

        if (currentState == BossState.Teleporting)
        {
            UpdateAnimator();
            return;
        }

        if (currentState == BossState.Hurt)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                currentState = targetPlayer != null ? BossState.Chase : BossState.IdleAtPost;
            }
            UpdateAnimator();
            return;
        }

        if (currentState == BossState.Attack)
        {
            stateTimer -= Time.deltaTime;
            float currentDuration = GetCurrentAttackDuration();

            if (!hasHitInCurrentAttack && stateTimer <= currentDuration * 0.5f)
            {
                DealCurrentAttackDamage();
                hasHitInCurrentAttack = true;
            }

            if (stateTimer <= 0f)
            {
                currentState = targetPlayer != null ? BossState.Chase : BossState.IdleAtPost;
            }

            UpdateAnimator();
            return;
        }

        UpdateTargetDetection();

        if (targetPlayer != null)
        {
            FaceTarget(targetPlayer.position.x);
            float distance = Vector2.Distance(transform.position, targetPlayer.position);

            // Teletransportar cuando esté habilitado y pase el cooldown
            if (CanTeleport && teleportCooldownTimer <= 0f)
            {
                teleportCoroutine = StartCoroutine(TeleportRoutine());
                UpdateAnimator();
                return;
            }

            float activeRange = GetNextAttackRange();
            if (distance <= activeRange && attackCooldownTimer <= 0f)
            {
                StartCloseAttack();
                UpdateAnimator();
                return;
            }

            currentState = BossState.Chase;
        }
        else if (currentState == BossState.ReturningHome)
        {
            float distToHome = Mathf.Abs(transform.position.x - homePosition.x);
            if (distToHome <= HomeReachThreshold)
            {
                transform.position = new Vector3(homePosition.x, transform.position.y, transform.position.z);
                currentState = BossState.IdleAtPost;
            }
        }
        else
        {
            currentState = BossState.IdleAtPost;
        }

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (currentState == BossState.Dead || currentState == BossState.Teleporting)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (currentState == BossState.IdleAtPost || currentState == BossState.Attack || currentState == BossState.Hurt)
        {
            if (currentState != BossState.Hurt)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            return;
        }

        if (currentState == BossState.Chase && targetPlayer != null)
        {
            float distance = Vector2.Distance(transform.position, targetPlayer.position);
            float stopRange = GetNextAttackRange() * 0.85f;

            if (distance > stopRange)
            {
                FaceTarget(targetPlayer.position.x);
                rb.linearVelocity = new Vector2(facingDirection * ChaseSpeed, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            return;
        }

        if (currentState == BossState.ReturningHome)
        {
            FaceTarget(homePosition.x);
            rb.linearVelocity = new Vector2(facingDirection * ChaseSpeed, rb.linearVelocity.y);
        }
    }

    private void UpdateTargetDetection()
    {
        Vector2 origin = attackOrigin != null ? (Vector2)attackOrigin.position : (Vector2)transform.position;

        if (targetPlayer != null)
        {
            float dist = Vector2.Distance(origin, targetPlayer.position);
            HealthSystem hp = targetPlayer.GetComponent<HealthSystem>() ?? targetPlayer.GetComponentInParent<HealthSystem>();
            if (dist > LoseTargetRadius || (hp != null && hp.IsDead))
            {
                targetPlayer = null;
                if (ReturnHomeWhenLost)
                {
                    currentState = BossState.ReturningHome;
                }
            }
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(origin, DetectionRadius, playerLayer);
        if (hit != null)
        {
            Transform candidate = hit.transform;
            HealthSystem hp = candidate.GetComponent<HealthSystem>() ?? candidate.GetComponentInParent<HealthSystem>();
            if (hp == null || !hp.IsDead)
            {
                targetPlayer = candidate;
                currentState = BossState.Chase;
            }
        }
    }

    private float GetNextAttackRange()
    {
        return nextAttackIsHeavy ? HeavyAttackRange : LightAttackRange;
    }

    private float GetCurrentAttackRange()
    {
        return currentAttackVariant == AttackVariant.Heavy ? HeavyAttackRange : LightAttackRange;
    }

    private int GetCurrentAttackDamage()
    {
        return currentAttackVariant == AttackVariant.Heavy ? HeavyAttackDamage : LightAttackDamage;
    }

    private float GetCurrentAttackDuration()
    {
        return currentAttackVariant == AttackVariant.Heavy ? HeavyAttackDuration : LightAttackDuration;
    }

    private void StartCloseAttack()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        currentAttackVariant = nextAttackIsHeavy ? AttackVariant.Heavy : AttackVariant.Light;
        nextAttackIsHeavy = !nextAttackIsHeavy;

        attackCooldownTimer = AttackCooldown;
        stateTimer = GetCurrentAttackDuration();
        hasHitInCurrentAttack = false;
        currentState = BossState.Attack;

        if (animator != null)
        {
            animator.SetTrigger(currentAttackVariant == AttackVariant.Heavy ? HashAttackHeavy : HashAttackLight);
        }
    }

    public void DealCurrentAttackDamage()
    {
        Vector2 origin = attackOrigin != null ? (Vector2)attackOrigin.position : (Vector2)transform.position;
        float range = GetCurrentAttackRange();
        int dmg = GetCurrentAttackDamage();

        Collider2D[] targets = Physics2D.OverlapCircleAll(origin, range, playerLayer);
        HashSet<HealthSystem> hitPlayers = new HashSet<HealthSystem>();

        foreach (Collider2D target in targets)
        {
            if (target == null) continue;

            HealthSystem playerHealth = target.GetComponent<HealthSystem>() ?? target.GetComponentInParent<HealthSystem>();
            if (playerHealth != null && !playerHealth.IsDead && hitPlayers.Add(playerHealth))
            {
                playerHealth.TakeDamage(dmg);
            }
        }
    }

    private IEnumerator TeleportRoutine()
    {
        currentState = BossState.Teleporting;
        teleportCooldownTimer = TeleportCooldown;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (bodyCollider != null)
        {
            bodyCollider.enabled = false;
        }

        if (animator != null)
        {
            animator.SetTrigger(HashPortalOut);
        }

        yield return new WaitForSeconds(VanishDuration);

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        transform.position = CalculateTeleportDestination();

        if (targetPlayer != null)
        {
            FaceTarget(targetPlayer.position.x);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        if (animator != null)
        {
            animator.SetTrigger(HashPortalIn);
        }

        yield return new WaitForSeconds(ReappearDuration);

        if (bodyCollider != null)
        {
            bodyCollider.enabled = true;
        }

        teleportCoroutine = null;

        if (currentState != BossState.Dead)
        {
            currentState = targetPlayer != null ? BossState.Chase : BossState.IdleAtPost;
        }
    }

    private Vector3 CalculateTeleportDestination()
    {
        if (teleportPoints != null && teleportPoints.Length > 0)
        {
            Transform chosen = teleportPoints[Random.Range(0, teleportPoints.Length)];
            if (chosen != null)
            {
                return new Vector3(chosen.position.x, chosen.position.y, transform.position.z);
            }
        }

        if (targetPlayer != null)
        {
            float side = targetPlayer.localScale.x >= 0f ? -1f : 1f;
            float targetX = targetPlayer.position.x + (side * TeleportOffsetFromPlayer);
            return new Vector3(targetX, transform.position.y, transform.position.z);
        }

        return transform.position;
    }

    private void HandleDamaged(Transform attacker)
    {
        if (currentState == BossState.Dead || currentState == BossState.Teleporting) return;

        if (targetPlayer == null && attacker != null)
        {
            targetPlayer = attacker;
        }

        stateTimer = health != null ? health.HurtDuration : 0.2f;
        currentState = BossState.Hurt;

        if (animator != null)
        {
            animator.SetTrigger(HashHurt);
        }
    }

    private void HandleDied()
    {
        if (teleportCoroutine != null)
        {
            StopCoroutine(teleportCoroutine);
            teleportCoroutine = null;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        if (bodyCollider != null)
        {
            bodyCollider.enabled = false;
        }

        currentState = BossState.Dead;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (animator != null)
        {
            animator.SetTrigger(HashDead);
        }
    }

    private void FaceTarget(float targetX)
    {
        int desiredDir = targetX >= transform.position.x ? 1 : -1;
        if (desiredDir != facingDirection)
        {
            facingDirection = desiredDir;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * facingDirection;
            transform.localScale = scale;
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetFloat(HashSpeed, Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool(HashIsChasing, currentState == BossState.Chase || currentState == BossState.ReturningHome);
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 origin = attackOrigin != null ? (Vector2)attackOrigin.position : (Vector2)transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, DetectionRadius);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(origin, LoseTargetRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(origin, LightAttackRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, HeavyAttackRange);
    }
}
