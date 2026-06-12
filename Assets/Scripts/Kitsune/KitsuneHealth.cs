using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // OBLIGATORIO: Para poder recargar el nivel completo

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

    [Header("Debug y Atrapado (GEMINI)")]
    [SerializeField] private bool debugHealthKeysEnabled = false;
    [SerializeField] private KeyCode autoMuerteKey = KeyCode.R;
    [SerializeField] private KeyCode toggleDebugKey = KeyCode.F3;
    [SerializeField] private KeyCode damageKey = KeyCode.J;
    [SerializeField] private KeyCode healKey = KeyCode.K;
    [SerializeField] private float debugStep = 10f;

    [Header("Configuración de Reinicio Total")]
    [Tooltip("Tiempo en segundos que se debe mantener presionada la R para reiniciar todo el nivel de cero.")]
    [SerializeField] private float tiempoParaReiniciarNivel = 2.0f;

    private bool isDead = false;
    private Vector3 spawnPosition;
    private float temporizadorR = 0f; // Mide cuánto tiempo se mantiene presionada la tecla R

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

        // LÓGICA DE REINICIO HÍBRIDA (PULSAR VS MANTENER R)
        if (!isDead)
        {
            // Mientras la tecla R se mantenga apretada...
            if (Input.GetKey(autoMuerteKey))
            {
                temporizadorR += Time.deltaTime;

                // Si superó el tiempo límite establecido, se gatilla el reinicio absoluto
                if (temporizadorR >= tiempoParaReiniciarNivel)
                {
                    Debug.Log("Tecla " + autoMuerteKey + " mantenida por " + tiempoParaReiniciarNivel + "s. ¡Reiniciando nivel desde cero de forma absoluta!");
                    ReiniciarNivelCompleto();
                }
            }

            // En el instante que el jugador SUELTA la tecla R...
            if (Input.GetKeyUp(autoMuerteKey))
            {
                // Si la soltó rápido (menos del tiempo límite), se interpreta como el suicidio normal
                if (temporizadorR < tiempoParaReiniciarNivel && temporizadorR > 0.05f)
                {
                    Debug.Log("Kitsune atascado. Ejecutando botón de pánico espiritual rápido con la tecla: " + autoMuerteKey);
                    StartCoroutine(RespawnRoutine());
                }

                // Resetear el acumulador obligatoriamente al soltar la tecla
                temporizadorR = 0f;
            }
        }
    }

    private void ReiniciarNivelCompleto()
    {
        // Obtener el nombre de la escena que se está jugando actualmente (ej: "SampleScene")
        string nombreEscenaActual = SceneManager.GetActiveScene().name;

        // Cargar de nuevo la escena limpia, reseteando memoria, enemigos, llaves, checkpoints y powerups
        SceneManager.LoadScene(nombreEscenaActual);
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
        temporizadorR = 0f; // Evitar cualquier conflicto con el contador al morir

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

        if (GameController.Instance != null)
        {
            transform.position = GameController.Instance.PuntoRetornoActual;
            Debug.Log("Respawn exitoso en Checkpoint ID actual: " + GameController.Instance.CheckpointActualID + " | Posición: " + transform.position);
        }
        else if (respawnPoint != null)
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