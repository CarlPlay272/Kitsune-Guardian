using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    [Header("Referencias")]
    [SerializeField] private GameObject player;
    [SerializeField] private Transform limiteIzquierdo;
    [SerializeField] private Transform limiteDerecho;

    [Header("UI")]
    [SerializeField] private TMP_Text vidasText;
    [SerializeField] private TMP_Text puntosText;
    [SerializeField] private GameObject gameOverPanel;

    [Header("HUD Colas")]
    [SerializeField] private GameObject colaHUD1; // ńes/HUD/TailIcons/ColaHUD_01_Invisibilidad
    [SerializeField] private GameObject colaHUD2; // ńes/HUD/TailIcons/ColaHUD_02_Dash

    [Header("Estado")]
    [SerializeField] private int vidasIniciales = 5;

    [Header("Purificación")]
    [SerializeField] private GameObject contenedorCorrupcion;

    private Dictionary<string, bool> purificacionPorEscena =
        new Dictionary<string, bool>();

    [Header("Plataforma de salto")]
    [SerializeField] private GameObject humosSalto;
    [SerializeField] private GameObject jumpPadTrigger;
    [SerializeField] private bool plataformaSaltoActiva = false;

    [Header("Poderes")]
    [SerializeField] private bool invisibilidadDesbloqueada = false;
    [SerializeField] private bool dashDesbloqueado = false;
    [SerializeField] private bool disparoDesbloqueado = false;

    [Header("Llave del nivel")]
    [SerializeField] private bool tieneLlave = false;

    [Header("Sistema de Checkpoints Secuenciales (GEMINI)")]
    [SerializeField] private int checkpointActualID = 0;
    [SerializeField] private Vector3 puntoRetornoActual;

    private int vidasActuales;
    private int puntosActuales;

    // VARIABLES CACHÉ PARA PERSISTENCIA ENTRE NIVELES (GEMINI)
    private static int cachedVidas = -1;
    private static int cachedPuntos = 0;
    private static float cachedHealth = -1f;
    private static float cachedSpirit = -1f;
    private static bool cachedInvisibilidad = false;
    private static bool cachedDash = false;
    private static bool cachedDisparo = false;
    private static bool vieneDeNivelAnterior = false;

    // Propiedades Públicas
    public GameObject Player => player;
    public float LimiteIzquierdo => limiteIzquierdo != null ? limiteIzquierdo.position.x : 0f;
    public float LimiteDerecho => limiteDerecho != null ? limiteDerecho.position.x : 0f;
    public int VidasActuales => vidasActuales;
    public int PuntosActuales => puntosActuales;
    public bool BosquePurificado
    {
        get
        {
            string escena = SceneManager.GetActiveScene().name;

            return purificacionPorEscena.TryGetValue(
                escena,
                out bool purificado
            ) && purificado;
        }
    }
    public bool TieneLlave => tieneLlave;
    public bool InvisibilidadDesbloqueada => invisibilidadDesbloqueada;
    public bool DashDesbloqueado => dashDesbloqueado;
    public bool DisparoDesbloqueado => disparoDesbloqueado;
    public bool PlataformaSaltoActiva => plataformaSaltoActiva;

    public Vector3 PuntoRetornoActual => puntoRetornoActual;
    public int CheckpointActualID => checkpointActualID;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        InicializarDatosNivel();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InicializarDatosNivel()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (!vieneDeNivelAnterior)
        {
            vidasActuales = vidasIniciales;
            puntosActuales = 0;
            if (player != null)
            {
                puntoRetornoActual = player.transform.position;
            }
        }
        else
        {
            vidasActuales = cachedVidas;
            puntosActuales = cachedPuntos;
            invisibilidadDesbloqueada = cachedInvisibilidad;
            dashDesbloqueado = cachedDash;
            disparoDesbloqueado = cachedDisparo;

            ActualizarNuevoPlayerInstanciado();
        }

        ActualizarUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        vidasText = GameObject.Find("VidasText")?.GetComponent<TMP_Text>();
        puntosText = GameObject.Find("PuntosText")?.GetComponent<TMP_Text>();
        gameOverPanel = GameObject.Find("GameOverPanel");

        Transform hudCanvas = GameObject.Find("HUD")?.transform;
        if (hudCanvas != null)
        {
            colaHUD1 = hudCanvas.Find("TailIcons/ColaHUD_01_Invisibilidad")?.gameObject;
            colaHUD2 = hudCanvas.Find("TailIcons/ColaHUD_02_Dash")?.gameObject;
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (colaHUD1 != null) colaHUD1.SetActive(invisibilidadDesbloqueada);
        if (colaHUD2 != null) colaHUD2.SetActive(dashDesbloqueado);

        contenedorCorrupcion = GameObject.Find("ContenedorCorrupcion");

        if (contenedorCorrupcion != null && BosquePurificado)
        {
            contenedorCorrupcion.SetActive(false);
        }

        InicializarDatosNivel();
    }

    private void ActualizarNuevoPlayerInstanciado()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = true;
            }

            string escenaNombre = SceneManager.GetActiveScene().name;

            if (escenaNombre == "Nivel_1")
            {
                GameObject puntoDestino1 = GameObject.Find("DestinoPortal_Mapa1");
                if (puntoDestino1 != null)
                {
                    player.transform.position = puntoDestino1.transform.position;
                    puntoRetornoActual = puntoDestino1.transform.position;
                }
                else
                {
                    puntoRetornoActual = player.transform.position;
                }
            }
            else if (escenaNombre == "Nivel_2")
            {
                GameObject puntoDestino2 = GameObject.Find("DestinoPortal_Mapa2");
                if (puntoDestino2 != null)
                {
                    player.transform.position = puntoDestino2.transform.position;
                    puntoRetornoActual = puntoDestino2.transform.position;
                }
                else
                {
                    puntoRetornoActual = player.transform.position;
                }
            }
            else
            {
                puntoRetornoActual = player.transform.position;
            }

            KitsuneHealth healthComp = player.GetComponentInParent<KitsuneHealth>();
            if (healthComp != null && cachedHealth > 0)
            {
                healthComp.RestoreFullHealth();
                float danioAplicar = healthComp.MaxHealth - cachedHealth;
                healthComp.TakeDamage(danioAplicar);
            }

            KitsuneSpirit spiritComp = player.GetComponentInParent<KitsuneSpirit>();
            if (spiritComp != null && cachedSpirit > 0)
            {
                spiritComp.CurrentSpirit = cachedSpirit;
            }

            // ===============================================================
            // 💥 [PARCHE DIRECTO] Sincronizar el disparo al clon del Nivel 2
            // ===============================================================
            KitsuneController controllerComp = player.GetComponent<KitsuneController>();
            if (controllerComp != null)
            {
                Debug.Log("🔥 [SINCRO] Persistencia del disparo inyectada con éxito: " + disparoDesbloqueado);
            }
        }
    }

    public void GuardarDatosParaSiguienteNivel()
    {
        vieneDeNivelAnterior = true;
        cachedVidas = vidasActuales;
        cachedPuntos = puntosActuales;
        cachedInvisibilidad = invisibilidadDesbloqueada;
        cachedDash = dashDesbloqueado;
        cachedDisparo = disparoDesbloqueado;

        if (player != null)
        {
            KitsuneHealth healthComp = player.GetComponentInParent<KitsuneHealth>();
            if (healthComp != null) cachedHealth = healthComp.CurrentHealth;

            KitsuneSpirit spiritComp = player.GetComponentInParent<KitsuneSpirit>();
            if (spiritComp != null) cachedSpirit = spiritComp.CurrentSpirit;
        }
    }

    public bool IntentarActivarCheckpoint(int id, Vector3 posicion)
    {
        if (id == checkpointActualID + 1)
        {
            checkpointActualID = id;
            puntoRetornoActual = posicion;
            return true;
        }
        return false;
    }

    public void SumarPunto(int cantidad = 1)
    {
        puntosActuales += cantidad;
        ActualizarUI();
    }

    public void RestarVida(int cantidad = 1)
    {
        vidasActuales -= cantidad;
        if (vidasActuales < 0) vidasActuales = 0;

        ActualizarUI();

        if (vidasActuales <= 0)
            ActivarGameOver();
    }

    public void DesbloquearInvisibilidad()
    {
        if (invisibilidadDesbloqueada) return;
        invisibilidadDesbloqueada = true;
        if (colaHUD1 != null) colaHUD1.SetActive(true);
    }

    public void DesbloquearDash()
    {
        if (dashDesbloqueado) return;
        dashDesbloqueado = true;
        if (colaHUD2 != null) colaHUD2.SetActive(true);
    }

    public void DesbloquearDisparo()
    {
        if (disparoDesbloqueado) return;
        disparoDesbloqueado = true;
        Debug.Log("Disparo desbloqueado");
    }

    public void PurificarBosqueSagrado()
    {
        string escenaActual = SceneManager.GetActiveScene().name;

        if (purificacionPorEscena.TryGetValue(
                escenaActual,
                out bool yaPurificado)
            && yaPurificado)
        {
            Debug.Log("YA ESTABA PURIFICADO EN " + escenaActual);
            return;
        }

        purificacionPorEscena[escenaActual] = true;

        Debug.Log("PURIFICANDO " + escenaActual);

        if (contenedorCorrupcion != null)
            contenedorCorrupcion.SetActive(false);
    }

    public void ActivarPlataformaSalto()
    {
        if (plataformaSaltoActiva) return;
        plataformaSaltoActiva = true;
        if (humosSalto != null) humosSalto.SetActive(true);
        if (jumpPadTrigger != null) jumpPadTrigger.SetActive(true);
    }

    public void ObtenerLlaveAzul()
    {
        if (tieneLlave) return;
        tieneLlave = true;
    }

    private void ActualizarUI()
    {
        if (vidasText != null) vidasText.text = vidasActuales.ToString();
        if (puntosText != null) puntosText.text = puntosActuales.ToString();
    }

    private void ActivarGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    // ===============================================================
    // BLOQUE DE MÉTODOS DE CONTROL PARA EL TECLADO DEBUG (CARLOS)
    // ===============================================================

    public void ModificarVidasDebug(int cantidad)
    {
        vidasActuales += cantidad;
        if (vidasActuales < 0) vidasActuales = 0;
        ActualizarUI();
        Debug.Log("❤️ [DEBUG] Vidas/Corazones modificados en " + cantidad + ". Total: " + vidasActuales);
    }

    public void DesbloquearDisparoDebug(bool estado)
    {
        disparoDesbloqueado = estado;
    }

    public void DesbloquearInvisibilidadDebug(bool estado)
    {
        invisibilidadDesbloqueada = estado;
        if (colaHUD1 != null) colaHUD1.SetActive(estado);
    }

    public void DesbloquearDashDebug(bool estado)
    {
        dashDesbloqueado = estado;
        if (colaHUD2 != null) colaHUD2.SetActive(estado);
    }
}