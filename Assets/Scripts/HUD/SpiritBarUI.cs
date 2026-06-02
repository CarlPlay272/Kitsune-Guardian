using UnityEngine;
using UnityEngine.UI;

public class SpiritBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private KitsuneSpirit kitsuneSpirit;

    void Update()
    {
        if (fillImage == null || kitsuneSpirit == null)
            return;

        fillImage.fillAmount =
            kitsuneSpirit.CurrentSpirit /
            kitsuneSpirit.MaxSpirit;
    }
}