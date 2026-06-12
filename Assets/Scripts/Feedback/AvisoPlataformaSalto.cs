using System.Collections;
using UnityEngine;
using TMPro;

public class AvisoPlataformaSalto : MonoBehaviour
{
    [Header("Textos de las 3 Fases")]
    [TextArea(2, 5)]
    [SerializeField] private string textoFase1Caida = "El salto de regreso es imposible desde aquí... y esta extraña plataforma no parece reaccionar a mi presencia. Tendré que explorar el fondo de esta fosa para encontrar una salida.";
    [TextArea(2, 5)]
    [SerializeField] private string textoFase2Bucle = "La plataforma sigue apagada... Ese guardián corrupto de adelante debe ser la clave para reactivar la esencia de este lugar.";
    [TextArea(2, 5)]
    [SerializeField] private string textoFase3Activa = "Siento cómo el poder del bosque reactivó este mecanismo espiritual. Ahora que está en funcionamiento, podré volver a las plataformas superiores.";

    [Header("Configuración")]
    [SerializeField] private float tiempoVisible = 5.0f;
    [SerializeField] private float velocidadFade = 2f;
    [SerializeField] private TextMeshPro textMeshPro;

    private Coroutine coroutineActual;
    private bool jugadorAdentro = false;
    private bool fase1Mostrada = false;

    void Start()
    {
        if (textMeshPro == null) textMeshPro = GetComponentInChildren<TextMeshPro>();
        if (textMeshPro != null) DefinirAlpha(0f);
        fase1Mostrada = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        KitsuneHealth jugador = other.GetComponentInParent<KitsuneHealth>();
        if (jugador == null || jugador.IsDead) return;

        if (!jugadorAdentro)
        {
            jugadorAdentro = true;

            // EVALUACIÓN DE LAS 3 FASES EN ORDEN CRONOLÓGICO
            if (GameController.Instance != null && GameController.Instance.PlataformaSaltoActiva)
            {
                // FASE 3: El Tengu murió y la plataforma se encendió
                textMeshPro.text = textoFase3Activa;
            }
            else if (AvisoTenguCondicional.VistoAlTengu)
            {
                // FASE 2: La plataforma está apagada pero ya fuiste a mirar al jefe (Línea Amarilla)
                textMeshPro.text = textoFase2Bucle;
            }
            else
            {
                // FASE 1: Es la primera vez que caes y no has visto al jefe (Línea Roja)
                if (fase1Mostrada)
                {
                    jugadorAdentro = false; // Permitir que se libere si vuelve a pisarlo en Fase 1 sin avanzar
                    return;
                }

                textMeshPro.text = textoFase1Caida;
                fase1Mostrada = true; // Se marca para aparecer una sola vez en Fase 1
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

            // Si es la Fase 2 o 3, hacemos que se desvanezca al alejarse para que funcione siempre
            if (GameController.Instance != null && (GameController.Instance.PlataformaSaltoActiva || AvisoTenguCondicional.VistoAlTengu))
            {
                if (coroutineActual != null) StopCoroutine(coroutineActual);
                coroutineActual = StartCoroutine(OcultarTextoRoutine(0f)); // Fade out rápido al salir
            }
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