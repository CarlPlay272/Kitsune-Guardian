using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [Header("Escenas")]
    [SerializeField] private string escenaJugar = "SampleScene";

    public void Jugar()
    {
        if (!string.IsNullOrEmpty(escenaJugar))
        {
            Debug.Log("Botón Jugar");
            SceneManager.LoadScene(escenaJugar);
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