using System.Collections;
using UnityEngine;
using TMPro; // Requerido para renderizar el texto flotante en la escena

public class LlaveAzulController : MonoBehaviour
{
    [Header("Flotación")]
    [SerializeField] private float flotacionAmplitud = 0.25f;
    [SerializeField] private float flotacionVelocidad = 1.8f;

    [Header("Desaparición")]
    [SerializeField] private float tiempoDesaparicion = 0.6f;

    [Header("Audio")]
    [SerializeField] private AudioClip sonidoRecoleccion;
    private AudioSource audioSource;

    [Header("Lore de la Llave")]
    [SerializeField] private TextMeshPro textMeshProComponente;
    [TextArea(2, 5)][SerializeField] private string textoLlave = "Vaya, una llave brillante. No sé qué puerta abra en este laberinto, pero mi instinto me dice que la guarde bien.";
    [SerializeField] private float tiempoMensaje = 4.0f;

    private bool recolectada = false;
    private Vector3 posicionInicial;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        posicionInicial = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        if (textMeshProComponente != null)
        {
            Color c = textMeshProComponente.color;
            c.a = 0f;
            textMeshProComponente.color = c;
        }
    }

    void Update()
    {
        if (!recolectada)
        {
            float nuevaY = posicionInicial.y + Mathf.Sin(Time.time * flotacionVelocidad) * flotacionAmplitud;
            transform.position = new Vector3(posicionInicial.x, nuevaY, posicionInicial.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (recolectada) return;

        KitsuneHealth kitsune = other.GetComponentInParent<KitsuneHealth>();
        if (kitsune == null) return;
        if (kitsune.IsDead) return;

        RecogerLlave();
    }

    private void RecogerLlave()
    {
        recolectada = true;

        if (GameController.Instance != null)
        {
            GameController.Instance.ObtenerLlaveAzul(); // Registra que el jugador tiene la llave
        }

        if (sonidoRecoleccion != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoRecoleccion);
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        StartCoroutine(AnimacionDesaparicion());

        if (textMeshProComponente != null)
        {
            StartCoroutine(MostrarTextoRoutine());
        }
    }

    private IEnumerator MostrarTextoRoutine()
    {
        textMeshProComponente.text = textoLlave;
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * 2f;
            if (textMeshProComponente != null)
            {
                Color c = textMeshProComponente.color;
                c.a = alpha;
                textMeshProComponente.color = c;
            }
            yield return null;
        }

        yield return new WaitForSeconds(tiempoMensaje);

        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * 2f;
            if (textMeshProComponente != null)
            {
                Color c = textMeshProComponente.color;
                c.a = alpha;
                textMeshProComponente.color = c;
            }
            yield return null;
        }
    }

    private IEnumerator AnimacionDesaparicion()
    {
        float tiempoTranscurrido = 0f;
        Color colorInicial = spriteRenderer != null ? spriteRenderer.color : Color.white;

        while (tiempoTranscurrido < tiempoDesaparicion)
        {
            tiempoTranscurrido += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, tiempoTranscurrido / tiempoDesaparicion);

            if (spriteRenderer != null)
            {
                Color nuevoColor = colorInicial;
                nuevoColor.a = alpha;
                spriteRenderer.color = nuevoColor;
            }

            yield return null;
        }

        if (spriteRenderer != null) spriteRenderer.enabled = false;
    }
}