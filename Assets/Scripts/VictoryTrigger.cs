using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class VictoryTrigger : MonoBehaviour
{
    [Header("Victory Behavior")]
    [Tooltip("Si es true, intenta pasar al siguiente nivel en GameManager.levelSceneName; si es el último o no está listado, entra en GameState.Victory.")]
    [SerializeField] private bool useCompleteLevelFlow = true;

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    private bool hasTriggered;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;

        if (!other.CompareTag(playerTag)) return;

        hasTriggered = true;

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[VictoryTrigger] No hay GameManager.Instance en la escena.");
            return;
        }

        if (useCompleteLevelFlow)
        {
            GameManager.Instance.CompleteLevel();
        }
        else
        {
            GameManager.Instance.Win();
        }
    }
}
