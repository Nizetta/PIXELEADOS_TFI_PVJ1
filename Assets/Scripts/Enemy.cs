using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Transform enemy;
    [SerializeField] private float radius = 2f;
    [SerializeField] private float speed= 4f;
    [SerializeField] private EnemyData enemyData;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool enMovimiento;
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
            if (direction.x<0)
            {
                transform.localScale = new Vector3(5, 5, 5);
            }
            else if (direction.x>0)
            {
                transform.localScale = new Vector3(-5, 5, 5);
            }
            movement =new Vector2(direction.x, 0);
            enMovimiento = true;
        }
        else
        {
            movement = Vector2.zero;
            enMovimiento = false;
        }

        rb.MovePosition(rb.position + movement * speed * Time.deltaTime);
        animator.SetBool("enMovimiento", enMovimiento);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        TryDamagePlayer(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        TryDamagePlayer(collision.gameObject);
    }

    private void TryDamagePlayer(GameObject player)
    {
        Inmune inmunidad = player.GetComponent<Inmune>();
        if (inmunidad == null)
            return;

        int damage = enemyData != null ? enemyData.Damage : 10;
        inmunidad.TryTakeDamage(damage);
    }
}
