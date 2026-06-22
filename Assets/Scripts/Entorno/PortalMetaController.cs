using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        if (textMeshProReferencia != null)
        {
            textMeshProReferencia.text = "";
            Color c = textMeshProReferencia.color;
            c.a = 0f;
            textMeshProReferencia.color = c;
        }

        // CONTROL DE SEGURIDAD INTER-NIVEL: Liberar controles físicos del zorro al despertar en el mapa nuevo
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            KitsuneController kc = playerObj.GetComponent<KitsuneController>();
            if (kc != null) kc.DesbloquearControles();
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

        string escenaActual = SceneManager.GetActiveScene().name;

        if (escenaActual == "Nivel_1" && requiereLlave && (GameController.Instance == null || !GameController.Instance.TieneLlave))
        {
            RechazarJugador(kitsuneHealth, kitsuneController, portalState, kitsuneHealth.transform.position);
            return;
        }

        StartCoroutine(SecuenciaCambioNivelRoutine(kitsuneHealth, rb, portalState, escenaActual));
    }

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

        if (textMeshProReferencia != null)
        {
            if (corrutinaTexto != null) StopCoroutine(corrutinaTexto);
            corrutinaTexto = StartCoroutine(MostrarTextoRechazoRoutine());
        }
    }

    private IEnumerator MostrarTextoRechazoRoutine()
    {
        textMeshProReferencia.text = textoSinLlave;
        float alpha = 0f;

        while (alpha < 1f)
        {
            alpha += Time.deltaTime * 2f;
            Color c = textMeshProReferencia.color;
            c.a = alpha;
            textMeshProReferencia.color = c;
            yield return null;
        }

        yield return new WaitForSeconds(tiempoVisibleRechazo);

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

    private IEnumerator SecuenciaCambioNivelRoutine(KitsuneHealth kitsuneHealth, Rigidbody2D rb, KitsunePortalState portalState, string escenaOrigen)
    {
        procesandoTransicion = true;
        portalState.BloquearPortales(retrasoCargaEscena + 1f);

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        SpriteRenderer sr = kitsuneHealth.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        if (textMeshProReferencia != null)
        {
            if (corrutinaTexto != null) StopCoroutine(corrutinaTexto);
            textMeshProReferencia.text = textoConLlave;

            Color c = textMeshProReferencia.color;
            c.a = 1f;
            textMeshProReferencia.color = c;
        }

        yield return new WaitForSeconds(retrasoCargaEscena);

        string escenaDestino = escenaOrigen switch
        {
            "Nivel_1" => "Nivel_2",
            "Nivel_2" => "Nivel_3",
            "Nivel_3" => "Nivel_1", // cambiar si deseas otro destino final
            _ => "Nivel_1"
        };

        if (GameController.Instance != null)
        {
            GameController.Instance.GuardarDatosParaSiguienteNivel();
        }

        Debug.Log("Lanzando pantalla de carga asíncrona hacia: " + escenaDestino);

        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.CambiarEscenaMistica(escenaDestino);
        }
        else
        {
            SceneManager.LoadScene(escenaDestino);
        }
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