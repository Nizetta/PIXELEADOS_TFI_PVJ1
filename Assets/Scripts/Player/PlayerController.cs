using System.Collections.Generic;
using UnityEngine;

public enum PlayerState {Idle, Run, Jump, Attack, Hurt}
public class PlayerController : MonoBehaviour
{
    [Header("Data & References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerClimb playerClimb;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Combat / Attack")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 0.7f;
    [SerializeField] private int attackDamage = 25;
    [SerializeField] private float attackDuration = 0.35f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Hurt / Knockback")]
    [SerializeField] private float hurtDuration = 0.25f;
    [SerializeField] private Vector2 knockbackForce = new Vector2(5f, 4f);

    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;
    public bool IsGrounded { get; private set; }

    private Rigidbody2D rb;
    private Collider2D col;
    private HealthSystem healthSystem;

    private float moveInput;
    private float stateTimer;
    private bool hasDealtAttackDamage;

    // Nombres de parámetros en el Animator (ajustalos si en tu Animator tienen otro nombre)
    private static readonly int HashState = Animator.StringToHash("state");
    private static readonly int HashHorizontal = Animator.StringToHash("horizontal");
    private static readonly int HashGrounded = Animator.StringToHash("suelo");
    private static readonly int HashJumpTrigger = Animator.StringToHash("salto");
    private static readonly int HashAttackTrigger = Animator.StringToHash("atacar");
    private static readonly int HashHurtTrigger = Animator.StringToHash("golpeado");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        healthSystem = GetComponent<HealthSystem>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (playerClimb == null)
        {
            playerClimb = GetComponent<PlayerClimb>();
        }

        if (playerStats == null)
        {
            Debug.LogError("[PlayerController] Falta asignar PlayerStats en el Inspector.", this);
        }
    }

