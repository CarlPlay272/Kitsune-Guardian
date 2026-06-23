using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GhostBullet : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float speed = 4f;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private float damage = 10f;

    private Vector2 direccionMover;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void InicializarDireccion(Vector3 targetPos)
    {
        direccionMover = (targetPos - transform.position).normalized;
        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + direccionMover * speed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Verificar si el objeto con el que chocamos es el jugador
        KitsuneHealth playerHealth = other.GetComponent<KitsuneHealth>() ?? other.GetComponentInParent<KitsuneHealth>();

        if (playerHealth != null && !playerHealth.IsDead)
        {
            // 🔥 PARCHE MÍSTICO: Buscamos el controlador para verificar si Carlos activó la invisibilidad
            KitsuneController playerController = other.GetComponent<KitsuneController>() ?? other.GetComponentInParent<KitsuneController>();

            if (playerController != null && playerController.IsInvisible)
            {
                Debug.Log("🔮 [INVISIBILIDAD] La bala fantasma pasó de largo a través del Kitsune.");
                return; // Corta la ejecución: la bala no hace daño y sigue su camino de largo
            }

            // Si no está invisible, se aplica el comportamiento normal de daño
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}