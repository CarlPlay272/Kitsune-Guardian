using UnityEngine;

/// <summary>
/// Onda horizontal usada en el ataque "Ola Corrupta", disponible solo en Fase 2.
/// Avanza por el suelo recorriendo toda la arena. Se esquiva saltando.
/// La invisibilidad NO protege contra este ataque (a propósito).
/// </summary>
public class BossWaveAttack : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float speed = 4f;
    [Tooltip("Distancia máxima que recorre antes de autodestruirse (cubrir el ancho de la arena).")]
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private int damage = 12;
    [SerializeField] private LayerMask playerLayer;
    [Tooltip("1 = avanza a la derecha, -1 = avanza a la izquierda.")]
    [SerializeField] private int direction = 1;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime, Space.World);

        if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            // No se comprueba invisibilidad a propósito: este ataque ignora la invisibilidad del jugador.
            IDamageable damageable = other.GetComponent<IDamageable>();
            damageable?.TakeDamage(damage);
        }
    }
}