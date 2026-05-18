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

    private int vidasActuales;
    private int puntosActuales;

    public GameObject Player => player;
    public float LimiteIzquierdo => limiteIzquierdo != null ? limiteIzquierdo.position.x : 0f;
    public float LimiteDerecho => limiteDerecho != null ? limiteDerecho.position.x : 0f;
    public int VidasActuales => vidasActuales;
    public int PuntosActuales => puntosActuales;

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

    public void SumarPunto(int cantidad = 1)
    {
        puntosActuales += cantidad;
        ActualizarUI();
    }

    public void RestarVida(int cantidad = 1)
    {
        vidasActuales -= cantidad;

        if (vidasActuales < 0)
            vidasActuales = 0;

        ActualizarUI();

        if (vidasActuales <= 0)
        {
            ActivarGameOver();
        }
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