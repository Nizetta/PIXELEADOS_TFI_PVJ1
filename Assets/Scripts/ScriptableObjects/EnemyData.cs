using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Stats/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Stats")]
    [SerializeField] private string enemyName;
    [SerializeField] private Sprite enemyIcon;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] public float movementSpeed = 3f;
    [SerializeField] private int damage = 10;
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
    public int ScoreValue => scoreValue;
    //public AudioClip EnemySound => enemySound;
    //public GameObject VFXPrefab => vfxPrefab;
}
