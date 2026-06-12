using System.Collections;
using UnityEngine;
using TMPro;

public class AvisoTenguCondicional : MonoBehaviour
{
    [Header("Textos del Lore")]
    [TextArea(2, 5)]
    [SerializeField] private string textoConInvisibilidad = "Ese es el gran Tengu... Siento que si activo mi Invisibilidad (Teclado: Q) podré colarme por su espalda sin que note mi presencia.";
    [TextArea(2, 5)]
    [SerializeField] private string textoSinInvisibilidad = "¡Un momento! Ese demonio es invencible en mi estado actual... Siento que la corrupción me destruirá si me acerco. No debí bajar corriendo. (Mantén presionada R para reiniciar el nivel).";

    [Header("Configuración")]
    [SerializeField] private float tiempoVisible = 4.5f;
    [SerializeField] private float velocidadFade = 2f;
    [SerializeField] private TextMeshPro textMeshPro;

    private Coroutine coroutineActual;
    private bool jugadorAdentro = false;

    void Start()
    {
        if (textMeshPro == null) textMeshPro = GetComponentInChildren<TextMeshPro>();
        if (textMeshPro != null) DefinirAlpha(0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // CONDICIÓN DE BLOQUEO (OPCIÓN 2): Si el bosque ya se purificó, el cartel se calla para siempre
        if (GameController.Instance != null && GameController.Instance.BosquePurificado)
        {
            return;
        }

        KitsuneHealth jugador = other.GetComponentInParent<KitsuneHealth>();
        if (jugador == null || jugador.IsDead) return;

        if (!jugadorAdentro)
        {
            jugadorAdentro = true;

            if (GameController.Instance != null && GameController.Instance.InvisibilidadDesbloqueada)
            {
                textMeshPro.text = textoConInvisibilidad;
            }
            else
            {
                textMeshPro.text = textoSinInvisibilidad;
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
            coroutineActual = StartCoroutine(OcultarTextoRoutine(0f));
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