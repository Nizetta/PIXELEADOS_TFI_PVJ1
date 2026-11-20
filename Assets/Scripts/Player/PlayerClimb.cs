using UnityEngine;

public class PlayerClimb : MonoBehaviour
{
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private LayerMask ladderLayer;
    [SerializeField] private float detectionRadius = 0.1f;
    private Rigidbody2D rb;
    private Collider2D col;
    private float baseGravity;
    private float verticalInput;
    private bool onLadder;
    private bool isClimbing;
    public bool IsClimbing => isClimbing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        baseGravity = rb.gravityScale;
    }

    private void Update()
    {
        verticalInput = Input.GetAxisRaw("Vertical");

        onLadder = Physics2D.OverlapCircle(
            col.bounds.center, detectionRadius, ladderLayer) != null;

        if (!onLadder)
        {
            isClimbing = false;                
        }
        else if (!isClimbing)
        {
            bool pressingVertical = Mathf.Abs(verticalInput) > 0.1f;
            bool falling = rb.linearVelocity.y < -0.5f;  

            if (pressingVertical || falling)
                isClimbing = true;
        }
    }

    private void FixedUpdate()
    {
        if (isClimbing)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalInput * climbSpeed);
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }
    private void OnDisable()
    {
        if (rb != null) rb.gravityScale = baseGravity;
    }

    private void OnDrawGizmosSelected()
    {
        Collider2D c = col != null ? col : GetComponent<Collider2D>();
        if (c == null) return;

        Gizmos.color = onLadder ? Color.green : Color.red;
        Gizmos.DrawWireSphere(c.bounds.center, detectionRadius);
    }
}
