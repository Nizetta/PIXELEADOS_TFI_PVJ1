using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Transform enemy;
    [SerializeField] private float radius = 2f;
    [SerializeField] private float speed= 4f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, enemy.position);
        if (distanceToPlayer <= radius)
        {
            Vector2 direction = (enemy.position - transform.position).normalized;
            movement =new Vector2(direction.x, 0);
        }
        else
        {
            movement = Vector2.zero;
        }

        rb.MovePosition(rb.position + movement * speed * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
