using UnityEngine;
using UnityEngine.UI;

public class TenguHealthBarUI : MonoBehaviour
{
    [Header("Referencias del Jefe")]
    [SerializeField] private TenguState tenguState;

    [Header("UI Componentes (Barra Antigua)")]
    [SerializeField] private Image fillImageBoss;
    [SerializeField] private CanvasGroup canvasGroupBarra;

    [Header("Configuración de Proximidad")]
    [SerializeField] private float distanciaDeteccion = 18f; // Rango óptimo para que abarque la caída a la fosa
    [SerializeField] private float velocidadFade = 3f;

    private Transform jugadorTransform;

    void Start()
    {
        if (canvasGroupBarra != null)
            canvasGroupBarra.alpha = 0f; // Empezar invisible
    }

    void Update()
    {
        // Enlazar al Kitsune usando de puente el GameController de forma dinámica
        if (jugadorTransform == null && GameController.Instance != null && GameController.Instance.Player != null)
        {
            jugadorTransform = GameController.Instance.Player.transform;
        }

        // Si el jefe ya pasó a mejor vida, desvanecemos la barra del HUD
        if (tenguState == null || tenguState.IsDead)
        {
            ControlarFade(0f);
            return;
        }

        // Leer los valores de salud encapsulados mediante Reflection
        if (fillImageBoss != null)
        {
            try
            {
                System.Reflection.FieldInfo fieldCurrent = typeof(TenguState).GetField("currentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                System.Reflection.FieldInfo fieldMax = typeof(TenguState).GetField("maxHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (fieldCurrent != null && fieldMax != null)
                {
                    int current = (int)fieldCurrent.GetValue(tenguState);
                    int max = (int)fieldMax.GetValue(tenguState);

                    fillImageBoss.fillAmount = (float)current / max;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("TenguHealthBarUI: Error de Reflection -> " + e.Message);
            }
        }

        // Manejar el sistema de proximidad
        if (jugadorTransform != null)
        {
            float distancia = Vector2.Distance(tenguState.transform.position, jugadorTransform.position);

            // Condición estricta: Solo aparece en proximidad Y si el Kitsune ya posee el Dash (Paso 2 terminado)
            if (distancia <= distanciaDeteccion && GameController.Instance.DashDesbloqueado)
            {
                ControlarFade(1f); // Fade In
            }
            else
            {
                ControlarFade(0f); // Fade Out
            }
        }
    }

    private void ControlarFade(float targetAlpha)
    {
        if (canvasGroupBarra != null)
        {
            canvasGroupBarra.alpha = Mathf.MoveTowards(canvasGroupBarra.alpha, targetAlpha, Time.deltaTime * velocidadFade);
        }
    }
}