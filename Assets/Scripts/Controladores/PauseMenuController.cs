using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance;

    [Header("Paneles de la Interfaz")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject bordeMisticoFondo;
    [SerializeField] private GameObject controlesPanel;
    [SerializeField] private GameObject loadingPanel;

    private bool isPaused = false;
    private bool bloqueadoPorDialogo = false;
    private AudioSource musicaFondo;

    public bool IsPaused => isPaused;

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
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        BuscarReferenciasPaneles();
        AsegurarEstadosPaneles(false);
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (loadingPanel != null && loadingPanel.activeSelf) return;

            if (isPaused)
                ContinuarJuego();
            else
                PausarJuego();
        }
    }

    private void BuscarReferenciasPaneles()
    {
        Canvas[] todosLosCanvas = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        GameObject hudObj = null;

        foreach (Canvas canvas in todosLosCanvas)
        {
            if (canvas.name == "HUD")
            {
                hudObj = canvas.gameObject;
                break;
            }
        }

        if (hudObj != null)
        {
            Transform panelPausaTransform = hudObj.transform.Find("PanelPausa");
            if (panelPausaTransform != null)
            {
                pauseMenuPanel = panelPausaTransform.gameObject;
                bordeMisticoFondo = panelPausaTransform.Find("BordeMisticoFondo")?.gameObject;
                controlesPanel = panelPausaTransform.Find("PanelControlesBeta")?.gameObject;
            }

            Transform panelCargaTransform = hudObj.transform.Find("PanelCarga");
            if (panelCargaTransform != null)
            {
                loadingPanel = panelCargaTransform.gameObject;
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BuscarReferenciasPaneles();
        AsegurarEstadosPaneles(false);
        if (loadingPanel != null) loadingPanel.SetActive(false);

        isPaused = false;
        bloqueadoPorDialogo = false;

        GameObject botonObj = GameObject.Find("BotonPausaUI");
        if (botonObj != null)
        {
            Button btn = botonObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(PausarJuego);
            }
        }

        if (bordeMisticoFondo != null)
        {
            Button btnContinuar = bordeMisticoFondo.transform.Find("Boton_Continuar")?.GetComponent<Button>();
            if (btnContinuar != null) { btnContinuar.onClick.RemoveAllListeners(); btnContinuar.onClick.AddListener(ContinuarJuego); }

            Button btnReiniciar = bordeMisticoFondo.transform.Find("Boton_Reiniciar")?.GetComponent<Button>();
            if (btnReiniciar != null) { btnReiniciar.onClick.RemoveAllListeners(); btnReiniciar.onClick.AddListener(ReiniciarDesdeCero); }

            Button btnOpciones = bordeMisticoFondo.transform.Find("Boton_Opciones")?.GetComponent<Button>();
            if (btnOpciones != null) { btnOpciones.onClick.RemoveAllListeners(); btnOpciones.onClick.AddListener(AbrirControlesOpciones); }

            Button btnSalir = bordeMisticoFondo.transform.Find("Boton_Salir")?.GetComponent<Button>();
            if (btnSalir != null) { btnSalir.onClick.RemoveAllListeners(); btnSalir.onClick.AddListener(SalirDelJuego); }
        }

        if (controlesPanel != null)
        {
            Button btnCerrarControles = controlesPanel.transform.Find("Boton_CerrarControles")?.GetComponent<Button>();
            if (btnCerrarControles != null) { btnCerrarControles.onClick.RemoveAllListeners(); btnCerrarControles.onClick.AddListener(CerrarControlesOpciones); }
        }
    }

    public void PausarJuego()
    {
        if (isPaused) return;

        BuscarReferenciasPaneles();
        if (pauseMenuPanel == null) return;

        isPaused = true;

        // Validar si hay un texto corriendo de verdad para congelar logs[cite: 5]
        IntroduccionInicio introActiva = Object.FindFirstObjectByType<IntroduccionInicio>();
        if (introActiva != null && introActiva.EstaReproduciendose)
        {
            bloqueadoPorDialogo = true;
            Debug.Log("💬 [HISTORIA] Pausa activada en medio de una cinemática de texto.");
        }
        else
        {
            bloqueadoPorDialogo = false;
        }

        pauseMenuPanel.SetActive(true);
        if (bordeMisticoFondo != null) bordeMisticoFondo.SetActive(true);
        if (controlesPanel != null) controlesPanel.SetActive(false);

        Time.timeScale = 0f;
        PausarMusicaEscena(true);
        BloquearKitsune(true);
    }

    public void ContinuarJuego()
    {
        if (!isPaused) return;

        isPaused = false;
        AsegurarEstadosPaneles(false);
        Time.timeScale = 1f;
        PausarMusicaEscena(false);

        // 🔥 MODIFICACIÓN COMPLETA: Preguntar al cerebro si la historia mantiene los controles amarrados[cite: 5, 7]
        if (GameController.Instance != null && GameController.Instance.ControlesBloqueadosPorHistoria)
        {
            // Forzar el bloqueo físico: el jugador NO se moverá aunque se despause[cite: 5, 7]
            BloquearKitsune(true);
            Debug.Log("🛡️ [HIERRO] Menú cerrado en medio de historia. Controles retenidos por GameController.");
        }
        else
        {
            // Gameplay normal: se devuelven los inputs con normalidad[cite: 5]
            BloquearKitsune(false);
            Debug.Log("🎮 [GAMEPLAY] Menú cerrado de forma normal. Inputs liberados.");
        }

        bloqueadoPorDialogo = false;
    }

    private void AsegurarEstadosPaneles(bool estado)
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(estado);
        if (bordeMisticoFondo != null) bordeMisticoFondo.SetActive(estado);
        if (controlesPanel != null) controlesPanel.SetActive(estado);
    }

    public void ReiniciarDesdeCero()
    {
        Time.timeScale = 1f;
        isPaused = false;
        bloqueadoPorDialogo = false;
        AsegurarEstadosPaneles(false);

        Debug.Log("🔄 [REINICIO] Solicitando carga asíncrona mística al LoadingManager para el Nivel_1.");

        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.CambiarEscenaMistica("Nivel_1");
        }
        else
        {
            SceneManager.LoadScene("Nivel_1");
        }
    }

    public void AbrirControlesOpciones()
    {
        if (bordeMisticoFondo != null) bordeMisticoFondo.SetActive(false);
        if (controlesPanel != null) controlesPanel.SetActive(true);
    }

    public void CerrarControlesOpciones()
    {
        if (controlesPanel != null) controlesPanel.SetActive(false);
        if (bordeMisticoFondo != null) bordeMisticoFondo.SetActive(true);
    }

    public void SalirDelJuego()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void BloquearKitsune(bool bloquear)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            KitsuneController controller = playerObj.GetComponent<KitsuneController>();
            if (controller != null)
            {
                if (bloquear) controller.BloquearControles();
                else controller.DesbloquearControles();
            }
        }
    }

    private void PausarMusicaEscena(bool pausar)
    {
        if (pausar)
        {
            GameObject objetoMusica = GameObject.Find("Music_Game") ?? GameObject.FindWithTag("MainCamera");
            if (objetoMusica != null)
            {
                musicaFondo = objetoMusica.GetComponent<AudioSource>();
                if (musicaFondo != null && musicaFondo.isPlaying)
                {
                    musicaFondo.Pause();
                }
            }
        }
        else
        {
            if (musicaFondo != null)
            {
                musicaFondo.UnPause();
            }
        }
    }
}