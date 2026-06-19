using UnityEngine;

public class KitsuneSpirit : MonoBehaviour
{
    [Header("Espíritu")]
    [SerializeField] private float maxSpirit = 100f;
    [SerializeField] private float currentSpirit = 50f;

    public float MaxSpirit => maxSpirit;

    public float CurrentSpirit
    {
        get => currentSpirit;
        set => currentSpirit = Mathf.Clamp(value, 0f, maxSpirit);
    }

    public float SpiritPercentage
    {
        get
        {
            if (maxSpirit <= 0f)
                return 0f;

            return currentSpirit / maxSpirit;
        }
    }

    public bool HasSpirit(float amount)
    {
        return currentSpirit >= amount;
    }

    public void AddSpirit(float amount)
    {
        currentSpirit += amount;
        currentSpirit = Mathf.Clamp(
            currentSpirit,
            0f,
            maxSpirit
        );
    }

    public bool ConsumeSpirit(float amount)
    {
        if (currentSpirit < amount)
            return false;

        currentSpirit -= amount;

        currentSpirit = Mathf.Clamp(
            currentSpirit,
            0f,
            maxSpirit
        );

        return true;
    }

    public void FillSpirit()
    {
        currentSpirit = maxSpirit;
    }

    public void EmptySpirit()
    {
        currentSpirit = 0f;
    }

    public void SetMaxSpirit(float newMaxSpirit)
    {
        maxSpirit = Mathf.Max(1f, newMaxSpirit);

        currentSpirit = Mathf.Clamp(
            currentSpirit,
            0f,
            maxSpirit
        );
    }
}