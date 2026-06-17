using System.Collections;
using UnityEngine;
using TMPro; // Requerido para la interfaz de usuario

public class AvisoCaidaEImposible : MonoBehaviour
{
    [Header("Configuración de Texto")]
    [TextArea(2, 5)]
    [SerializeField] private string textoMensaje = "¡Cuidado! Caí antes de tiempo... Mi pelaje se eriza en este fondo, presiento que perderse aquí abajo será peligroso.";
    [SerializeField] private float tiempoVisible = 4.0f;

    [Header("Referencia al HUD (Canvas)")]
    [SerializeField] private TextMeshProUGUI textMeshProReferencia;

    private bool yaMostrado = false;

    void Start()
    {
        // Forzar que el texto del HUD empiece invisible y vacío al arrancar el nivel
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
        if (yaMostrado) return;

        // Si el jugador ya tiene la llave, este trigger ambiental se apaga solo
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

        // Asegurar que quede completamente visible
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

        gameObject.SetActive(false);
    }
}