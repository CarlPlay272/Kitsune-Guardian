using System.Collections;
using UnityEngine;

public class DashPowerUnlock : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip sonidoDesbloqueo;

    [Header("Efecto de recolección")]
    [SerializeField] private float duracionEfecto = 0.5f;
    [SerializeField] private float alturaSubida = 0.5f;

    [Header("Flotación")]
    [SerializeField] private float flotacionAmplitud = 0.15f;
    [SerializeField] private float flotacionVelocidad = 2f;

    private AudioSource audioSource;
    private Collider2D colliderPremio;
    private SpriteRenderer spriteRenderer;

    private bool recolectado = false;
    private Vector3 posicionInicial;

    private void Start()
    {
        posicionInicial = transform.position;

        audioSource = GetComponent<AudioSource>();
        colliderPremio = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (recolectado)
            return;

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
        if (recolectado)
            return;

        KitsuneHealth kitsune =
            other.GetComponentInParent<KitsuneHealth>();

        if (kitsune == null || kitsune.IsDead)
            return;

        recolectado = true;

        if (GameController.Instance != null)
        {
            GameController.Instance.DesbloquearDash();
        }

        Debug.Log("Dash desbloqueado");

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
        if (spriteRenderer == null)
        {
            Destroy(gameObject);
            yield break;
        }

        float tiempo = 0f;

        Vector3 inicio = transform.position;
        Vector3 fin = inicio + Vector3.up * alturaSubida;

        Color colorInicial = spriteRenderer.color;

        while (tiempo < duracionEfecto)
        {
            tiempo += Time.deltaTime;

            float t = tiempo / duracionEfecto;

            transform.position =
                Vector3.Lerp(inicio, fin, t);

            Color color = colorInicial;
            color.a = Mathf.Lerp(1f, 0f, t);

            spriteRenderer.color = color;

            yield return null;
        }

        Destroy(gameObject);
    }
}