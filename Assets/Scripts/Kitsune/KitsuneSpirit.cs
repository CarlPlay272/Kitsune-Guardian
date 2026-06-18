using UnityEngine;

public class KitsuneSpirit : MonoBehaviour
{
    [SerializeField] private float maxSpirit = 100f;
    [SerializeField] private float currentSpirit = 0f;

    public float MaxSpirit => maxSpirit;

    // MODIFICADO: Agregado el set público para permitir la persistencia entre niveles
    public float CurrentSpirit
    {
        get => currentSpirit;
        set => currentSpirit = Mathf.Clamp(value, 0f, maxSpirit);
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
}