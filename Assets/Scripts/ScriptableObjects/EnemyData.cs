using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Stats/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("General")]
    [SerializeField] private string enemyName;
    [SerializeField] private Sprite enemyIcon;
    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] public float movementSpeed = 3f;
    [SerializeField] private int damage = 10;
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float attackRadius = 1f;
    [Header("Rewards")]
    [SerializeField] private int scoreValue = 40;
    /*[Header("Audio")]
    [SerializeField] private AudioClip enemySound;
    [Header("Visual Effects")]
    [SerializeField] private GameObject enemyVFXPrefab;
    */

    public string EnemyName => enemyName;
    public Sprite EnemyIcon => enemyIcon;
    public int MaxHealth => maxHealth;
    public float MovementSpeed => movementSpeed;
    public int Damage => damage;
    public float DetectionRadius => detectionRadius;
    public float AttackRadius => attackRadius;
    public int ScoreValue => scoreValue;
    //public AudioClip EnemySound => enemySound;
    //public GameObject VFXPrefab => vfxPrefab;
}
