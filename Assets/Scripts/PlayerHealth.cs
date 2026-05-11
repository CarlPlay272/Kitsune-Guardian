using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Debug")]
    public bool debugHealthKeysEnabled = false;
    public KeyCode toggleDebugKey = KeyCode.F3;
    public KeyCode damageKey = KeyCode.J;
    public KeyCode healKey = KeyCode.K;
    public float debugStep = 10f;

    void Start()
    {
        currentHealth = maxHealth;
        Debug.Log("PlayerHealth cargado. Vida inicial: " + currentHealth);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleDebugKey))
        {
            debugHealthKeysEnabled = !debugHealthKeysEnabled;
            Debug.Log("Modo debug de vida: " + (debugHealthKeysEnabled ? "ACTIVADO" : "DESACTIVADO"));
        }

        if (debugHealthKeysEnabled && Input.GetKeyDown(damageKey))
        {
            TakeDamage(debugStep);
        }

        if (debugHealthKeysEnabled && Input.GetKeyDown(healKey))
        {
            Heal(debugStep);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        Debug.Log("Daño recibido. Vida actual: " + currentHealth);
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        Debug.Log("Curación aplicada. Vida actual: " + currentHealth);
    }
}