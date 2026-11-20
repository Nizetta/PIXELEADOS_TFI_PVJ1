using UnityEngine;

public class MovementPlayer : MonoBehaviour
{
    [Header("Player Data")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Raycast")]
    [SerializeField] private float distanciaRaycast = 0.1f;
    [SerializeField] private LayerMask groundLayer;
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
    void Start()
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
    void Update()
    {
        movimientoHorizontal = Input.GetAxisRaw("Horizontal");
        CheckGround();
        CheckFlip();

        if (animator != null)
        {
            animator.SetFloat("horizontal", Mathf.Abs(movimientoHorizontal));
        }

        if (rb == null)
            return;

        Vector2 velocity = rb.linearVelocity;
        velocity.x = movimientoHorizontal * playerStats.MovementSpeed;
        rb.linearVelocity = velocity;

        if (animator != null)
        {
            animator.SetBool("suelo", isGrounded);
        }

        if (Input.GetButtonDown("Jump") && isGrounded )
        {
            if (animator != null)
            {
                animator.SetTrigger("salto");
            }

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, playerStats.JumpForce);
        }
    }

    private void CheckGround()
    {
        float offsetY = col != null ? col.bounds.extents.y + 0.02f : 0.5f;
        Vector2 origen = (Vector2)transform.position + Vector2.down * offsetY;
        int layerMask = groundLayer.value == 0 ? ~0 : groundLayer.value;
        RaycastHit2D hit = Physics2D.Raycast(origen, Vector2.down, distanciaRaycast, layerMask);
        isGrounded = hit.collider != null && hit.collider != col;
        Color colorRay = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(origen, Vector2.down * distanciaRaycast, colorRay);
        
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
    void OnDrawGizmos()
    {
        if (col != null)
        {
            float offsetY = col.bounds.extents.y + 0.02f;
            Vector2 origen = (Vector2)transform.position + Vector2.down * offsetY;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origen, origen + Vector2.down * distanciaRaycast);
        }
    }
}
