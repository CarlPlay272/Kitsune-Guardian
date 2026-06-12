using System.Collections;
using UnityEngine;
using TMPro;

public class AvisoPurificadorConsecutivo : MonoBehaviour
{
    [Header("Referencias UI Internas")]
    [SerializeField] private TextMeshPro textMeshPro;

    [Header("Textos Consecutivos (Lore + Dash)")]
    [TextArea(2, 5)][SerializeField] private string mensajeLore1 = "¡Increíble! He purificado la niebla corrupta de este sector sagrado... Siento cómo la arboleda vuelve a respirar.";
    [TextArea(2, 5)][SerializeField] private string mensajeTutorial2 = "¡La energía del bosque me ha otorgado el Dash! Puedo moverme a ráfagas veloces. (Presiona rápidamente Dos veces 'A' o 'D' para activar el Dash).";

    [Header("Configuración de Tiempos")]
    [SerializeField] private float tiempoMensaje1 = 4.0f;
    [SerializeField] private float tiempoMensaje2 = 5.5f;
    [SerializeField] private float velocidadFade = 2f;

    private bool secuenciaDisparada = false;

    void Start()
    {
        if (textMeshPro == null)
            textMeshPro = GetComponentInChildren<TextMeshPro>();

        if (textMeshPro != null)
        {
            textMeshPro.text = ""; // Asegurar vacío absoluto al iniciar
            SetTextoAlpha(0f);     // Forzar invisibilidad completa
        }
    }

    void Update()
    {
        if (secuenciaDisparada) return;

        // Monitorear de forma segura el estado del GameController global sin depender de colisionadores
        if (GameController.Instance != null && GameController.Instance.BosquePurificado)
        {
            secuenciaDisparada = true; // Bloquear actualización inmediata
            StartCoroutine(SecuenciaMensajesConsecutivosRoutine());
        }
    }

    private IEnumerator SecuenciaMensajesConsecutivosRoutine()
    {
        if (textMeshPro == null) yield break;

        float alpha = 0f;

        // ----- TEXTO 1: CELEBRACIÓN PURIFICACIÓN (LORE) -----
        textMeshPro.text = mensajeLore1;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * velocidadFade;
            SetTextoAlpha(alpha);
            yield return null;
        }
        SetTextoAlpha(1f);

        yield return new WaitForSeconds(tiempoMensaje1);

        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * velocidadFade;
            SetTextoAlpha(alpha);
            yield return null;
        }
        SetTextoAlpha(0f);

        yield return new WaitForSeconds(0.3f); // Pequeño respiro estético en negro

        // ----- TEXTO 2: APRENDIZAJE DEL DASH (TUTORIAL) -----
        textMeshPro.text = mensajeTutorial2;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * velocidadFade;
            SetTextoAlpha(alpha);
            yield return null;
        }
        SetTextoAlpha(1f);

        yield return new WaitForSeconds(tiempoMensaje2);

        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * velocidadFade;
            SetTextoAlpha(alpha);
            yield return null;
        }
        SetTextoAlpha(0f);

        // Desactivar el GameObject completo del feedback para ahorrar rendimiento
        gameObject.SetActive(false);
    }

    private void SetTextoAlpha(float a)
    {
        if (textMeshPro != null)
        {
            Color c = textMeshPro.color;
            c.a = Mathf.Clamp01(a);
            textMeshPro.color = c;
        }
    }
}