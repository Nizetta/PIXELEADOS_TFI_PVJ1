using UnityEngine;

public class Gemas : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager no está disponible; no se puede sumar el puntaje.");
            return;
        }

        GameManager.Instance.AddScore(1);
        Destroy(gameObject);
    }
}