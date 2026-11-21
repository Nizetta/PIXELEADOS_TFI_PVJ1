using UnityEngine;

public class Gemas : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 100f;
    private void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager no está disponible");
            return;
        }

        GameManager.Instance.AddScore(1);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCoin(transform);
        }
        Destroy(gameObject);
    }
}