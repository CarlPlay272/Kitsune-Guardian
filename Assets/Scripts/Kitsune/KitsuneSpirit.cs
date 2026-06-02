using UnityEngine;

public class KitsuneSpirit : MonoBehaviour
{
    [SerializeField] private float maxSpirit = 100f;
    [SerializeField] private float currentSpirit = 0f;

    public float MaxSpirit => maxSpirit;
    public float CurrentSpirit => currentSpirit;

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