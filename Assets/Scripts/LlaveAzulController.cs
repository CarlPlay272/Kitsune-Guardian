using System.Collections;
using UnityEngine;
using TMPro;

public class LlaveAzulController : MonoBehaviour
{
    [Header("Flotación")]
    [SerializeField] private float flotacionAmplitud = 0.25f;
    [SerializeField] private float flotacionVelocidad = 1.8f;

    [Header("Desaparición")]
    [SerializeField] private float tiempoDesaparicion = 0.6f;
    [SerializeField] private float alturaSubida = 0.4f;

    [Header("Audio")]
    [SerializeField] private AudioClip sonidoRecoleccion;
    private AudioSource audioSource;

    [Header("Lore de la Llave")]
    [SerializeField] private TextMeshPro textMeshProComponente;
    [TextArea(2, 5)]
    [SerializeField]
    private string textoLlave =
        "Vaya, una llave brillante. No sé qué puerta abra en este laberinto, pero mi instinto me dice que la guarde bien.";

    [SerializeField] private float tiempoMensaje = 4.0f;

    private bool recolectada = false;

    private Vector3 posicionInicial;
    private Vector3 posicionInicialGraphics;

    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    private Transform graphics;

    void Start()
    {
        posicionInicial = transform.position;

        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        graphics = transform;

        if (textMeshProComponente != null)
        {
            Color c = textMeshProComponente.color;
            c.a = 0f;
            textMeshProComponente.color = c;
        }
    }

    void Update()
    {
        if (recolectada) return;

        float nuevaY =
            posicionInicial.y +
            Mathf.Sin(Time.time * flotacionVelocidad) * flotacionAmplitud;

        transform.position = new Vector3(
            posicionInicial.x,
            nuevaY,
            posicionInicial.z
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (recolectada) return;

        KitsuneHealth kitsune =
            other.GetComponentInParent<KitsuneHealth>();

        if (kitsune == null || kitsune.IsDead)
            return;

        RecogerLlave();
    }

    private void RecogerLlave()
    {
        recolectada = true;

        if (GameController.Instance != null)
        {
            GameController.Instance.ObtenerLlaveAzul();
        }

        if (sonidoRecoleccion != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoRecoleccion);
        }

        if (col != null)
            col.enabled = false;

        StartCoroutine(AnimacionDesaparicion());

        if (textMeshProComponente != null)
        {
            StartCoroutine(MostrarTextoRoutine());
        }
    }

    private IEnumerator AnimacionDesaparicion()
    {
        float tiempo = 0f;

        Vector3 inicio = transform.position;
        Vector3 fin = inicio + Vector3.up * alturaSubida;

        Color colorInicial =
            spriteRenderer != null ? spriteRenderer.color : Color.white;

        while (tiempo < tiempoDesaparicion)
        {
            tiempo += Time.deltaTime;

            float t = tiempo / tiempoDesaparicion;

            // movimiento hacia arriba
            transform.position =
                Vector3.Lerp(inicio, fin, t);

            // fade
            if (spriteRenderer != null)
            {
                Color c = colorInicial;
                c.a = Mathf.Lerp(1f, 0f, t);
                spriteRenderer.color = c;
            }

            yield return null;
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
    }

    private IEnumerator MostrarTextoRoutine()
    {
        textMeshProComponente.text = textoLlave;

        float alpha = 0f;

        while (alpha < 1f)
        {
            alpha += Time.deltaTime * 2f;

            Color c = textMeshProComponente.color;
            c.a = alpha;
            textMeshProComponente.color = c;

            yield return null;
        }

        yield return new WaitForSeconds(tiempoMensaje);

        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * 2f;

            Color c = textMeshProComponente.color;
            c.a = alpha;
            textMeshProComponente.color = c;

            yield return null;
        }
    }
}