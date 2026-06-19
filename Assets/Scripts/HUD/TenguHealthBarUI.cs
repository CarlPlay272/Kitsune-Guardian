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
    [SerializeField] private float distanciaDeteccion = 18f;
    [SerializeField] private float velocidadFade = 3f;

    private Transform jugadorTransform;

    void Start()
    {
        if (canvasGroupBarra != null)
            canvasGroupBarra.alpha = 0f; // Empezar invisible
    }

    void Update()
    {
        // Enlazar al jugador desde GameController
        if (jugadorTransform == null && GameController.Instance != null && GameController.Instance.Player != null)
        {
            jugadorTransform = GameController.Instance.Player.transform;
        }

        // Si el jefe ya está muerto, ocultar barra
        if (tenguState == null || tenguState.IsDead)
        {
            ControlarFade(0f);
            return;
        }

        // Actualizar vida del boss
        if (fillImageBoss != null)
        {
            try
            {
                System.Reflection.FieldInfo fieldCurrent =
                    typeof(TenguState).GetField("currentHealth",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                System.Reflection.FieldInfo fieldMax =
                    typeof(TenguState).GetField("maxHealth",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

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

        // Sistema de aparición
        if (jugadorTransform != null)
        {
            float distancia = Vector2.Distance(
                tenguState.transform.position,
                jugadorTransform.position
            );

            // SOLO aparece si tienes el disparo desbloqueado y estás cerca del Tengu
            if (GameController.Instance != null &&
                GameController.Instance.DisparoDesbloqueado &&
                distancia <= distanciaDeteccion)
            {
                ControlarFade(1f); // Mostrar barra
            }
            else
            {
                ControlarFade(0f); // Ocultar barra
            }
        }
    }

    private void ControlarFade(float targetAlpha)
    {
        if (canvasGroupBarra != null)
        {
            canvasGroupBarra.alpha =
                Mathf.MoveTowards(
                    canvasGroupBarra.alpha,
                    targetAlpha,
                    Time.deltaTime * velocidadFade
                );
        }
    }
}