using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CameraFollow : MonoBehaviour
{
    [Header("Velocidad de movimiento")]
    [SerializeField] private float speed = 2.2f;
    [SerializeField] private float jumpForce = 5f;

    [Header("Raycast")]
    [SerializeField] private float distanciaRaycast = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    private float movimientoHorizontal;
    private bool isGrounded;
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator animator;

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

    private void Update()
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
        velocity.x = movimientoHorizontal * speed;
        rb.linearVelocity = velocity;

        //animator.SetBool("suelo", isGrounded);
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            if (animator != null)
            {
                animator.SetTrigger("salto");
            }

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void CheckGround()
    {
        float offsetY = col != null ? col.bounds.extents.y + 0.02f : 0.5f;
        Vector2 origen = (Vector2)transform.position + Vector2.down * offsetY;
        isGrounded = Physics2D.Raycast(origen, Vector2.down, distanciaRaycast, groundLayer);

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
}
