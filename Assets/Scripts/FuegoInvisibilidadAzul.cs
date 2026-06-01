using System.Collections;
using UnityEngine;

public class FuegoInvisibilidadAzul : MonoBehaviour
{
    [Header("Flotación")]
    [SerializeField] private float flotacionAmplitud = 0.25f;
    [SerializeField] private float flotacionVelocidad = 1.8f;

    [Header("Desaparición")]
    [SerializeField] private float tiempoDesaparicion = 0.6f;

    [Header("Audio")]
    [SerializeField] private AudioClip sonidoRecoleccion;
    private AudioSource audioSource;

    private bool recolectado = false;
    private Vector3 posicionInicial;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        posicionInicial = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!recolectado)
        {
            float nuevaY = posicionInicial.y + Mathf.Sin(Time.time * flotacionVelocidad) * flotacionAmplitud;
            transform.position = new Vector3(posicionInicial.x, nuevaY, posicionInicial.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (recolectado) return;

        KitsuneHealth kitsune = other.GetComponentInParent<KitsuneHealth>();
        if (kitsune == null) return;
        if (kitsune.IsDead) return;

        Recolectar();
    }

    private void Recolectar()
    {
        recolectado = true;

        if (GameController.Instance != null)
        {
            GameController.Instance.DesbloquearInvisibilidad();
        }

        if (sonidoRecoleccion != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoRecoleccion);
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        StartCoroutine(AnimacionDesaparicion());
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

        gameObject.SetActive(false);
    }
}