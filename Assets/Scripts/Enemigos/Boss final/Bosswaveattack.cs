using UnityEngine;

public class BossWaveAttack : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float speed = 4f; //
    [Tooltip("Distancia máxima que recorre antes de autodestruirse.")]
    [SerializeField] private float maxDistance = 35f;
    [SerializeField] private int damage = 12; //
    [SerializeField] private LayerMask playerLayer; //
    [SerializeField] private int direction = 1; //
    [SerializeField] private float fallSpeed = 14f; // Velocidad de caída firme hacia el piso

    private Vector3 startPosition; //
    private bool tocandoSuelo = false;
    private Collider2D miCollider;

    private void Start()
    {
        startPosition = transform.position; //
        miCollider = GetComponent<Collider2D>();

        // Forzamos que el Rigidbody2D sea Kinematic para evitar cualquier bug de gravedad de Unity
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            direction = (player.transform.position.x > transform.position.x) ? 1 : -1; //

            // Volteamos el sprite hacia el lado correcto
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction; //
            transform.localScale = scale; //
        }

        // Destrucción de seguridad para que no viva para siempre si se sale del mapa
        Destroy(gameObject, 6f); //
    }

    private void Update()
    {
        if (!tocandoSuelo)
        {
            // 🔥 DETECTOR DE ENTORNO AUTOMÁTICO:
            // Lanza una pequeña caja invisible justo debajo de los pies de la ola usando su propio tamaño de collider
            Vector2 posicionPies = new Vector2(transform.position.x, transform.position.y - 0.2f);
            Vector2 tamanoCaja = miCollider != null ? (Vector2)miCollider.bounds.size : new Vector2(0.8f, 0.2f);

            // Revisa si hay algo sólido abajo (excluyendo al jugador y otros proyectiles)
            Collider2D hit = Physics2D.OverlapBox(posicionPies, tamanoCaja, 0f);

            if (hit != null && !hit.CompareTag("Player") && !hit.CompareTag("Projectile") && !hit.name.Contains("Disparo"))
            {
                tocandoSuelo = true;
                // Clava la ola exactamente sobre el borde superior del bloque del suelo
                transform.position = new Vector3(transform.position.x, hit.bounds.max.y, transform.position.z);
            }
        }

        if (!tocandoSuelo)
        {
            // Caída recta y controlada hacia abajo si está en el aire
            transform.position = new Vector3(transform.position.x, transform.position.y - (fallSpeed * Time.deltaTime), transform.position.z);
            return; // No avanza horizontalmente hasta que toque el piso
        }

        // 🔥 AVANCE HORIZONTAL SOBRE EL SUELO:
        transform.position = new Vector3(transform.position.x + (direction * speed * Time.deltaTime), transform.position.y, transform.position.z);

        // Imán de piso nativo para subir o bajar desniveles de las plataformas de la arena
        RaycastHit2D hitAjuste = Physics2D.Raycast(transform.position + Vector3.up * 0.5f, Vector2.down, 2.5f);
        if (hitAjuste.collider != null && !hitAjuste.collider.CompareTag("Player") && !hitAjuste.collider.CompareTag("Projectile") && !hitAjuste.collider.name.Contains("Disparo"))
        {
            transform.position = new Vector3(transform.position.x, hitAjuste.point.y, transform.position.z);
        }

        // Destrucción por distancia máxima recorrida
        if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
        {
            Destroy(gameObject); //
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Lógica de daño al jugador Kitsune (Te atraviesa haciendo daño de forma justa sin destruirse)
        if (((1 << other.gameObject.layer) & playerLayer) != 0) //
        {
            IDamageable damageable = other.GetComponent<IDamageable>(); //
            if (damageable == null) damageable = other.GetComponentInParent<IDamageable>(); //

            if (damageable != null)
            {
                damageable.TakeDamage(damage); //
            }
        }
    }
}