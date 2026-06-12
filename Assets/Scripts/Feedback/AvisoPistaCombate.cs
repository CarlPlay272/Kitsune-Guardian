using System.Collections;
using UnityEngine;
using TMPro;

public class AvisoPistaCombate : MonoBehaviour
{
    [Header("Configuración")]
    [TextArea(2, 5)]
    [SerializeField] private string textoPista = "El Tengu es sumamente poderoso y resistirá mis golpes frontales... Mi instinto me dice que la única forma de dañarlo es atacándolo por la espalda con mi dash.";
    [SerializeField] private float tiempoVisible = 6.0f;
    [SerializeField] private float velocidadFade = 2f;
    [SerializeField] private TextMeshPro textMeshPro;

    private bool yaMostrado = false;

    void Start()
    {
        if (textMeshPro == null) textMeshPro = GetComponentInChildren<TextMeshPro>();
        if (textMeshPro != null) DefinirAlpha(0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (yaMostrado) return;

        // REGLA DE ORO: Solo aparece si YA purificaste la niebla (tienes el Dash) Y si el Tengu sigue vivo
        if (GameController.Instance == null || !GameController.Instance.BosquePurificado) return;

        // Comprobación de seguridad para no mostrarlo si el Tengu ya murió en esta partida
        TenguState tengu = FindFirstObjectByType<TenguState>();
        if (tengu != null && tengu.IsDead) return;

        KitsuneHealth jugador = other.GetComponentInParent<KitsuneHealth>();
        if (jugador == null || jugador.IsDead) return;

        yaMostrado = true;
        StartCoroutine(SecuenciaUnicaRoutine());
    }

    private IEnumerator SecuenciaUnicaRoutine()
    {
        textMeshPro.text = textoPista;
        float alpha = 0f;

        // Fade In
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * velocidadFade;
            DefinirAlpha(alpha);
            yield return null;
        }
        DefinirAlpha(1f);

        yield return new WaitForSeconds(tiempoVisible);

        // Fade Out
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * velocidadFade;
            DefinirAlpha(alpha);
            yield return null;
        }
        DefinirAlpha(0f);

        gameObject.SetActive(false);
    }

    private void DefinirAlpha(float valorAlpha)
    {
        if (textMeshPro != null)
        {
            Color c = textMeshPro.color;
            c.a = Mathf.Clamp01(valorAlpha);
            textMeshPro.color = c;
        }
    }
}