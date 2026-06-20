using System.Collections;
using UnityEngine;

public class FirePowerUnlock : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip sonidoDesbloqueo;

    [Header("Referencias")]
    [SerializeField] private Transform graphics;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Efecto de recolección")]
    [SerializeField] private float duracionEfecto = 0.5f;
    [SerializeField] private float alturaSubida = 0.5f;

    private AudioSource audioSource;
    private Collider2D colliderPremio;

    private bool recolectado = false;

    private void Start()
    {
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
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (recolectado)
            return;

        KitsuneHealth kitsune =
            other.GetComponentInParent<KitsuneHealth>();

        if (kitsune == null)
            return;

        recolectado = true;

        if (GameController.Instance != null)
        {
            GameController.Instance.DesbloquearDisparo();
        }

        Debug.Log("Poder de fuego desbloqueado");

        if (sonidoDesbloqueo != null &&
            audioSource != null)
        {
            audioSource.PlayOneShot(sonidoDesbloqueo);
        }

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
            Destroy(gameObject);
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

        Destroy(gameObject);
    }
}