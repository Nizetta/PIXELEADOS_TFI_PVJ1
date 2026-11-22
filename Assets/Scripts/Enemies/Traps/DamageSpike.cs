using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class DamageSpike : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask targetLayer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        if ((targetLayer.value & (1 << other.gameObject.layer)) == 0) return;

        HealthSystem playerHealth = other.GetComponent<HealthSystem>() ?? other.GetComponentInParent<HealthSystem>();
        if (playerHealth != null && !playerHealth.IsDead)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"Spike: el jugador recibió {damage} de daño.");
        }

    }
   
}
