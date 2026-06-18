using System.Collections;
using UnityEngine;
using TMPro;

public class AvisoPuntoNoRetorno : MonoBehaviour
{
    [Header("Referencias de Texto (Mundo)")]
    [SerializeField] private TextMeshPro textMeshProReferencia;
    [SerializeField] private float tiempoVisible = 5.0f;

    [Header("Diálogo SIN Llave (Texto Antiguo Elegido)")]
    [TextArea(2, 5)]
    [SerializeField] private string textoSinLlave = "Mala espina... El camino de regreso quedó sellado y este aire pesado me da muy malas vibras. Algo no anda bien adelante.";

    [Header("Diálogo CON Llave (Texto Antiguo Elegido)")]
    [TextArea(2, 5)]
    [SerializeField] private string textoConLlave = "Bueno... ya no hay forma de volver atrás. Siento que el final de este viaje espiritual está cruzando ese pasillo.";

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
        if (yaMostrado) return;

        KitsuneHealth jugador = other.GetComponentInParent<KitsuneHealth>();
        if (jugador == null || jugador.IsDead) return;

        yaMostrado = true;
        StartCoroutine(MostrarTextoRoutine());
    }

    private IEnumerator MostrarTextoRoutine()
    {
        // Evaluación del inventario a través del GameController global
        if (GameController.Instance != null && GameController.Instance.TieneLlave)
        {
            textMeshProReferencia.text = textoConLlave;
        }
        else
        {
            textMeshProReferencia.text = textoSinLlave;
        }

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

        yaMostrado = false;
    }
}