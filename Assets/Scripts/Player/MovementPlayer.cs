using UnityEngine;

public class MovementPlayer : MonoBehaviour
{
    [Header("Player Data")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckHeight = 0.05f;
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator animator;

    private float movimientoHorizontal;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();

        if (rb == null)
        {
            Debug.LogError("Falta el Rigidbody2D en el personaje.");
        }

        if (animator == null)
        {
            Debug.LogWarning("No se encontró Animator en el personaje. La animación no se actualizará.");
        }
    }
    private void Start()
    {
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats no está asignado");
            enabled = false;
            return;
        }

        if (groundLayer.value == 0)
        {
            Debug.LogWarning("Ground Layer sin asignar: el jugador no detectará el suelo.");
        }
    }
    private void Update()
    {
        movimientoHorizontal = Input.GetAxisRaw("Horizontal");
        CheckGround();
        CheckFlip();

        if (animator != null)
        {
            animator.SetFloat("horizontal", Mathf.Abs(movimientoHorizontal));
            animator.SetBool("suelo", isGrounded);
        }

        if (rb == null)
            return;

        Vector2 velocity = rb.linearVelocity;
        velocity.x = movimientoHorizontal * playerStats.MovementSpeed;
        rb.linearVelocity = velocity;

        if (Input.GetButtonDown("Jump") && isGrounded )
        {
            if (animator != null)
            {
                animator.SetTrigger("salto");
            }

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, playerStats.JumpForce);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (animator != null) animator.SetTrigger("ataque");
        }
    }

    private void CheckGround()
    {
        if (col == null) return;
        Bounds b = col.bounds;
        Vector2 center = new Vector2(b.center.x, b.min.y - groundCheckHeight * 0.5f);
        Vector2 size = new Vector2(b.size.x * 0.9f, groundCheckHeight);
        isGrounded = Physics2D.OverlapBox(center, size, 0f, groundLayer) != null;
        
    }
 
    private void CheckFlip()
    {
        if (movimientoHorizontal == 0)
            return;

        bool movingRight = movimientoHorizontal > 0;
        bool facingRight = transform.localScale.x >= 0;

        if (movingRight != facingRight)
        {
            Vector3 newScale = transform.localScale;
            newScale.x *= -1;
            transform.localScale = newScale;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Collider2D c = col != null ? col : GetComponent<Collider2D>();
        if (c == null) return;

        Bounds b = c.bounds;
        Vector3 center = new Vector3(b.center.x, b.min.y - groundCheckHeight * 0.5f, 0f);
        Vector3 size = new Vector3(b.size.x * 0.9f, groundCheckHeight, 0f);

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(center, size);
    }
}
