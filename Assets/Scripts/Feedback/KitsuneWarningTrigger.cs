using System.Collections;
using TMPro;
using UnityEngine;

public class KitsuneTenguWarning : MonoBehaviour
{
    [Header("Referencia al Oni")]
    [SerializeField] private OniBoss oniBoss;

    [Header("Texto")]
    [TextArea(2, 4)]
    [SerializeField]
    private string mensaje =
        "No puedo atacar al Oni mientras tenga a sus Tengus protegiéndolo...";

    [Header("UI")]
    [SerializeField] private TextMeshPro textMeshPro;

    [Header("Timing")]
    [SerializeField] private float tiempoVisible = 3f;
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private float cooldown = 6f;

    private bool activado = false;
    private bool enCooldown = false;

    void Start()
    {
        if (textMeshPro == null)
            textMeshPro = GetComponentInChildren<TextMeshPro>();

        SetAlpha(0f);
    }

    void Update()
    {
        if (activado || enCooldown) return;

        if (oniBoss == null) return;

        // 🔥 SOLO SE ACTIVA SI HAY TENGUS
        if (!oniBoss.TieneTengusActivos())
            return;

        activado = true;
        StartCoroutine(WarningRoutine());
    }

    private IEnumerator WarningRoutine()
    {
        textMeshPro.text = mensaje;

        yield return Fade(0f, 1f);

        yield return new WaitForSeconds(tiempoVisible);

        yield return Fade(1f, 0f);

        enCooldown = true;

        yield return new WaitForSeconds(cooldown);

        Destroy(gameObject);
    }

    private IEnumerator Fade(float a, float b)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;

            SetAlpha(Mathf.Lerp(a, b, t));

            yield return null;
        }

        SetAlpha(b);
    }

    private void SetAlpha(float a)
    {
        if (textMeshPro == null) return;

        Color c = textMeshPro.color;
        c.a = a;
        textMeshPro.color = c;
    }
}