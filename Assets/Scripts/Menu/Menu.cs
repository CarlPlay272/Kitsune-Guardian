using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [Header("Escenas")]
    [SerializeField] private string escenaJugar = "Nivel_1"; // CORREGIDO: Apunta por defecto a tu primer nivel real

    public void Jugar()
    {
        if (!string.IsNullOrEmpty(escenaJugar))
        {
            Debug.Log("Botón Jugar");
            SceneManager.LoadScene(escenaJugar); // Carga de forma limpia el Nivel_1
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