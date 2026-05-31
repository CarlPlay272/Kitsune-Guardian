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

    [Header("Estado")]
    [SerializeField] private int vidasIniciales = 3;

    [Header("Purificación")]
    [SerializeField] private GameObject contenedorCorrupcion;
    [SerializeField] private GameObject humosSalto;
    [SerializeField] private GameObject jumpPadTrigger;
    [SerializeField] private bool bosquePurificado = false;

    [Header("Llave del nivel")]
    [SerializeField] private bool tieneLlave = false;

    private int vidasActuales;
    private int puntosActuales;

    public GameObject Player => player;
    public float LimiteIzquierdo => limiteIzquierdo != null ? limiteIzquierdo.position.x : 0f;
    public float LimiteDerecho => limiteDerecho != null ? limiteDerecho.position.x : 0f;
    public int VidasActuales => vidasActuales;
    public int PuntosActuales => puntosActuales;
    public bool BosquePurificado => bosquePurificado;
    public bool TieneLlave => tieneLlave;

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

        ActualizarUI();
    }

    void Start()
    {
        if (humosSalto != null)
        {
            humosSalto.SetActive(false);
            Debug.Log("HumosSalto apagado al iniciar.");
        }
        else
        {
            Debug.LogWarning("GameController: humosSalto NO está asignado en el Inspector.");
        }

        if (jumpPadTrigger != null)
        {
            jumpPadTrigger.SetActive(false);
            Debug.Log("JumpPadTrigger apagado al iniciar.");
        }
        else
        {
            Debug.LogWarning("GameController: jumpPadTrigger NO está asignado en el Inspector.");
        }
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

    public void PurificarBosqueSagrado()
    {
        if (bosquePurificado) return;
        bosquePurificado = true;

        if (contenedorCorrupcion != null)
            contenedorCorrupcion.SetActive(false);
        else
            Debug.LogWarning("GameController: contenedorCorrupcion no asignado.");

        if (humosSalto != null)
        {
            humosSalto.SetActive(true);
            Debug.Log("HumosSalto ACTIVADO.");
        }
        else
            Debug.LogWarning("GameController: humosSalto no asignado al purificar.");

        if (jumpPadTrigger != null)
        {
            jumpPadTrigger.SetActive(true);
            Debug.Log("JumpPadTrigger ACTIVADO.");
        }
        else
            Debug.LogWarning("GameController: jumpPadTrigger no asignado al purificar.");

        Debug.Log("¡Bosque purificado y plataforma de salto activada!");
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