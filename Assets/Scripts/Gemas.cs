using UnityEngine;

public class Gemas : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 100f;
    [Header("Item Data")]
    [SerializeField] private ItemData itemData;
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
        if (itemData == null)
        {
            Debug.LogError("No se asignó ItemData a esta gema");
            return;
        }
        //GameManager.Instance.AddScore(1);
        GameManager.Instance.CollectItem(itemData);
        AudioManager.Instance.PlayCoin(transform, 1f);

        Destroy(gameObject);
    }
}