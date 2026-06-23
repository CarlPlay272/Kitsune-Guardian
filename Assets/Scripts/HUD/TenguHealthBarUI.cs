using UnityEngine;
using UnityEngine.UI;

public class TenguHealthBarUI : MonoBehaviour
{
    [Header("UI Componentes")]
    [SerializeField] private Image fillImageBoss;
    [SerializeField] private CanvasGroup canvasGroupBarra;

    [Header("Configuración de Proximidad")]
    [SerializeField] private float distanciaDeteccion = 12f; // Rango del radar para activar la barra
    [SerializeField] private float velocidadFade = 4f;

    private Transform jugadorTransform;
    private TenguState tenguActual; // Guarda el Tengu al que estamos enfrentando actualmente

    void Start()
    {
        if (canvasGroupBarra != null)
            canvasGroupBarra.alpha = 0f; // Inicia invisible de forma mística
    }

    void Update()
    {
        // Enlazar al jugador desde el GameController si se pierde la referencia
        if (jugadorTransform == null && GameController.Instance != null && GameController.Instance.Player != null)
        {
            jugadorTransform = GameController.Instance.Player.transform;
        }

        if (jugadorTransform == null) return;

        // 🔥 RADAR DINÁMICO: Busca al Tengu vivo más cercano en la escena
        BuscarTenguMasCercano();

        // Si encontramos un Tengu calificado y estamos cerca, procesamos la UI
        if (tenguActual != null && !tenguActual.IsDead)
        {
            float distancia = Vector2.Distance(transform.position, jugadorTransform.position);

            // Actualiza el porcentaje de llenado de la barra de forma limpia y directa
            if (fillImageBoss != null)
            {
                fillImageBoss.fillAmount = tenguActual.ObtenerPorcentajeVida();
            }

            // Aparece la barra si el jugador está en rango (independiente del candado de disparo si gustas)
            if (distancia <= distanciaDeteccion)
            {
                ControlarFade(1f); // Muestra barra suavemente
            }
            else
            {
                ControlarFade(0f); // Oculta barra si te alejas
            }
        }
        else
        {
            // Si no hay ningún Tengu cerca o el actual murió, desvanece la barra por completo
            ControlarFade(0f);
        }
    }

    private void BuscarTenguMasCercano()
    {
        // Busca todos los Tengus instanciados en el nivel actual
        TenguState[] todosLosTengus = FindObjectsByType<TenguState>(FindObjectsSortMode.None);
        float distanciaMasCorta = Mathf.Infinity;
        TenguState tenguMasCercano = null;

        foreach (TenguState tengu in todosLosTengus)
        {
            if (tengu == null || tengu.IsDead) continue;

            float distancia = Vector2.Distance(jugadorTransform.position, tengu.transform.position);
            if (distancia < distanciaMasCorta)
            {
                distanciaMasCorta = distancia;
                tenguMasCercano = tengu;
            }
        }

        // Asigna el objetivo dinámico si entra en el rango del radar
        if (distanciaMasCorta <= distanciaDeteccion)
        {
            tenguActual = tenguMasCercano;
        }
    }

    private void ControlarFade(float targetAlpha)
    {
        if (canvasGroupBarra != null)
        {
            canvasGroupBarra.alpha = Mathf.MoveTowards(
                canvasGroupBarra.alpha,
                targetAlpha,
                Time.deltaTime * velocidadFade
            );
        }
    }
}