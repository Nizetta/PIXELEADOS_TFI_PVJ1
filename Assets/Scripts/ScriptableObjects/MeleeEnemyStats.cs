using UnityEngine;

[CreateAssetMenu(fileName = "MeleeEnemyStats", menuName = "Enemies/Melee Enemy Stats")]
public class MeleeEnemyStats : ScriptableObject
{
    [Header("Health & Death")]
    [Min(1)][SerializeField] private int maxHealth = 60;
    [Min(0f)][SerializeField] private float deathDestroyDelay = 0.8f;

    [Header("Movement & Patrol")]
    [Min(0f)][SerializeField] private float moveSpeed = 2.5f;
    [Min(0f)][SerializeField] private float idleDuration = 1.0f;
    [Min(0.01f)][SerializeField] private float waypointReachThreshold = 0.15f;
    [Min(0f)][SerializeField] private float wallCheckDistance = 0.25f;
    [Min(0f)][SerializeField] private float edgeCheckDistance = 0.35f;

    [Header("Detection & Melee Attack")]
    [Min(0f)][SerializeField] private float detectionRadius = 3.5f;
    [Min(0f)][SerializeField] private float attackRange = 1.1f;
    [Min(1)][SerializeField] private int attackDamage = 15;
    [Min(0f)][SerializeField] private float attackCooldown = 1.2f;
    [Min(0.01f)][SerializeField] private float attackDuration = 0.45f;

    [Header("Hit & Knockback")]
    [SerializeField] private Vector2 knockbackForce = new Vector2(4f, 2.5f);
    [Min(0f)][SerializeField] private float hurtDuration = 0.25f;

    // Propiedades de solo lectura para consumo seguro desde los scripts
    public int MaxHealth => maxHealth;
    public float DeathDestroyDelay => deathDestroyDelay;

    public float MoveSpeed => moveSpeed;
    public float IdleDuration => idleDuration;
    public float WaypointReachThreshold => waypointReachThreshold;
    public float WallCheckDistance => wallCheckDistance;
    public float EdgeCheckDistance => edgeCheckDistance;

    public float DetectionRadius => detectionRadius;
    public float AttackRange => attackRange;
    public int AttackDamage => attackDamage;
    public float AttackCooldown => attackCooldown;
    public float AttackDuration => attackDuration;

    public Vector2 KnockbackForce => knockbackForce;
    public float HurtDuration => hurtDuration;

}
