using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance;

    [Header("Paneles de la Interfaz")]
    [SerializeField] private GameObject pauseMenuPanel;    // El objeto 'PanelPausa'
    [SerializeField] private GameObject bordeMisticoFondo; // La caja 'BordeMisticoFondo'
    [SerializeField] private GameObject controlesPanel;    // La pestaña 'PanelControlesBeta'
    [SerializeField] private GameObject loadingPanel;      // El panel de carga

    private bool isPaused = false;
    private bool bloqueadoPorDialogo = false; // Candado para los textos de la historia
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
        // Forzar escaneo en la primera escena activa
        BuscarReferenciasPaneles();
        AsegurarEstadosPaneles(false);
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Evitar conflictos si el panel de carga está activo
            if (loadingPanel != null && loadingPanel.activeSelf) return;

            if (isPaused)
                ContinuarJuego();
            else
                PausarJuego();
        }
    }

    private void BuscarReferenciasPaneles()
    {
        // 🔥 SOLUCIÓN BLINDADA: Escanea todos los Canvas de la escena para encontrar el HUD, sin importar su jerarquía
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
            // transform.Find encuentra los subpaneles aunque estén ocultos o anidados
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

        // Resetear estados lógicos al cambiar de mapa
        isPaused = false;
        bloqueadoPorDialogo = false;

        // ===============================================================
        // REVINCULACIÓN AUTOMÁTICA DE TODOS LOS BOTONES DE LA ESCENA
        // ===============================================================
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

    // ===============================================================
    // LÓGICA CORE DE PAUSA
    // ===============================================================
    public void PausarJuego()
    {
        if (isPaused) return; // Antispam: Evita que se congele doble si se presiona muy rápido

        BuscarReferenciasPaneles(); // Asegurar escaneo fresco antes de abrir
        if (pauseMenuPanel == null) return;

        isPaused = true;

        // Escaneo dinámico forzado en el microsegundo exacto de la pausa
        IntroduccionInicio introActiva = Object.FindFirstObjectByType<IntroduccionInicio>();
        if (introActiva != null)
        {
            bloqueadoPorDialogo = true;
            Debug.Log("💬 [HISTORIA] Texto de Naya detectado al pausar por primera vez. Controles protegidos.");
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
        BloquearKitsune(true); // Congelar inputs del Kitsune de pana
    }

    public void ContinuarJuego()
    {
        if (!isPaused) return; // Antispam

        isPaused = false;
        AsegurarEstadosPaneles(false);
        Time.timeScale = 1f;
        PausarMusicaEscena(false);

        // APLICACIÓN DEL CANDADO INTELIGENTE
        if (bloqueadoPorDialogo)
        {
            Debug.Log("💬 [HISTORIA] Menú cerrado. El Kitsune permanece inmóvil hasta terminar de leer.");
        }
        else
        {
            // Gameplay normal: Se mueve de inmediato
            BloquearKitsune(false);
        }

        bloqueadoPorDialogo = false; // Reset de seguridad
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

        // 🔥 MODIFICACIÓN: Conexión con el nuevo flujo de carga asíncrona con desvanecido
        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.CambiarEscenaMistica("Nivel_1");
        }
        else
        {
            // Respaldo por si se prueba la interfaz de forma aislada
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
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            KitsuneController controller = player.GetComponent<KitsuneController>();
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