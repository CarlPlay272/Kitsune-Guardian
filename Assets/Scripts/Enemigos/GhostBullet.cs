using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GhostBullet : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float speed = 4f; // Velocidad lenta como pediste
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
        // Calcula la dirección matemática pura hacia el punto del Kitsune
        direccionMover = (targetPos - transform.position).normalized;

        // Destrucción automática si no golpea nada
        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        // Desplazamiento cinemático directo constante
        rb.MovePosition(rb.position + direccionMover * speed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si choca con el Kitsune, le aplica el daño directo de vida
        KitsuneHealth playerHealth = other.GetComponent<KitsuneHealth>() ?? other.GetComponentInParent<KitsuneHealth>();
        if (playerHealth != null && !playerHealth.IsDead)
        {
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}