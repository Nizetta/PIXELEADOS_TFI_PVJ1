using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Stats/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private int currentScore = 0;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float deceleration = 80f;
    [SerializeField] private Vector2 checkpoint;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public int CurrentScore => currentScore;
    public float MovementSpeed => movementSpeed;
    public float JumpForce => jumpForce;
    public float Acceleration => acceleration;
    public float Deceleration => deceleration;
    public Vector2 Checkpoint
    {
        get { return checkpoint; }
        set { checkpoint = value; }
    }
    public void ResetStats()
    {
        currentHealth = maxHealth;
        currentScore = 0;
    }
    public bool TakeDamage(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        return true;
    }
    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
    }
    public void AddScore(int amount)
    {
        currentScore += amount;
    }

}
