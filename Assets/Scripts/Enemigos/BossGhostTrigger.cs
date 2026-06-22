using UnityEngine;

public class BossGhostTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // FILTRO ABSOLUTO: Solo el Kitsune despierta la arena
        if (other.CompareTag("Player"))
        {
            ForzarAlertaFantasmas(true);
            Debug.Log("⚔️ [ZONA DE JEFE] ¡Kitsune ha entrado! Fantasmas alertadas.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // FILTRO ABSOLUTO: Solo si el jugador sale físicamente, se calman
        if (other.CompareTag("Player"))
        {
            ForzarAlertaFantasmas(false);
            Debug.Log("🏳️ [ZONA DE JEFE] Kitsune ha salido. Fantasmas regresando a sus puestos.");
        }
    }

    private void ForzarAlertaFantasmas(bool activar)
    {
        ZoneBossGhostAI[] fantasmas = Object.FindObjectsByType<ZoneBossGhostAI>(FindObjectsSortMode.None);
        foreach (ZoneBossGhostAI ghost in fantasmas)
        {
            if (ghost != null)
            {
                if (activar) ghost.KitsuneEntroALaZona();
                else ghost.KitsuneSalioDeLaZona();
            }
        }
    }
}