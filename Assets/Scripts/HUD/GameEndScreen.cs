using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameEndScreen : MonoBehaviour
{
    [Header("UI - Arrastrá estos desde el Inspector")]
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TextMeshProUGUI endText;
    [SerializeField] private Button mainMenuButton;

    [Header("Configuración")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private float textAppearDelay = 1f;

    [TextArea(3, 8)]
    [SerializeField]
    private string endMessage =
        "La oscuridad ha sido derrotada.\n\n" +
        "Kitsune purificó la corrupción del bosque sagrado.\n\n" +
        "La luz vuelve a brillar sobre las tierras antiguas.";

    void Start()
    {
        // Asegurarse de que el panel esté oculto al inicio
        if (endPanel != null)
            endPanel.SetActive(false);

        // Conectar el botón
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    // Llamá este método desde OniBoss cuando muere
    public void ShowEndScreen()
    {
        Debug.Log("✅ ShowEndScreen llamado");
        StartCoroutine(EndSequence());
    }

    private IEnumerator EndSequence()
    {
        // Congelar el juego
        Time.timeScale = 0f;

        // Esperar un momento antes de mostrar el panel
        // (usamos WaitForSecondsRealtime porque timeScale = 0)
        yield return new WaitForSecondsRealtime(textAppearDelay);

        // Mostrar panel
        if (endPanel != null)
            endPanel.SetActive(true);

        // Escribir el texto
        if (endText != null)
            endText.text = endMessage;

        // Mostrar el botón
        if (mainMenuButton != null)
            mainMenuButton.gameObject.SetActive(true);

        Debug.Log("✅ EndSequence iniciado");
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(textAppearDelay);
        Debug.Log("✅ Activando panel");

        if (endPanel != null)
            endPanel.SetActive(true);
        else
            Debug.LogError("❌ endPanel es null");

        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(textAppearDelay);

        if (endPanel != null)
        {
            endPanel.SetActive(true);
            Debug.Log($"✅ Panel activo: {endPanel.activeSelf} | Panel en jerarquía activo: {endPanel.activeInHierarchy}");
        }
        else
            Debug.LogError("❌ endPanel es null");
    }

    private void GoToMainMenu()
    {
        // Restaurar el tiempo antes de cambiar de escena
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnDestroy()
    {
        // Por seguridad: si se destruye el objeto, restaurar timeScale
        Time.timeScale = 1f;
    }
}