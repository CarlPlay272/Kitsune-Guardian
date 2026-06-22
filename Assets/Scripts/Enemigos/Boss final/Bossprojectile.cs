using UnityEngine;

/// <summary>
/// Proyectil simple en línea recta usado por el ataque "Ráfaga Corrupta" del Oni.
/// Se destruye al salir de la pantalla, al expirar su vida útil o al impactar al jugador.
/// La invisibilidad del jugador SÍ protege contra este proyectil: si el jugador está
/// invisible al momento del impacto, el proyectil simplemente continúa su trayectoria.
/// </summary>
public class BossProjectile : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int damage = 8;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private LayerMask playerLayer;
    [Tooltip("Opcional: capa de paredes/suelo que detiene el proyectil al chocar.")]
    [SerializeField] private LayerMask obstacleLayer;

    private Vector2 direction;
    private float speed;
    private bool initialized = false;

    /// <summary>
    /// Debe llamarse justo después de instanciar el proyectil, antes del primer Update.
    /// </summary>
    public void Initialize(Vector2 moveDirection, float moveSpeed)
    {
        direction = moveDirection.normalized;
        speed = moveSpeed;
        initialized = true;

        Destroy(gameObject, lifeTime);

        // Orienta el sprite/objeto en la dirección de viaje. Ajustar o quitar según el arte usado.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Update()
    {
        if (!initialized) return;
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            IInvisibilityProvider invisibility = other.GetComponent<IInvisibilityProvider>();
            bool isInvisible = invisibility != null && invisibility.IsInvisible;

            if (!isInvisible)
            {
                IDamageable damageable = other.GetComponent<IDamageable>();
                damageable?.TakeDamage(damage);
                Destroy(gameObject);
            }
            // Si el jugador está invisible, el proyectil "lo atraviesa" y sigue su curso: esquivado.
            return;
        }

        if (((1 << other.gameObject.layer) & obstacleLayer) != 0)
        {
            Destroy(gameObject);
        }
    }
}