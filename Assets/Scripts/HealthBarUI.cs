using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Image fillImage;
    public PlayerHealth playerHealth;

    void Update()
    {
        fillImage.fillAmount = playerHealth.currentHealth / playerHealth.maxHealth;
    }
}