using System.Collections;
using UnityEngine;

public class FireSpiritPremio : MonoBehaviour
{
    [Header("Referencias")]
    public Animator animator;

    [Header("Premio")]
    [SerializeField] private int puntos = 1;

    [Header("Recolección")]
    [SerializeField] private float tiempoAntesDeDesaparecer = 0.8f;

    [Header("Flotación")]
    [SerializeField] private float flotacionAmplitud = 0.15f;
    [SerializeField] private float flotacionVelocidad = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip sonidoRecoleccion;
    private AudioSource audioSource;

    private bool recolectado = false;
    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;

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
            GameController.Instance.SumarPunto(puntos);
        }

        if (animator != null)
        {
            animator.SetTrigger("Collected");
        }

        if (sonidoRecoleccion != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoRecoleccion);
        }

        GetComponent<Collider2D>().enabled = false;

        StartCoroutine(DesaparecerDespues(tiempoAntesDeDesaparecer));
    }

    private IEnumerator DesaparecerDespues(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        gameObject.SetActive(false);
    }
}