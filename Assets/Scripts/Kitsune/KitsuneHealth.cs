using System.Collections;
using UnityEngine;

public class KitsuneHealth : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float respawnDelay = 1f;
    [SerializeField] private bool restoreFullHealthOnRespawn = true;

    [Header("Daño por peligro")]
    [SerializeField] private string deathTag = "";
    [SerializeField] private float trapDamage = 999f;

    [Header("Debug")]
    [SerializeField] private bool debugHealthKeysEnabled = false;
    [SerializeField] private KeyCode toggleDebugKey = KeyCode.F3;
    [SerializeField] private KeyCode damageKey = KeyCode.J;
    [SerializeField] private KeyCode healKey = KeyCode.K;
    [SerializeField] private float debugStep = 10f;

    private bool isDead = false;
    private Vector3 spawnPosition;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    void Start()
    {
        currentHealth = maxHealth;
        spawnPosition = transform.position;

        Debug.Log("KitsuneHealth cargado. Vida inicial: " + currentHealth);
        Debug.Log("Punto de spawn inicial guardado en: " + spawnPosition);
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
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log("Daño recibido. Vida actual: " + currentHealth);

        if (currentHealth <= 0f)
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log("Curación aplicada. Vida actual: " + currentHealth);
    }

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        Debug.Log("Vida restaurada al máximo: " + currentHealth);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (!string.IsNullOrEmpty(deathTag) && other.CompareTag(deathTag))
        {
            TakeDamage(trapDamage);
        }
    }

    private IEnumerator RespawnRoutine()
    {
        isDead = true;

        if (GameController.Instance != null)
        {
            GameController.Instance.RestarVida(1);
        }

        yield return new WaitForSeconds(respawnDelay);

        if (GameController.Instance != null && GameController.Instance.VidasActuales <= 0)
        {
            gameObject.SetActive(false);
            yield break;
        }

        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            Debug.Log("Respawn en respawnPoint asignado: " + respawnPoint.position);
        }
        else
        {
            transform.position = spawnPosition;
            Debug.Log("Respawn en spawn inicial guardado: " + spawnPosition);
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (restoreFullHealthOnRespawn)
        {
            RestoreFullHealth();
        }

        isDead = false;
        Debug.Log("Kitsune reapareció correctamente.");
    }
}