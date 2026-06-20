using System.Collections;
using TMPro;
using UnityEngine;

public class IntroduccionDash : MonoBehaviour
{
    [Header("Textos")]
    [TextArea(2, 5)]
    [SerializeField] private string[] mensajes;

    [Header("Configuración")]
    [SerializeField] private float tiempoVisible = 4f;
    [SerializeField] private float velocidadFade = 2f;

    [Header("Referencias")]
    [SerializeField] private TextMeshPro textMeshPro;

    [Header("Bloqueo Temporal")]
    [SerializeField] private GameObject muroBloqueo;

    private bool activado = false;

    private void Start()
    {
        if (textMeshPro == null)
        {
            textMeshPro = GetComponentInChildren<TextMeshPro>();
        }

        if (textMeshPro != null)
        {
            Color c = textMeshPro.color;
            c.a = 0f;
            textMeshPro.color = c;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activado)
            return;

        KitsuneHealth kitsune =
            other.GetComponentInParent<KitsuneHealth>();

        if (kitsune == null)
            return;

        if (kitsune.IsDead)
            return;

        if (GameController.Instance == null)
            return;

        if (!GameController.Instance.DashDesbloqueado)
            return;

        activado = true;

        KitsuneController controller =
            kitsune.GetComponent<KitsuneController>();

        StartCoroutine(
            IntroRutina(controller)
        );
    }

    private IEnumerator IntroRutina(
        KitsuneController controller
    )
    {
        if (controller != null)
        {
            controller.BloquearControles();
        }

        foreach (string mensaje in mensajes)
        {
            textMeshPro.text = mensaje;

            yield return Fade(0f, 1f);

            yield return new WaitForSeconds(
                tiempoVisible
            );

            yield return Fade(1f, 0f);

            yield return new WaitForSeconds(
                0.5f
            );
        }

        if (controller != null)
        {
            controller.DesbloquearControles();
        }

        if (muroBloqueo != null)
        {
            Destroy(muroBloqueo);
        }

        Destroy(gameObject);
    }

    private IEnumerator Fade(
        float alphaInicial,
        float alphaFinal
    )
    {
        float tiempo = 0f;

        while (tiempo < 1f)
        {
            tiempo += Time.deltaTime *
                velocidadFade;

            Color c = textMeshPro.color;

            c.a = Mathf.Lerp(
                alphaInicial,
                alphaFinal,
                tiempo
            );

            textMeshPro.color = c;

            yield return null;
        }

        Color colorFinal = textMeshPro.color;
        colorFinal.a = alphaFinal;
        textMeshPro.color = colorFinal;
    }
}