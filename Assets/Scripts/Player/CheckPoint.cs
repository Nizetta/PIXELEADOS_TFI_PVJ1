using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] BoxCollider2D trigger;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Respawn.Instance.respawnPoint = transform;
            trigger.enabled = false; 
        }
    }
}
