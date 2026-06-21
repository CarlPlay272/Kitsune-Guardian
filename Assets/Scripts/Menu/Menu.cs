using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [Header("Escenas")]
    [SerializeField] private string escenaJugar = "Nivel_1"; // Apunta por defecto a tu primer nivel real

    public void Jugar()
    {
        if (!string.IsNullOrEmpty(escenaJugar))
        {
            Debug.Log("Botón Jugar - Iniciando transición asíncrona mística.");
            
            // 🔥 CAMBIO COMPLETO: Si el LoadingManager está vivo en la escena, inicia la carga asíncrona con el fondo místico
            if (LoadingManager.Instance != null)
            {
                LoadingManager.Instance.CambiarEscenaMistica(escenaJugar);
            }
            else
            {
                // Caída de seguridad por si pruebas el menú solo en el editor sin el objeto managers
                SceneManager.LoadScene(escenaJugar);
            }
        }
        else
        {
            Debug.LogWarning("No se asignó la escena de juego.");
        }
    }

    public void Salir()
    {
        Debug.Log("Botón Salir");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}