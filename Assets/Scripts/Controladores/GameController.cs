using UnityEngine;
using TMPro;

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
    [SerializeField] private GameObject colaHUD1; // ñes/HUD/TailIcons/ColaHUD_01_Invisibilidad
    [SerializeField] private GameObject colaHUD2; // ñes/HUD/TailIcons/ColaHUD_02_Dash

    [Header("Estado")]
    [SerializeField] private int vidasIniciales = 3;

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

    [Header("Llave del nivel")]
    [SerializeField] private bool tieneLlave = false;

    [Header("Sistema de Checkpoints Secuenciales (GEMINI)")]
    [SerializeField] private int checkpointActualID = 0; // 0 es el inicio de la escena
    [SerializeField] private Vector3 puntoRetornoActual;

    private int vidasActuales;
    private int puntosActuales;

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
    public bool PlataformaSaltoActiva => plataformaSaltoActiva;
    
    // Getters para el sistema de control secuencial
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

        vidasActuales = vidasIniciales;
        puntosActuales = 0;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Inicializar la reaparición en la posición en la que arranca el Player en el mapa
        if (player != null)
        {
            puntoRetornoActual = player.transform.position;
        }

        ActualizarUI();
    }

    void Start()
    {
        if (humosSalto != null)
            humosSalto.SetActive(plataformaSaltoActiva);

        if (jumpPadTrigger != null)
            jumpPadTrigger.SetActive(plataformaSaltoActiva);

        if (colaHUD1 != null)
            colaHUD1.SetActive(invisibilidadDesbloqueada);

        if (colaHUD2 != null)
            colaHUD2.SetActive(dashDesbloqueado);
    }

    // Método que valida la secuencia e impide activar puntos anteriores o salteados
    public bool IntentarActivarCheckpoint(int id, Vector3 posicion)
    {
        // Regla estricta: Solo se activa si es EXACTAMENTE el siguiente en el orden numérico (Metroidvania)
        if (id == checkpointActualID + 1)
        {
            checkpointActualID = id;
            puntoRetornoActual = posicion;
            Debug.Log("¡Progreso Guardado con éxito! Ahora el Checkpoint activo es el ID: " + id);
            return true;
        }
        
        Debug.LogWarning("Intento de activación denegado. Checkpoint ID " + id + " no corresponde a la secuencia actual (ID Global: " + checkpointActualID + ")");
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

        if (colaHUD1 != null)
            colaHUD1.SetActive(true);

        Debug.Log("¡Invisibilidad desbloqueada!");
    }

    public void DesbloquearDash()
    {
        if (dashDesbloqueado) return;

        dashDesbloqueado = true;

        if (colaHUD2 != null)
            colaHUD2.SetActive(true);

        Debug.Log("¡Dash desbloqueado!");
    }

    public void PurificarBosqueSagrado()
    {
        if (bosquePurificado) return;

        bosquePurificado = true;

        if (contenedorCorrupcion != null)
            contenedorCorrupcion.SetActive(false);
        else
            Debug.LogWarning("GameController: contenedorCorrupcion no asignado.");

        DesbloquearDash();

        Debug.Log("¡Bosque purificado y dash desbloqueado!");
    }

    public void ActivarPlataformaSalto()
    {
        if (plataformaSaltoActiva) return;

        plataformaSaltoActiva = true;

        if (humosSalto != null)
            humosSalto.SetActive(true);
        else
            Debug.LogWarning("GameController: humosSalto no asignado.");

        if (jumpPadTrigger != null)
            jumpPadTrigger.SetActive(true);
        else
            Debug.LogWarning("GameController: jumpPadTrigger no asignado.");

        Debug.Log("¡Tengu derrotado! Plataforma de salto activada.");
    }

    public void ObtenerLlaveAzul()
    {
        if (tieneLlave) return;

        tieneLlave = true;
        Debug.Log("¡Llave azul obtenida!");
    }

    private void ActualizarUI()
    {
        if (vidasText != null)
            vidasText.text = vidasActuales.ToString();

        if (puntosText != null)
            puntosText.text = puntosActuales.ToString();
    }

    private void ActivarGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }
}