using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Stats/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [Header("Base Stats")]
    [Min(1)][SerializeField] private int baseMaxHealth = 100;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float jumpForce = 7.5f;

    [Header("Shop Upgrades")]
    [SerializeField] private int bonusMaxHealth = 0;
    [SerializeField] private float bonusMovementSpeed = 0f;
    [SerializeField] private bool hasSecondaryUpgrade = false;
    [Header("Checkpoint")]
    [SerializeField] private Vector2 checkpoint;

    public int MaxHealth => Mathf.Max(1, baseMaxHealth + bonusMaxHealth);
    public float MovementSpeed => Mathf.Max(0.1f, movementSpeed + bonusMovementSpeed);
    public float JumpForce => jumpForce;
    public bool HasSecondaryUpgrade => hasSecondaryUpgrade;
    public Vector2 Checkpoint
    {
        get { return checkpoint; }
        set { checkpoint = value; }
    }
    public event Action OnUpgradesChanged;
    public void AddMaxHealthBonus(int amount)
    {
        if (amount <= 0) return;
        bonusMaxHealth += amount;
        OnUpgradesChanged?.Invoke();
    }
    public void AddMovementSpeedBonus(float amount)
    {
        if (amount <= 0f) return;
        bonusMovementSpeed += amount;
        OnUpgradesChanged?.Invoke();
    }
    public void UnlockSecondaryUpgrade()
    {
        if (hasSecondaryUpgrade) return;
        hasSecondaryUpgrade = true;
        OnUpgradesChanged?.Invoke();
    }
    public void ResetUpgrades()
    {
        bonusMaxHealth = 0;
        bonusMovementSpeed = 0f;
        hasSecondaryUpgrade = false;
        OnUpgradesChanged?.Invoke();
    }

}
