using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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
    [SerializeField] private bool bosquePurificado = false;

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
    private static bool vieneDeNivelAnterior = false;

    // Propiedades Públicas
    public GameObject Player => player;
    public float LimiteIzquierdo => limiteIzquierdo != null ? limiteIzquierdo.position.x : 0f;
    public float LimiteDerecho => limiteDerecho != null ? limiteDerecho.position.x : 0f;
    public int VidasActuales => vidasActuales;
    public int PuntosActuales => puntosActuales;
    public bool BosquePurificado => bosquePurificado;
    public bool TieneLlave => tieneLlave;
    public bool InvisibilidadDesbloqueada => invisibilidadDesbloqueada;
    public bool DashDesbloqueado => dashDesbloqueado;
    public bool DisparoDesbloqueado => disparoDesbloqueado;
    public bool PlataformaSaltoActiva => plataformaSaltoActiva;

    public Vector3 PuntoRetornoActual => puntoRetornoActual;
    public int CheckpointActualID => checkpointActualID;

    void Awake()
    {
        // SISTEMA SINGLETON CONTROLADO CON EVENTO DE ESCENAS
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // El controlador se vuelve eterno

        // Suscribirse al evento de carga de escena de Unity para inyectar datos al nuevo mapa
        SceneManager.sceneLoaded += OnSceneLoaded;

        InicializarDatosNivel();
    }

    void OnDestroy()
    {
        // Desuscripción obligatoria para evitar fugas de memoria
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InicializarDatosNivel()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Si es la primera vez que arranca el juego completo
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
            // Inyectar la data del nivel anterior guardada en el caché estático
            vidasActuales = cachedVidas;
            puntosActuales = cachedPuntos;
            invisibilidadDesbloqueada = cachedInvisibilidad;
            dashDesbloqueado = cachedDash;

            // Buscar y actualizar al nuevo Kitsune de esta escena
            ActualizarNuevoPlayerInstanciado();
        }

        ActualizarUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Volver a buscar las referencias de UI del nuevo nivel cargado ya que las viejas se destruyeron
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

        // Reconfigurar los estados visuales en el nuevo mapa
        if (colaHUD1 != null) colaHUD1.SetActive(invisibilidadDesbloqueada);
        if (colaHUD2 != null) colaHUD2.SetActive(dashDesbloqueado);

        InicializarDatosNivel();
    }

    private void ActualizarNuevoPlayerInstanciado()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // 1. BLINDAJE DE FÍSICAS Y RENDERER (Evita que el zorro aparezca tiezo o invisible)
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic; // Forzar a dinámico para devolver el control físico
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = true; // Forzar el encendido visual del sprite en la nueva escena
            }

            string escenaNombre = SceneManager.GetActiveScene().name;

            // 2. ENLAZADO DINÁMICO DE SPAWNS MEDIANTE GAME OBJECTS EXACTOS (GEMINI)
            if (escenaNombre == "Nivel_1")
            {
                GameObject puntoDestino1 = GameObject.Find("DestinoPortal_Mapa1");
                if (puntoDestino1 != null)
                {
                    player.transform.position = puntoDestino1.transform.position;
                    puntoRetornoActual = puntoDestino1.transform.position;
                    Debug.Log("↩️ [BACKTRACKING] Kitsune llevado con éxito a: DestinoPortal_Mapa1");
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
                    Debug.Log("🚀 [AVANCE] Kitsune llevado con éxito a: DestinoPortal_Mapa2");
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

            // Inyectar Vida guardada
            KitsuneHealth healthComp = player.GetComponentInParent<KitsuneHealth>();
            if (healthComp != null && cachedHealth > 0)
            {
                healthComp.RestoreFullHealth();
                float danioAplicar = healthComp.MaxHealth - cachedHealth;
                healthComp.TakeDamage(danioAplicar);
            }

            // Inyectar Energía/Espíritu guardado
            KitsuneSpirit spiritComp = player.GetComponentInParent<KitsuneSpirit>();
            if (spiritComp != null && cachedSpirit > 0)
            {
                spiritComp.CurrentSpirit = cachedSpirit;
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
        if (disparoDesbloqueado)
            return;

        disparoDesbloqueado = true;

        Debug.Log("Disparo desbloqueado");
    }

    public void PurificarBosqueSagrado()
    {
        if (bosquePurificado) return;

        bosquePurificado = true;

        if (contenedorCorrupcion != null)
        {
            contenedorCorrupcion.SetActive(false);
        }

        // ❌ Dash eliminado para Nivel 1
        // Se desbloqueará en Nivel 2 mediante otro evento
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