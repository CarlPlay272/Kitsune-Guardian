using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject container;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text bossNameText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Configuración")]
    [SerializeField] private string bossName = "Oni";
    [SerializeField] private float fillSpeed = 5f;
    [SerializeField] private float fadeDuration = 0.4f;

    private float targetFillAmount = 1f;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (container != null)
            container.SetActive(false);

        if (bossNameText != null)
            bossNameText.text = bossName;

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    private void Update()
    {
        if (fillImage == null) return;

        fillImage.fillAmount = Mathf.MoveTowards(
            fillImage.fillAmount,
            targetFillAmount,
            fillSpeed * Time.deltaTime
        );
    }

    public void Show()
    {
        if (container == null) return;

        container.SetActive(true);

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeIn());

        if (fillImage != null)
            fillImage.fillAmount = 1f;

        targetFillAmount = 1f;
    }

    public void Hide()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeOut());
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        targetFillAmount = maxHealth > 0
            ? (float)currentHealth / maxHealth
            : 0f;
    }

    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;

        float t = 0f;
        canvasGroup.alpha = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;

        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        container.SetActive(false);
    }
}