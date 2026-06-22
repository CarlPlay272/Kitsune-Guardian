using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [SerializeField] private OniBoss oniBoss;

    private bool activated;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("TOCÓ: " + other.name);

        if (activated) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("PLAYER DETECTADO → START BOSS");
            activated = true;

            if (oniBoss == null)
            {
                Debug.LogError("ONI NO ASIGNADO EN INSPECTOR");
                return;
            }

            oniBoss.StartCombat();
        }
    }
}