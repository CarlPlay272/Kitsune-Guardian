using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitsuneHealth : MonoBehaviour, IDamageable
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

    [Header("Debug General")]
    [SerializeField] private bool debugHealthKeysEnabled = false;
    [SerializeField] private KeyCode autoMuerteKey = KeyCode.R;
    [SerializeField] private KeyCode toggleDebugKey = KeyCode.F3;
    [SerializeField] private float debugStep = 10f;

    [Header("Mapeo Invertido Fila Numérica (Carlos)")]
    [SerializeField] private KeyCode menosCorazonKey = KeyCode.Alpha5;
    [SerializeField] private KeyCode masCorazonKey = KeyCode.Alpha6;
    [SerializeField] private KeyCode menosVidaKey = KeyCode.Alpha7;
    [SerializeField] private KeyCode masVidaKey = KeyCode.Alpha8;
    [SerializeField] private KeyCode menosEnergiaKey = KeyCode.Alpha9;
    [SerializeField] private KeyCode masEnergiaKey = KeyCode.Alpha0;

    [Header("Mapeo Habilidades QWERTY")]
    [SerializeField] private KeyCode toggleDisparoKey = KeyCode.U;
    [SerializeField] private KeyCode toggleDashKey = KeyCode.I;
    [SerializeField] private KeyCode toggleInvisibilidadKey = KeyCode.O;

    [Header("Configuración De Reinicio Total")]
    [Tooltip("Tiempo en segundos que se debe mantener presionada la R para reiniciar todo el nivel de cero.")]
    [SerializeField] private float tiempoParaReiniciarNivel = 2.0f;

    private bool isDead = false;
    private Vector3 spawnPosition;
    private float temporizadorR = 0f;

    private KitsuneController kitsuneController;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    void Start()
    {
        currentHealth = maxHealth;
        spawnPosition = transform.position;

        kitsuneController = GetComponent<KitsuneController>();

        // 🔥 FIJACIÓN DE HIERRO: Forzamos el orden de las teclas en el inicio del juego
        // Esto ignora lo que esté mal guardado en el Inspector de los Niveles 1, 2 o 3.
        toggleDisparoKey = KeyCode.U;
        toggleDashKey = KeyCode.I;
        toggleInvisibilidadKey = KeyCode.O;

        Debug.Log("KitsuneHealth cargado. Vida inicial: " + currentHealth);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleDebugKey))
        {
            debugHealthKeysEnabled = !debugHealthKeysEnabled;
            Debug.Log("Modo debug de salud y habilidades: " + (debugHealthKeysEnabled ? "ACTIVADO" : "DESACTIVADO"));
        }

        if (debugHealthKeysEnabled)
        {
            if (Input.GetKeyDown(menosCorazonKey) && GameController.Instance != null)
            {
                GameController.Instance.ModificarVidasDebug(-1);

                if (GameController.Instance.VidasActuales <= 0)
                {
                    StartCoroutine(RespawnRoutine());
                }
            }

            if (Input.GetKeyDown(masCorazonKey) && GameController.Instance != null)
            {
                GameController.Instance.ModificarVidasDebug(1);
            }

            if (Input.GetKeyDown(menosVidaKey))
            {
                TakeDamage(debugStep);
            }
            if (Input.GetKeyDown(masVidaKey))
            {
                Heal(debugStep);
            }

            KitsuneSpirit spiritComp = GetComponentInParent<KitsuneSpirit>() ?? GetComponentInChildren<KitsuneSpirit>();
            if (spiritComp != null)
            {
                if (Input.GetKeyDown(menosEnergiaKey))
                {
                    spiritComp.ConsumeSpirit(debugStep);
                    Debug.Log("⚡ [DEBUG] Energía disminuida. Actual: " + spiritComp.CurrentSpirit);
                }
                if (Input.GetKeyDown(masEnergiaKey))
                {
                    spiritComp.AddSpirit(debugStep);
                    Debug.Log("⚡ [DEBUG] Energía aumentada. Actual: " + spiritComp.CurrentSpirit);
                }
            }

            // 🔥 COMPROBADO: Activa de forma exclusiva el Disparo (U)
            if (Input.GetKeyDown(toggleDisparoKey) && GameController.Instance != null)
            {
                bool nuevoEstado = !GameController.Instance.DisparoDesbloqueado;
                GameController.Instance.DesbloquearDisparoDebug(nuevoEstado);
                Debug.Log("🔥 [DEBUG] Habilidad de Disparo establecida en: " + nuevoEstado);
            }

            // 🔥 COMPROBADO: Activa de forma exclusiva el Dash (I)
            if (Input.GetKeyDown(toggleDashKey) && GameController.Instance != null)
            {
                bool nuevoEstado = !GameController.Instance.DashDesbloqueado;
                GameController.Instance.DesbloquearDashDebug(nuevoEstado);
                Debug.Log("💨 [DEBUG] Habilidad de Dash establecida en: " + nuevoEstado);
            }

            // 🔥 COMPROBADO: Activa de forma exclusiva la Invisibilidad (O)
            if (Input.GetKeyDown(toggleInvisibilidadKey) && GameController.Instance != null)
            {
                bool nuevoEstado = !GameController.Instance.InvisibilidadDesbloqueada;
                GameController.Instance.DesbloquearInvisibilidadDebug(nuevoEstado);
                Debug.Log("🔮 [DEBUG] Invisibilidad establecida en: " + nuevoEstado);
            }
        }

        if (!isDead)
        {
            if (Input.GetKey(autoMuerteKey))
            {
                temporizadorR += Time.deltaTime;

                if (temporizadorR >= tiempoParaReiniciarNivel)
                {
                    Debug.Log("Reiniciando nivel desde cero de forma absoluta!");
                    ReiniciarNivelCompleto();
                }
            }

            if (Input.GetKeyUp(autoMuerteKey))
            {
                if (temporizadorR < tiempoParaReiniciarNivel && temporizadorR > 0.05f)
                {
                    StartCoroutine(RespawnRoutine());
                }
                temporizadorR = 0f;
            }
        }
    }

    private void ReiniciarNivelCompleto()
    {
        string nombreEscenaActual = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(nombreEscenaActual);
    }

    public void TakeDamage(int amount)
    {
        TakeDamage((float)amount);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        if (kitsuneController != null && kitsuneController.IsDashing)
        {
            Debug.Log("🛡️ [INVULNERABLE] Kitsune esquivó el golpe usando la inmunidad del Dash.");
            return;
        }

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
        temporizadorR = 0f;

        if (GameController.Instance != null && GameController.Instance.VidasActuales > 0)
        {
            GameController.Instance.RestarVida(1);
        }

        yield return new WaitForSeconds(respawnDelay);

        if (GameController.Instance != null && GameController.Instance.VidasActuales <= 0)
        {
            KitsuneController controller = GetComponent<KitsuneController>();
            if (controller != null)
            {
                controller.BloquearControles();
            }

            GameController.Instance.ActivarGameOver();
            gameObject.SetActive(false);
            yield break;
        }

        if (GameController.Instance != null)
        {
            transform.position = GameController.Instance.PuntoRetornoActual;
        }
        else if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
        }
        else
        {
            transform.position = spawnPosition;
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
    }
}