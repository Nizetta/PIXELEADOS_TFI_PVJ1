using UnityEngine;

[CreateAssetMenu(fileName = "BossEnemyStats", menuName = "Enemies/Boss Enemy Stats")]
public class BossEnemyStats : ScriptableObject
{
    [Header("Health & Death")]
    [Min(1)][SerializeField] private int maxHealth = 200;
    [Min(0f)][SerializeField] private float deathDestroyDelay = 1.2f;

    [Header("Detection & Chase")]
    [Min(0f)][SerializeField] private float detectionRadius = 7f;
    [Min(0f)][SerializeField] private float loseTargetRadius = 10f;
    [Min(0f)][SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private bool returnHomeWhenLost = true;
    [Min(0.05f)][SerializeField] private float homeReachThreshold = 0.2f;

    [Header("Light Attack")]
    [Min(0f)][SerializeField] private float lightAttackRange = 1.4f;
    [Min(1)][SerializeField] private int lightAttackDamage = 15;
    [Min(0.05f)][SerializeField] private float lightAttackDuration = 0.4f;

    [Header("Heavy Attack")]
    [Min(0f)][SerializeField] private float heavyAttackRange = 2.2f;
    [Min(1)][SerializeField] private int heavyAttackDamage = 30;
    [Min(0.05f)][SerializeField] private float heavyAttackDuration = 0.7f;

    [Header("Attack Cooldown")]
    [Min(0.1f)][SerializeField] private float attackCooldown = 1.3f;

    [Header("Ground Portal Teleport")]
    [SerializeField] private bool canTeleport = true;
    [Min(0.5f)][SerializeField] private float teleportCooldown = 4.5f;
    [Min(0.05f)][SerializeField] private float vanishDuration = 0.35f;
    [Min(0.05f)][SerializeField] private float reappearDuration = 0.35f;
    [SerializeField] private float teleportOffsetFromPlayer = 2.0f;

    [Header("Hit & Knockback")]
    [SerializeField] private Vector2 knockbackForce = new Vector2(1.5f, 1.0f);
    [Min(0f)][SerializeField] private float hurtDuration = 0.2f;
    [Header("Rewards")]
    [SerializeField] private int scoreValue = 500;

    public int ScoreValue => scoreValue;

    public int MaxHealth => maxHealth;
    public float DeathDestroyDelay => deathDestroyDelay;

    public float DetectionRadius => detectionRadius;
    public float LoseTargetRadius => Mathf.Max(loseTargetRadius, detectionRadius);
    public float ChaseSpeed => chaseSpeed;
    public bool ReturnHomeWhenLost => returnHomeWhenLost;
    public float HomeReachThreshold => homeReachThreshold;

    public float LightAttackRange => lightAttackRange;
    public int LightAttackDamage => lightAttackDamage;
    public float LightAttackDuration => lightAttackDuration;

    public float HeavyAttackRange => heavyAttackRange;
    public int HeavyAttackDamage => heavyAttackDamage;
    public float HeavyAttackDuration => heavyAttackDuration;

    public float AttackCooldown => attackCooldown;

    public bool CanTeleport => canTeleport;
    public float TeleportCooldown => teleportCooldown;
    public float VanishDuration => vanishDuration;
    public float ReappearDuration => reappearDuration;
    public float TeleportOffsetFromPlayer => teleportOffsetFromPlayer;

    public Vector2 KnockbackForce => knockbackForce;
    public float HurtDuration => hurtDuration;
}
