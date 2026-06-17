using UnityEngine;
using UnityEngine.UI;

public class KitsuneVidasUI : MonoBehaviour
{
    [Header("Fila de Corazones (Asignar de Izquierda a Derecha)")]
    [SerializeField] private Image[] iconosCorazones;

    [Header("Sprites de Estado")]
    [SerializeField] private Sprite corazonLleno;
    [SerializeField] private Sprite corazonVacio; // Opcional por si deseas un fondo de silueta, o déjalo en null para ocultarlo por completo

    void Update()
    {
        if (GameController.Instance == null || iconosCorazones == null || iconosCorazones.Length == 0) return;

        int vidasActuales = GameController.Instance.VidasActuales;

        for (int i = 0; i < iconosCorazones.Length; i++)
        {
            if (i < vidasActuales)
            {
                // El jugador aún posee esta vida
                iconosCorazones[i].sprite = corazonLleno;
                iconosCorazones[i].enabled = true;
            }
            else
            {
                // Esta vida fue consumida
                if (corazonVacio != null)
                {
                    iconosCorazones[i].sprite = corazonVacio;
                }
                else
                {
                    iconosCorazones[i].enabled = false; // Se apaga de la pantalla de derecha a izquierda
                }
            }
        }
    }
}