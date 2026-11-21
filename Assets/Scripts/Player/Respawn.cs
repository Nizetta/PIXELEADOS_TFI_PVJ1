using UnityEngine;

public class Respawn : MonoBehaviour
{
    [SerializeField] public Transform respawnPoint; // Punto de reaparición
    [SerializeField] private int fallDamage = 10;
    public static Respawn Instance { get; private set; } // Instancia singleton

    private void Awake()
    {
        if (Instance == null) Instance = this;

        else Destroy(gameObject);
        
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        HealthSystem health = other.GetComponentInParent<HealthSystem>();

        if (health != null)
        {
            health.TakeDamage(fallDamage);
        }

        other.transform.position = respawnPoint.position;
    }
}
