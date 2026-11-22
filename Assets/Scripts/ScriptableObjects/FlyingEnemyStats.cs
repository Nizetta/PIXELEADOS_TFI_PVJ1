using UnityEngine;

public enum ProjectileAimDirection
{
    Down,
    Forward
}

[CreateAssetMenu(fileName = "FlyingEnemyStats", menuName = "Enemies/Flying Enemy Stats")]
public class FlyingEnemyStats : ScriptableObject
{
    [Header("Health & Death")]
    [Min(1)][SerializeField] private int maxHealth = 30;
    [Min(0f)][SerializeField] private float deathDestroyDelay = 0.5f;

    [Header("Flight / Oscillation Movement")]
    [SerializeField] private float frequency = 2f;
    [SerializeField] private float amplitude = 2.5f;
    [SerializeField] private float verticalBobAmplitude = 0.25f;
    [SerializeField] private float verticalBobFrequency = 3f;

    [Header("Shooting / Projectiles")]
    [SerializeField] private ProjectileAimDirection aimDirection = ProjectileAimDirection.Down;
    [Min(0.05f)][SerializeField] private float shootInterval = 2.0f;
    [Min(0f)][SerializeField] private float initialDelay = 0.5f;
    [Min(0.1f)][SerializeField] private float projectileSpeed = 6f;
    [Min(0.1f)][SerializeField] private float projectileLifeTime = 3.5f;
    [Min(1)][SerializeField] private int projectileDamage = 15;

    [Header("Hit & Knockback")]
    [SerializeField] private Vector2 knockbackForce = new Vector2(2.5f, 1.5f);
    [Min(0f)][SerializeField] private float hurtFlashDuration = 0.15f;
    [Header("Rewards")]
    [SerializeField] private int scoreValue = 150;

    public int ScoreValue => scoreValue;

    public int MaxHealth => maxHealth;
    public float DeathDestroyDelay => deathDestroyDelay;

    public float Frequency => frequency;
    public float Amplitude => amplitude;
    public float VerticalBobAmplitude => verticalBobAmplitude;
    public float VerticalBobFrequency => verticalBobFrequency;

    public ProjectileAimDirection AimDirection => aimDirection;
    public float ShootInterval => shootInterval;
    public float InitialDelay => initialDelay;
    public float ProjectileSpeed => projectileSpeed;
    public float ProjectileLifeTime => projectileLifeTime;
    public int ProjectileDamage => projectileDamage;

    public Vector2 KnockbackForce => knockbackForce;
    public float HurtFlashDuration => hurtFlashDuration;
}