    private void OnEnable()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDamaged += HandleDamaged;
            healthSystem.OnDied += HandleDied;
        }
    }

    private void OnDisable()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDamaged -= HandleDamaged;
            healthSystem.OnDied -= HandleDied;
        }
    }

    private void Start()
    {
        if (playerStats != null && healthSystem != null)
        {
            int targetMaxHealth = playerStats.MaxHealth;
            int sessionHealth = GameManager.Instance != null
                ? GameManager.Instance.GetSessionHealth(targetMaxHealth)
                : targetMaxHealth;

            healthSystem.SetMaxHealth(targetMaxHealth, fillToMax: false);
            // Sincroniza la vida
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReportHealth(
                    Mathf.Clamp(sessionHealth, 0, targetMaxHealth),
                    targetMaxHealth
                );
            }
        }

        ChangeState(PlayerState.Idle);
    }

    private void Update()
    {
        if (!CanAct())
        {
            moveInput = 0f;
            UpdateAnimator();
            return;
        }

        CheckGround();
        moveInput = Input.GetAxisRaw("Horizontal");

        if (CurrentState != PlayerState.Hurt && CurrentState != PlayerState.Attack)
        {
            CheckFlip();
        }

        if (CurrentState == PlayerState.Attack)
        {
            stateTimer -= Time.deltaTime;
            if (!hasDealtAttackDamage && stateTimer <= attackDuration * 0.65f)
            {
                PerformAttackHit();
                hasDealtAttackDamage = true;
            }

            if (stateTimer <= 0f)
            {
                ChangeState(DetermineLocomotionState());
            }

            UpdateAnimator();
            return;
        }

        if (CurrentState == PlayerState.Hurt)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                ChangeState(DetermineLocomotionState());
            }

            UpdateAnimator();
            return;
        }

        // Inputs en estados normales
        if (Input.GetKeyDown(KeyCode.J))
        {
            StartAttack();
            UpdateAnimator();
            return;
        }

        if (Input.GetButtonDown("Jump") && IsGrounded)
        {
            Jump();
        }

        ChangeState(DetermineLocomotionState());
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (rb == null || playerStats == null) return;

        if (!CanAct())
        {
            if (CurrentState != PlayerState.Hurt)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            return;
        }

        bool isClimbing = playerClimb != null && playerClimb.IsClimbing;

        if (CurrentState == PlayerState.Hurt)
        {
            // Deja que el knockback aplicado en HandleDamaged se sienta
            return;
        }

        if (CurrentState == PlayerState.Attack && IsGrounded)
        {
            // Frenar desplazamiento horizontal al atacar en el suelo (opcional)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float targetSpeedX = isClimbing ? moveInput * (playerStats.MovementSpeed * 0.7f) : moveInput * playerStats.MovementSpeed;
        rb.linearVelocity = new Vector2(targetSpeedX, rb.linearVelocity.y);
    }

    private bool CanAct()
    {
        if (healthSystem != null && healthSystem.IsDead) return false;
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return false;
        return true;
    }

    private PlayerState DetermineLocomotionState()
    {
        if (!IsGrounded && (playerClimb == null || !playerClimb.IsClimbing))
        {
            return PlayerState.Jump;
        }

        return Mathf.Abs(moveInput) > 0.01f ? PlayerState.Run : PlayerState.Idle;
    }

    private void Jump()
    {
        if (rb == null || playerStats == null) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, playerStats.JumpForce);
        IsGrounded = false;
        ChangeState(PlayerState.Jump);

        if (animator != null)
        {
            animator.SetTrigger(HashJumpTrigger);
        }
    }

    private void StartAttack()
    {
        CurrentState = PlayerState.Attack;
        stateTimer = attackDuration;
        hasDealtAttackDamage = false;

        if (animator != null)
        {
            animator.SetTrigger(HashAttackTrigger);
        }
    }

    // También podés llamarlo desde un Animation Event si preferís
    public void PerformAttackHit()
    {
        Vector2 origin = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, attackRadius, enemyLayer);

        HashSet<Object> damagedTargets = new HashSet<Object>();

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            MeleeEnemyHealth meleeHealth = hit.GetComponent<MeleeEnemyHealth>() ?? hit.GetComponentInParent<MeleeEnemyHealth>();
            if (meleeHealth != null && !meleeHealth.IsDead && damagedTargets.Add(meleeHealth))
            {
                meleeHealth.TakeDamage(attackDamage, transform);
                continue;
            }
      
            FlyingEnemyHealth flyingHealth = hit.GetComponent<FlyingEnemyHealth>() ?? hit.GetComponentInParent<FlyingEnemyHealth>();
            if (flyingHealth != null)
            {
                flyingHealth.TakeDamage(attackDamage, transform);
            }

            BossEnemyHealth bossHealth = hit.GetComponent<BossEnemyHealth>() ?? hit.GetComponentInParent<BossEnemyHealth>();
            if (bossHealth != null && !bossHealth.IsDead && damagedTargets.Add(bossHealth))
            {
                bossHealth.TakeDamage(attackDamage, transform);
            }
        }
    }

    private void HandleDamaged(int damageAmount)
    {
        if (healthSystem != null && healthSystem.IsDead) return;

        CurrentState = PlayerState.Hurt;
        stateTimer = hurtDuration;

        AudioManager.Instance?.PlayPlayerHurt(transform);

        if (animator != null)
        {
            animator.SetTrigger(HashHurtTrigger);
        }

        if (rb != null)
        {
            float knockDir = transform.localScale.x >= 0 ? -1f : 1f;
            rb.linearVelocity = new Vector2(knockDir * knockbackForce.x, knockbackForce.y);
        }
    }

    private void HandleDied()
    {
        moveInput = 0f;
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
        ChangeState(PlayerState.Idle);
    }

    private void ChangeState(PlayerState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
    }

    private void CheckGround()
    {
        if (col == null) return;

        Bounds bounds = col.bounds;

        Vector2 center = new Vector2(bounds.center.x, bounds.min.y - 0.01f);

        Vector2 size = new Vector2(bounds.size.x * 0.85f, 0.08f);

        IsGrounded = Physics2D.OverlapBox(center, size, 0f, groundLayer) != null;

        Debug.DrawLine(center + Vector2.left * size.x / 2f, center + Vector2.right * size.x / 2f, IsGrounded ? Color.green : Color.red);
    }

    private void CheckFlip()
    {
        if (Mathf.Abs(moveInput) < 0.01f) return;

        bool movingRight = moveInput > 0f;
        bool facingRight = transform.localScale.x >= 0f;

        if (movingRight != facingRight)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetInteger(HashState, (int)CurrentState);
        animator.SetFloat(HashHorizontal, Mathf.Abs(moveInput));
        animator.SetBool(HashGrounded, IsGrounded);
    }

    private void OnDrawGizmosSelected()
    {
        Collider2D playerCollider = col != null ? col : GetComponent<Collider2D>();
        if (playerCollider == null) return;

        Bounds bounds = playerCollider.bounds;

        Vector2 center = new Vector2(bounds.center.x, bounds.min.y - 0.01f);
        Vector2 size = new Vector2(bounds.size.x * 0.85f, 0.08f);

        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(center, size);
    }

}
