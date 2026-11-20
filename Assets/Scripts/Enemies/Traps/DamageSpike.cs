using UnityEngine;

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

        Inmune inmune = other.GetComponentInParent<Inmune>();
        if (inmune == null) return;

        if (inmune.TryTakeDamage(damage))
            Debug.Log($"Spike: el jugador recibió {damage} de daño.");
    }

}
