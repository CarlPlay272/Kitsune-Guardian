using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private KitsuneHealth kitsuneHealth;

    void Update()
    {
        if (fillImage == null || kitsuneHealth == null) return;

        fillImage.fillAmount = kitsuneHealth.CurrentHealth / kitsuneHealth.MaxHealth;
    }
}