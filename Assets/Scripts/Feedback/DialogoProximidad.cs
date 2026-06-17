using System.Collections;
using UnityEngine;
using TMPro;

public class DialogoProximidad : MonoBehaviour
{
    [Header("Configuración de Mensajes Dinámicos")]
    [TextArea(2, 5)]
    [SerializeField] private string textoAntesPurificar = "Siento una densa energía oscura emanando de esta niebla... Cruzar a la fuerza solo destrozará mi esencia espiritual. Debo buscar otra entrada.";
    [TextArea(2, 5)]
    [SerializeField] private string textoDespuesPurificar = "¡Excelente! La niebla oscura se ha disipado por completo. El camino hacia las profundidades del bosque finalmente está libre.";

    [Header("Configuración de Tiempos")]
    [SerializeField] private float tiempoVisible = 4.0f;
    [SerializeField] private float velocidadFade = 2f;

    [Header("Referencias UI Internas")]
    [SerializeField] private TextMeshPro textMeshPro;

    private Coroutine coroutineActual;
    private bool jugadorAdentro = false;

    void Start()
    {
        if (textMeshPro == null)
            textMeshPro = GetComponentInChildren<TextMeshPro>();

        if (textMeshPro != null)
        {
            // Forzar que empiece completamente invisible y limpio (Alpha 0)
            textMeshPro.text = "";
            Color c = textMeshPro.color;
            c.a = 0f;
            textMeshPro.color = c;
        }
        else
        {
            Debug.LogError("DialogoProximidad: No se encontró un componente TextMeshPro en los hijos.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        KitsuneHealth jugador = other.GetComponentInParent<KitsuneHealth>();
        if (jugador == null || jugador.IsDead) return;

        if (!jugadorAdentro)
        {
            jugadorAdentro = true;

            // EVALUACIÓN DINÁMICA: Revisamos si el bosque ya fue liberado en el GameController
            if (GameController.Instance != null && GameController.Instance.BosquePurificado)
            {
                textMeshPro.text = textoDespuesPurificar;
            }
            else
            {
                textMeshPro.text = textoAntesPurificar;
            }

            if (coroutineActual != null) StopCoroutine(coroutineActual);
            coroutineActual = StartCoroutine(MostrarTextoRoutine());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        KitsuneHealth jugador = other.GetComponentInParent<KitsuneHealth>();
        if (jugador == null) return;

        if (jugadorAdentro)
        {
            jugadorAdentro = false;
            if (coroutineActual != null) StopCoroutine(coroutineActual);
            coroutineActual = StartCoroutine(OcultarTextoRoutine(0f)); // Se desvanece rápido al alejarse
        }
    }

    private IEnumerator MostrarTextoRoutine()
    {
        float alpha = textMeshPro.color.a;

        while (alpha < 1f)
        {
            alpha += Time.deltaTime * velocidadFade;
            DefinirAlpha(alpha);
            yield return null;
        }
        DefinirAlpha(1f);

        yield return new WaitForSeconds(tiempoVisible);
        yield return OcultarTextoRoutine(velocidadFade);
    }

    private IEnumerator OcultarTextoRoutine(float velocidad)
    {
        float alpha = textMeshPro.color.a;

        if (velocidad > 0f)
        {
            while (alpha > 0f)
            {
                alpha -= Time.deltaTime * velocidad;
                DefinirAlpha(alpha);
                yield return null;
            }
        }

        DefinirAlpha(0f);
    }

    private void DefinirAlpha(float valorAlpha)
    {
        if (textMeshPro != null)
        {
            Color c = textMeshPro.color;
            c.a = Mathf.Clamp01(valorAlpha);
            textMeshPro.color = c;
        }
    }
}