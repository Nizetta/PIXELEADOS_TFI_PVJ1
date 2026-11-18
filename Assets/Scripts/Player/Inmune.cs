using System.Collections;
using UnityEngine;

public class Inmune : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float blinkInterval = 0.1f;
    [SerializeField] private float immunityDuration = 1f;

    private bool isImmune;
    private Coroutine immunityCoroutine;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats no está asignado en Inmune.");
            enabled = false;
            return;
        }

        blinkInterval = Mathf.Max(0.02f, blinkInterval);
        immunityDuration = Mathf.Max(0f, immunityDuration);
    }

    public bool TryTakeDamage(int amount)
    {
        if (playerStats == null || isImmune)
            return false;

        bool damageApplied = playerStats.TakeDamage(amount);

        if (damageApplied)
        {
            if (immunityCoroutine != null)
                StopCoroutine(immunityCoroutine);

            immunityCoroutine = StartCoroutine(InmunityTimer());
        }

        return damageApplied;
    }

    private IEnumerator InmunityTimer()
    {
        isImmune = true;
        float elapsed = 0f;

        while (elapsed < immunityDuration)
        {
            float remaining = immunityDuration - elapsed;
            float waitTime = Mathf.Min(blinkInterval, remaining);
            float currentAlpha = spriteRenderer != null ? spriteRenderer.color.a : 1f;

            SetSpriteAlpha(currentAlpha > 0.5f ? 0.25f : 1f);
            yield return new WaitForSeconds(waitTime);
            elapsed += waitTime;
        }

        SetSpriteAlpha(1f);
        isImmune = false;
        immunityCoroutine = null;
    }

    private void SetSpriteAlpha(float alpha)
    {
        if (spriteRenderer == null)
            return;

        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}
