using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    [Header("Referencias de Interfaz (UI)")]
    [SerializeField] private GameObject panelCargaGlobal; // Objeto 'PanelCarga'
    [SerializeField] private CanvasGroup canvasGroupCarga; // Componente para controlar la transparencia

    [Header("Configuración de Tiempos")]
    [SerializeField] private float tiempoMinimoPantalla = 2.5f; // Cuántos segundos se queda fija la pantalla
    [SerializeField] private float velocidadDesvanecido = 2.0f; // Qué tan rápido se desvanece (Fade Out)

    private bool estaCargando = false;

    void Awake()
    {
        // Regla Singleton estricta para evitar clones molestos
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Hace que el objeto viaje de pana entre niveles

        if (canvasGroupCarga == null && panelCargaGlobal != null)
        {
            canvasGroupCarga = panelCargaGlobal.GetComponent<CanvasGroup>();
            if (canvasGroupCarga == null)
            {
                canvasGroupCarga = panelCargaGlobal.AddComponent<CanvasGroup>();
            }
        }
    }

    void Start()
    {
        // Si le das PLAY directo en el Nivel 2 o 3 desde el editor de Unity:
        // Como el panel viene encendido por defecto, ejecutamos la espera y el desvanecido local al tiro
        if (panelCargaGlobal != null && panelCargaGlobal.activeSelf && SceneManager.GetActiveScene().name != "Menu")
        {
            StartCoroutine(RutinaDesvanecerPantallaInicial());
        }
    }

    /// <summary>
    /// Flujo para cuando le das PLAY directo a un nivel en el editor de Unity
    /// </summary>
    private IEnumerator RutinaDesvanecerPantallaInicial()
    {
        ManejarControlesKitsune(true); // Congelar al Kitsune al arrancar
        if (canvasGroupCarga != null) canvasGroupCarga.alpha = 1f; // Totalmente visible

        // Esperar los segundos del consejo en tiempo real
        yield return new WaitForSecondsRealtime(tiempoMinimoPantalla);

        // 🎭 EFECTO FADE OUT (Desvanecer)
        if (canvasGroupCarga != null)
        {
            while (canvasGroupCarga.alpha > 0f)
            {
                canvasGroupCarga.alpha -= velocidadDesvanecido * Time.unscaledDeltaTime;
                yield return null;
            }
            canvasGroupCarga.alpha = 0f;
        }

        if (panelCargaGlobal != null) panelCargaGlobal.SetActive(false);
        ManejarControlesKitsune(false); // Liberar al zorro
    }

    /// <summary>
    /// Punto de entrada oficial para el botón Jugar del Menú y los Portales de Meta
    /// </summary>
    public void CambiarEscenaMistica(string nombreEscena)
    {
        if (!estaCargando)
        {
            StartCoroutine(RutinaDeCargaAsincrona(nombreEscena));
        }
    }

    private IEnumerator RutinaDeCargaAsincrona(string nombreEscena)
    {
        estaCargando = true;

        // 1. Re-vincular referencias por si cambiamos de nivel y el HUD es nuevo
        BuscarReferenciasLocales();

        // 2. Forzar el encendido de la pantalla de carga en la escena actual antes de saltar
        if (panelCargaGlobal != null)
        {
            if (canvasGroupCarga != null) canvasGroupCarga.alpha = 1f;
            panelCargaGlobal.SetActive(true);
        }

        // Dejar un frame para que Unity renderice el fondo místico antes de congelar la memoria
        yield return null;

        // Calcular el segundo exacto en tiempo real donde se cumplirá la espera mínima del consejo
        float tiempoParaLiberar = Time.realtimeSinceStartup + tiempoMinimoPantalla;

        // 3. Cargar la escena en segundo plano de forma asíncrona profunda
        AsyncOperation operacionCarga = SceneManager.LoadSceneAsync(nombreEscena);
        
        // Bloqueamos la activación automática para que Unity NO cambie de nivel bruscamente
        operacionCarga.allowSceneActivation = false;

        // 4. El bucle sagrado: se queda atrapado aquí mientras el mapa cargue en RAM O falte tiempo visual
        while (operacionCarga.progress < 0.9f || Time.realtimeSinceStartup < tiempoParaLiberar)
        {
            // El círculo pixel art sigue girando de pana aquí gracias a las corrutinas
            yield return null;
        }

        // 5. Se cumplió el tiempo y el mapa está cargado: Damos luz verde para activar el nivel
        operacionCarga.allowSceneActivation = true;

        // Esperar a que la escena destino se monte completamente en el monitor
        while (!operacionCarga.isDone)
        {
            yield return null;
        }

        // 6. Al despertar en la nueva escena, volvemos a escanear el HUD local recién cargado
        BuscarReferenciasLocales();

        // Congelar al Kitsune inmediatamente en el nuevo mapa para que no camine a ciegas
        ManejarControlesKitsune(true);
        if (panelCargaGlobal != null) panelCargaGlobal.SetActive(true);
        if (canvasGroupCarga != null) canvasGroupCarga.alpha = 1f; // Mantener visible el fondo místico

        // Pequeña espera de cortesía para estabilizar la cámara del nuevo nivel
        yield return new WaitForSecondsRealtime(0.2f);

        // 7. 🎭 EFECTO FADE OUT: Desvanecer la pantalla de carga suavemente
        if (canvasGroupCarga != null)
        {
            while (canvasGroupCarga.alpha > 0f)
            {
                canvasGroupCarga.alpha -= velocidadDesvanecido * Time.unscaledDeltaTime;
                yield return null;
            }
            canvasGroupCarga.alpha = 0f;
        }

        // 8. Desactivar el panel para poder jugar tranquilos
        if (panelCargaGlobal != null)
        {
            panelCargaGlobal.SetActive(false);
        }

        // Liberar al Kitsune: ¡La pantalla desapareció por completo, a correr!
        ManejarControlesKitsune(false);
        
        estaCargando = false;
        Debug.Log($"⏳ [Transición Completa] Cambiado a {nombreEscena} con éxito.");
    }

    private void BuscarReferenciasLocales()
    {
        // Busca el Canvas 'HUD' de la escena actual para no quedar apuntando al nivel viejo
        Canvas[] todosLosCanvas = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in todosLosCanvas)
        {
            if (canvas.name == "HUD")
            {
                Transform panelCargaTransform = canvas.transform.Find("PanelCarga");
                if (panelCargaTransform != null)
                {
                    panelCargaGlobal = panelCargaTransform.gameObject;
                    canvasGroupCarga = panelCargaGlobal.GetComponent<CanvasGroup>();
                    if (canvasGroupCarga == null)
                    {
                        canvasGroupCarga = panelCargaGlobal.AddComponent<CanvasGroup>();
                    }
                }
                break;
            }
        }
    }

    private void ManejarControlesKitsune(bool bloquear)
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
}