using UnityEngine;

public class Gemas : MonoBehaviour
{
    [Header("Score")]
    [SerializeField] private int scoreValue;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager no está disponible; no se puede sumar el puntaje.");
            return;
        }

        GameManager.Instance.AddScore(scoreValue);
        AudioManager.Instance.PlayCoin(transform, 1f);
        Destroy(gameObject);
    }
}