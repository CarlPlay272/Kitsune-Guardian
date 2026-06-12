using System.Collections;
using UnityEngine;
using TMPro;

public class AvisoInvisibilidad : MonoBehaviour
{
    [Header("Configuración del Mensaje")]
    [TextArea(2, 5)]
    [SerializeField] private string textoMensaje = "Siento una vibra extraña... ¿Acaso dejé algo atrás en las ramas superiores? (Te faltó algo, retrocede)";
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
            textMeshPro.text = textoMensaje;
            // Forzar que empiece completamente invisible (Alpha 0)
            Color c = textMeshPro.color;
            c.a = 0f;
            textMeshPro.color = c;
        }
        else
        {
            Debug.LogError("AvisoInvisibilidad: No se encontró un componente TextMeshPro en los hijos.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // CONTROL CONDICIONAL: Si el jugador YA tiene la invisibilidad, no muestra nada y sale
        if (GameController.Instance != null && GameController.Instance.InvisibilidadDesbloqueada)
        {
            return;
        }

        // Verificar si es el Kitsune usando el script de salud core
        KitsuneHealth jugador = other.GetComponentInParent<KitsuneHealth>();
        if (jugador == null || jugador.IsDead) return;

        if (!jugadorAdentro)
        {
            jugadorAdentro = true;
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

        // Efecto Fade In (Aparecer suavemente)
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * velocidadFade;
            DefinirAlpha(alpha);
            yield return null;
        }
        DefinirAlpha(1f);

        // Esperar el tiempo configurado flotando en pantalla
        yield return new WaitForSeconds(tiempoVisible);

        // Efecto Fade Out automático por tiempo si el jugador sigue ahí parado
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