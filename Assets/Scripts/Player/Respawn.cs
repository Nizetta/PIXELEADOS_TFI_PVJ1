using UnityEngine;

public class Respawn : MonoBehaviour
{
    [SerializeField] public Transform respawnPoint; // Punto de reaparición
    public static Respawn Instance { get; private set; } // Instancia singleton

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = respawnPoint.position; // Teletransporta al jugador al punto de reaparición
        }
    }
}
