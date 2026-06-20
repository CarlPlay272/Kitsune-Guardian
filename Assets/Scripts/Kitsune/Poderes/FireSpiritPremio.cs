using System.Collections;
using UnityEngine;

public class FireSpiritPremio : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform graphics;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Premio")]
    [SerializeField] private int puntos = 1;
    [SerializeField] private float curacionAlRecolectar = 25f;
    [SerializeField] private float energiaOtorgada = 3f;
    [Header("Efecto de recolección")]
    [SerializeField] private float duracionEfecto = 0.5f;
    [SerializeField] private float alturaSubida = 0.5f;

    [Header("Flotación")]
    [SerializeField] private float flotacionAmplitud = 0.15f;
    [SerializeField] private float flotacionVelocidad = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip sonidoRecoleccion;

    private AudioSource audioSource;
    private Collider2D colliderPremio;

    private bool recolectado = false;

    private Vector3 posicionInicial;
    private Vector3 posicionInicialGraphics;

    private void Start()
    {
        posicionInicial = transform.position;

        audioSource = GetComponent<AudioSource>();
        colliderPremio = GetComponent<Collider2D>();

        if (graphics == null)
        {
            graphics = transform.Find("Graphics");
        }

        if (spriteRenderer == null && graphics != null)
        {
            spriteRenderer = graphics.GetComponent<SpriteRenderer>();
        }

        if (graphics != null)
        {
            posicionInicialGraphics = graphics.localPosition;
        }

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
    }

    private void Update()
    {
        if (recolectado)
            return;

        float nuevaY = posicionInicial.y +
            Mathf.Sin(Time.time * flotacionVelocidad) * flotacionAmplitud;

        transform.position = new Vector3(
            posicionInicial.x,
            nuevaY,
            posicionInicial.z
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (recolectado)
            return;

        KitsuneHealth kitsune =
            other.GetComponentInParent<KitsuneHealth>();

        if (kitsune == null)
            return;

        if (kitsune.IsDead)
            return;

        Recolectar(kitsune);
    }

    private void Recolectar(KitsuneHealth kitsune)
    {
        recolectado = true;

        Debug.Log("Espíritu de fuego recolectado.");

        // Puntos
        if (GameController.Instance != null)
        {
            GameController.Instance.SumarPunto(puntos);
        }

        // Curación
        if (curacionAlRecolectar > 0f)
        {
            kitsune.Heal(curacionAlRecolectar);
        }

        // Energía espiritual
        KitsuneSpirit spirit =
            kitsune.GetComponent<KitsuneSpirit>();

        if (spirit != null)
        {
            spirit.AddSpirit(energiaOtorgada);

            Debug.Log(
                "Espíritu actual: " +
                spirit.CurrentSpirit +
                "/" +
                spirit.MaxSpirit
            );
        }

        // Sonido
        if (sonidoRecoleccion != null &&
            audioSource != null)
        {
            audioSource.PlayOneShot(
                sonidoRecoleccion
            );
        }

        // Desactivar colisión
        if (colliderPremio != null)
        {
            colliderPremio.enabled = false;
        }

        StartCoroutine(EfectoRecoleccion());
    }

    private IEnumerator EfectoRecoleccion()
    {
        if (graphics == null || spriteRenderer == null)
        {
            gameObject.SetActive(false);
            yield break;
        }

        float tiempo = 0f;

        Vector3 inicio = graphics.localPosition;
        Vector3 fin = inicio + Vector3.up * alturaSubida;

        Color colorInicial = spriteRenderer.color;

        while (tiempo < duracionEfecto)
        {
            tiempo += Time.deltaTime;

            float t = tiempo / duracionEfecto;

            graphics.localPosition =
                Vector3.Lerp(inicio, fin, t);

            Color color = colorInicial;
            color.a = Mathf.Lerp(1f, 0f, t);

            spriteRenderer.color = color;

            yield return null;
        }

        gameObject.SetActive(false);
    }
}