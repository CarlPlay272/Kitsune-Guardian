using UnityEngine;

public class BossOniTrigger : MonoBehaviour
{
    [SerializeField] private OniBoss oniBoss;

    private bool activated;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Encendemos la presencia del jugador en el HUD maestro de forma fija
            if (BossOniHUDController.Instance != null)
            {
                BossOniHUDController.Instance.CambiarEstadoPresenciaJugador(true);
            }

            Debug.Log("TOCÓ: " + other.name);

            if (activated) return;

            Debug.Log("PLAYER DETECTADO → START BOSS");
            activated = true;

            if (oniBoss == null)
            {
                Debug.LogError("ONI NO ASIGNADO EN INSPECTOR");
                return;
            }

            oniBoss.StartCombat(); // Arranca el combate místico
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 🔥 PARCHE DE SEGURIDAD: Comentamos o eliminamos el apagado automático por salida.
        // Evita que si el jugador retrocede un poco en el cerro se congele la invocación de los Tengus.
        /*
        if (other.CompareTag("Player"))
        {
            if (BossOniHUDController.Instance != null)
            {
                BossOniHUDController.Instance.CambiarEstadoPresenciaJugador(false);
            }
        }
        */
    }
}