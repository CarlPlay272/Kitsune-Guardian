using System.Collections;
using UnityEngine;
using TMPro;

public class AvisoTenguCondicional : MonoBehaviour
{
    // Permite que otros scripts sepan si el jugador ya llegó hasta aquí
    public static bool VistoAlTengu { get; private set; } = false;

    [Header("Textos del Lore")]
    [TextArea(2, 5)]
    [SerializeField]
    private string textoConDisparo;

    [TextArea(2, 5)]
    [SerializeField]
    private string textoSinDisparo =
        "¡Un momento! Ese demonio es demasiado poderoso. Siento que necesito recuperar más de mi energía espiritual antes de intentarlo.";

    [Header("Configuración")]
    [SerializeField] private float tiempoVisible = 4.5f;
    [SerializeField] private float velocidadFade = 2f;
    [SerializeField] private TextMeshPro textMeshPro;

    private Coroutine coroutineActual;
    private bool jugadorAdentro = false;

    void Start()
    {
        // Al reiniciar el nivel de cero, reseteamos la variable global obligatoriamente
        VistoAlTengu = false;

        if (textMeshPro == null)
            textMeshPro = GetComponentInChildren<TextMeshPro>();

        if (textMeshPro != null)
            DefinirAlpha(0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameController.Instance != null &&
            GameController.Instance.BosquePurificado)
        {
            return;
        }

        KitsuneHealth jugador =
            other.GetComponentInParent<KitsuneHealth>();

        if (jugador == null || jugador.IsDead)
            return;

        if (!jugadorAdentro)
        {
            jugadorAdentro = true;

            // Registro: el jugador ya descubrió al Tengu
            VistoAlTengu = true;

            if (GameController.Instance != null &&
                GameController.Instance.DisparoDesbloqueado)
            {
                textMeshPro.text = textoConDisparo;
            }
            else
            {
                textMeshPro.text = textoSinDisparo;
            }

            if (coroutineActual != null)
                StopCoroutine(coroutineActual);

            coroutineActual =
                StartCoroutine(MostrarTextoRoutine());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        KitsuneHealth jugador =
            other.GetComponentInParent<KitsuneHealth>();

        if (jugador == null)
            return;

        if (jugadorAdentro)
        {
            jugadorAdentro = false;

            if (coroutineActual != null)
                StopCoroutine(coroutineActual);

            coroutineActual =
                StartCoroutine(OcultarTextoRoutine(0f));
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