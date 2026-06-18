using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // OBLIGATORIO: Para poder cargar el Nivel_2 de forma real
using TMPro;

public class PortalMetaController : MonoBehaviour
{
    [Header("Bloqueo sin llave")]
    [SerializeField] private bool requiereLlave = true;
    [SerializeField] private float danioBloqueo = 10f;
    [SerializeField] private Vector2 empujeBloqueo = new Vector2(6f, 2f);
    [SerializeField] private float duracionKnockback = 0.25f;
    [SerializeField] private GameObject voidBloqueoVisual;
    [SerializeField] private float duracionVoidVisual = 0.6f;

    [Header("Referencias de Texto (Mundo)")]
    [SerializeField] private TextMeshPro textMeshProReferencia;
    [SerializeField] private float tiempoVisibleRechazo = 5.0f;

    [Header("Textos Oficiales Seleccionados")]
    [TextArea(2, 5)]
    [SerializeField] private string textoSinLlave = "¡No puede ser! El portal me rechaza... Está sellado por una fuerza oscura y no tengo la llave. ¡Quedé atrapado! (Presiona R para volver al último checkpoint).";
    [TextArea(2, 5)]
    [SerializeField] private string textoConLlave = "¡Aquí vamos!";

    [Header("Control Animación de Transición")]
    [SerializeField] private float cooldownPortalJugador = 0.8f;
    [SerializeField] private float retrasoCargaEscena = 1.2f;

    private bool mostrandoVoid = false;
    private bool procesandoTransicion = false;
    private Coroutine corrutinaTexto;

    void Start()
    {
        // Limpiar el componente de texto ambiental al arrancar el nivel
        if (textMeshProReferencia != null)
        {
            textMeshProReferencia.text = "";
            Color c = textMeshProReferencia.color;
            c.a = 0f;
            textMeshProReferencia.color = c;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (procesandoTransicion) return;

        KitsuneHealth kitsuneHealth = other.GetComponentInParent<KitsuneHealth>();
        if (kitsuneHealth == null || kitsuneHealth.IsDead) return;

        KitsuneController kitsuneController = kitsuneHealth.GetComponent<KitsuneController>();
        Rigidbody2D rb = kitsuneHealth.GetComponent<Rigidbody2D>();
        KitsunePortalState portalState = kitsuneHealth.GetComponent<KitsunePortalState>();

        if (rb == null || kitsuneController == null || portalState == null) return;
        if (!portalState.PuedeUsarPortal) return;

        // Evaluar inventario de forma global
        if (requiereLlave && (GameController.Instance == null || !GameController.Instance.TieneLlave))
        {
            RechazarJugador(kitsuneHealth, kitsuneController, portalState, kitsuneHealth.transform.position);
            return;
        }

        // Si tiene la llave, se gatilla la secuencia cinematográfica hacia la nueva escena
        StartCoroutine(SecuenciaCambioNivelRoutine(kitsuneHealth, rb, portalState));
    }

    // CORREGIDO: Añadido correctamente el tipo 'KitsunePortalState' y su identificador 'portalState'
    private void RechazarJugador(KitsuneHealth kitsuneHealth, KitsuneController kitsuneController, KitsunePortalState portalState, Vector3 posicionJugador)
    {
        kitsuneHealth.TakeDamage(danioBloqueo);

        float direccion = posicionJugador.x < transform.position.x ? -1f : 1f;
        Vector2 fuerzaFinal = new Vector2(direccion * empujeBloqueo.x, empujeBloqueo.y);

        kitsuneController.AplicarKnockback(fuerzaFinal, duracionKnockback);
        portalState.BloquearPortales(cooldownPortalJugador);

        if (voidBloqueoVisual != null && !mostrandoVoid)
        {
            StartCoroutine(MostrarVoidTemporal());
        }

        // Desplegar o reiniciar el cartel flotante de desesperación mística
        if (corrutinaTexto != null) StopCoroutine(corrutinaTexto);
        corrutinaTexto = StartCoroutine(MostrarTextoRechazoRoutine());
    }

    private IEnumerator MostrarTextoRechazoRoutine()
    {
        textMeshProReferencia.text = textoSinLlave;
        float alpha = 0f;

        // Fade In
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * 2f;
            Color c = textMeshProReferencia.color;
            c.a = alpha;
            textMeshProReferencia.color = c;
            yield return null;
        }

        yield return new WaitForSeconds(tiempoVisibleRechazo);

        // Fade Out
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * 2f;
            Color c = textMeshProReferencia.color;
            c.a = alpha;
            textMeshProReferencia.color = c;
            yield return null;
        }

        textMeshProReferencia.text = "";
    }

    private IEnumerator SecuenciaCambioNivelRoutine(KitsuneHealth kitsuneHealth, Rigidbody2D rb, KitsunePortalState portalState)
    {
        procesandoTransicion = true;
        portalState.BloquearPortales(retrasoCargaEscena + 1f);

        // 1. Congelar físicas por completo (Evita caídas y movimiento del jugador)
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 2. Volver invisible al zorro apagando su renderizador de sprites
        SpriteRenderer sr = kitsuneHealth.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        // 3. Lanzar instantáneamente el cartel cinemático
        if (corrutinaTexto != null) StopCoroutine(corrutinaTexto);
        textMeshProReferencia.text = textoConLlave;
        Color c = textMeshProReferencia.color;
        c.a = 1f;
        textMeshProReferencia.color = c;

        // 4. Cooldown de inmersión espiritual
        yield return new WaitForSeconds(retrasoCargaEscena);

        // 5. CAMBIO DE ESCENA REAL: Cargar el archivo separado de forma absoluta
        Debug.Log("Secuencia completada con éxito. Cargando de forma nativa: Nivel_2");
        SceneManager.LoadScene("Nivel_2");
    }

    private IEnumerator MostrarVoidTemporal()
    {
        mostrandoVoid = true;
        voidBloqueoVisual.SetActive(true);

        yield return new WaitForSeconds(duracionVoidVisual);

        if (voidBloqueoVisual != null)
            voidBloqueoVisual.SetActive(false);

        mostrandoVoid = false;
    }
}