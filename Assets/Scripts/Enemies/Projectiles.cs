using UnityEngine;

public class Projectiles : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private int damage = 10;
    private Vector2 direction;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
    public void SetDirection(Vector2 targetPosition)
    {
        direction = (targetPosition - (Vector2)transform.position).normalized;
    }
    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))return;
        Inmune immunity = collision.GetComponent<Inmune>();

        if (immunity != null)
        {
            immunity.TryTakeDamage(damage);
        }
        Destroy(gameObject);
    }
}