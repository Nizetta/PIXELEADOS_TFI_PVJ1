using UnityEngine;

public class MovementPlayer : MonoBehaviour
{
    [Header("Player Data")]
    [SerializeField] private PlayerStats playerStats;
    //public float speed = 5f; 
    void Start()
    {
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats no está asignado");
            enabled = false;
        }
    }
    void Update()
    {
        float movementX = Input.GetAxis("Horizontal")* playerStats.MovementSpeed * Time.deltaTime;
        Vector2 position= transform.position;
        transform.position = new Vector2(position.x + movementX, position.y);
    }
}
