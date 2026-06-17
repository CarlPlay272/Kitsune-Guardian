using System.Collections;
using UnityEngine;
using TMPro;

public class AvisoZonaMuerta : MonoBehaviour
{
    [Header("Configuración")]
    [TextArea(2, 5)]
    [SerializeField] private string textoMensaje = "Vaya... caí en un callejón sin salida y las paredes son demasiado altas para trepar. No tengo cómo salir de aquí a pie. (Presiona R para regresar al último checkpoint).";
    [SerializeField] private float tiempoVisible = 5.0f;
    [SerializeField] private float velocidadFade = 2f;
    [SerializeField] private TextMeshPro textMeshPro;

    private Coroutine coroutineActual;
    private bool jugadorAdentro = false;

    void Start()
    {
        if (textMeshPro == null)
            textMeshPro = GetComponentInChildren<TextMeshPro>();

        if (textMeshPro != null)
            DefinirAlpha(0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        KitsuneHealth jugador = other.GetComponentInParent<KitsuneHealth>();
        if (jugador == null || jugador.IsDead) return;

        if (!jugadorAdentro)
        {
            jugadorAdentro = true;
            textMeshPro.text = textoMensaje;

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