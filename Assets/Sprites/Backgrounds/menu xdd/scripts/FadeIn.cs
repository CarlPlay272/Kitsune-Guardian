using UnityEngine;
using UnityEngine.UI;

public class FadeIn : MonoBehaviour
{
    public Image fadeImage;
    public float fadeSpeed = 1f;

    void Update()
    {
        Color color = fadeImage.color;

        if (color.a > 0)
        {
            color.a -= fadeSpeed * Time.deltaTime;
            fadeImage.color = color;
        }
    }
}