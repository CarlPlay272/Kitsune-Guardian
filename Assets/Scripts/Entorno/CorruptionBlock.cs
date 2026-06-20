using UnityEngine;

public class CorruptionBlock : MonoBehaviour
{
    [Header("Configuración de Daño")]
    [SerializeField] private float dañoPorSegundo = 10f;
    [SerializeField] private float intervaloDaño = 1.0f; // Daño pausado cada 1 segundo completo

    private float próximoTiempoDaño = 0f;

    public void DestruirBloqueo()
    {
        Debug.Log("🔮 [CORRUPCIÓN] ¡Bloqueo purificado por el fuego del Kitsune!");
        Destroy(gameObject);
    }

    // DETECCIÓN TRIGGER (Para la bola de fuego)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si el fueguito toca el trigger exterior ampliado, lo hace explotar de una
        if (other.GetComponent<Fireball>() != null)
        {
            DestruirBloqueo();
            Destroy(other.gameObject);
        }
    }

    // DETECCIÓN SÓLIDA (Para cuando el Kitsune camina o se para arriba)
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            KitsuneHealth health = collision.gameObject.GetComponent<KitsuneHealth>() ??
                                  collision.gameObject.GetComponentInChildren<KitsuneHealth>() ??
                                  collision.gameObject.GetComponentInParent<KitsuneHealth>();

            if (health != null && Time.time >= próximoTiempoDaño)
            {
                próximoTiempoDaño = Time.time + intervaloDaño;
                health.TakeDamage(dañoPorSegundo); // Quita vida lento y no por cada frame
                Debug.Log("⚠️ [CORRUPCIÓN] El Kitsune está pisando el bloqueo sólido. Perdiendo vida lentamente.");
            }
        }
    }
}