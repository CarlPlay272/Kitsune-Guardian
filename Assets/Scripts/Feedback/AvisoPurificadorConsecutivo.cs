using System.Collections;
using UnityEngine;
using TMPro;

public class AvisoPurificadorConsecutivo : MonoBehaviour
{
    [Header("Referencias UI Internas")]
    [SerializeField] private TextMeshPro textMeshPro;

    [Header("Textos Consecutivos (Solo Lore Nivel 1)")]
    [TextArea(2, 5)]
    [SerializeField]
    private string mensajeLore1 =
        "¡Increíble! He purificado la niebla corrupta de este sector sagrado... Siento cómo la arboleda vuelve a respirar.";

    [Header("Configuración de Tiempos")]
    [SerializeField] private float tiempoMensaje1 = 4.0f;
    [SerializeField] private float velocidadFade = 2f;

    private bool secuenciaDisparada = false;

    void Start()
    {
        if (textMeshPro == null)
            textMeshPro = GetComponentInChildren<TextMeshPro>();

        if (textMeshPro != null)
        {
            textMeshPro.text = "";
            SetTextoAlpha(0f);
        }
    }

    void Update()
    {
        if (secuenciaDisparada) return;

        if (GameController.Instance != null &&
            GameController.Instance.BosquePurificado)
        {
            secuenciaDisparada = true;
            StartCoroutine(SecuenciaMensajesRoutine());
        }
    }

    private IEnumerator SecuenciaMensajesRoutine()
    {
        if (textMeshPro == null)
            yield break;

        float alpha = 0f;

        // ----- TEXTO ÚNICO: PURIFICACIÓN -----
        textMeshPro.text = mensajeLore1;

        while (alpha < 1f)
        {
            alpha += Time.deltaTime * velocidadFade;
            SetTextoAlpha(alpha);
            yield return null;
        }

        SetTextoAlpha(1f);

        yield return new WaitForSeconds(tiempoMensaje1);

        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * velocidadFade;
            SetTextoAlpha(alpha);
            yield return null;
        }

        SetTextoAlpha(0f);

        gameObject.SetActive(false);
    }

    private void SetTextoAlpha(float a)
    {
        if (textMeshPro != null)
        {
            Color c = textMeshPro.color;
            c.a = Mathf.Clamp01(a);
            textMeshPro.color = c;
        }
    }
}