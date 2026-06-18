using System.Collections;
using UnityEngine;
using TMPro;

public class AvisoFlotanteAmbiental : MonoBehaviour
{
    [Header("Configuración de Texto")]
    [TextArea(2, 5)]
    [SerializeField] private string textoMensaje = "Es inútil intentar trepar por aquí, estas paredes están lisas. No me queda otra opción que seguir el sendero inferior.";
    [SerializeField] private float tiempoVisible = 4.0f;

    [Header("Referencia al Texto en Escena (Mundo)")]
    [SerializeField] private TextMeshPro textMeshProReferencia;

    private bool yaMostrado = false;

    void Start()
    {
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
        // Si ya se está mostrando el texto actualmente, ignoramos la nueva colisión
        if (yaMostrado) return;

        // Desactivar si el jugador ya tiene la llave (backtracking)
        if (GameController.Instance != null && GameController.Instance.TieneLlave)
        {
            gameObject.SetActive(false);
            return;
        }

        KitsuneHealth jugador = other.GetComponentInParent<KitsuneHealth>();
        if (jugador == null || jugador.IsDead) return;

        yaMostrado = true;
        if (textMeshProReferencia != null)
        {
            StartCoroutine(MostrarTextoRoutine());
        }
    }

    private IEnumerator MostrarTextoRoutine()
    {
        textMeshProReferencia.text = textoMensaje;
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

        if (textMeshProReferencia != null)
        {
            Color c = textMeshProReferencia.color;
            c.a = 1f;
            textMeshProReferencia.color = c;
        }

        yield return new WaitForSeconds(tiempoVisible);

        // Fade Out
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * 2f;
            Color c = textMeshProReferencia.color;
            c.a = alpha;
            textMeshProReferencia.color = c;
            yield return null;
        }

        if (textMeshProReferencia != null)
        {
            textMeshProReferencia.text = "";
        }

        // CORRECCIÓN: Volvemos a dejar la variable en false para que el trigger se pueda volver a usar
        yaMostrado = false;
    }
}