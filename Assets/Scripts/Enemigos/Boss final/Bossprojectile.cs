using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int damage = 8; //[cite: 19]
    [SerializeField] private float lifeTime = 5f; //[cite: 19]
    [SerializeField] private LayerMask playerLayer; //[cite: 19]
    [SerializeField] private LayerMask obstacleLayer; //[cite: 19]

    private Vector2 direction;
    private float speed;
    private bool initialized = false;

    public void Initialize(Vector2 moveDirection, float moveSpeed)
    {
        direction = moveDirection.normalized; //[cite: 19]
        speed = moveSpeed; //[cite: 19]
        initialized = true; //[cite: 19]

        Destroy(gameObject, lifeTime); //[cite: 19]

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; //[cite: 19]
        transform.rotation = Quaternion.Euler(0f, 0f, angle); //[cite: 19]
    }

    private void Update()
    {
        if (!initialized) return; //[cite: 19]
        transform.Translate(direction * speed * Time.deltaTime, Space.World); //[cite: 19]
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Projectile") || other.name.Contains("Disparo") || other.name.Contains("Fuego"))
        {
            return;
        }

        if (((1 << other.gameObject.layer) & playerLayer) != 0) //[cite: 19]
        {
            IInvisibilityProvider invisibility = other.GetComponentInParent<IInvisibilityProvider>(); //[cite: 19]
            if (invisibility == null) invisibility = other.GetComponent<IInvisibilityProvider>(); //[cite: 19]

            bool isInvisible = invisibility != null && invisibility.IsInvisible; //[cite: 19]

            if (!isInvisible) //[cite: 19]
            {
                IDamageable damageable = other.GetComponent<IDamageable>(); //[cite: 19]
                if (damageable == null) damageable = other.GetComponentInParent<IDamageable>(); //[cite: 19]

                if (damageable != null)
                {
                    damageable.TakeDamage(damage); //[cite: 19]
                    Destroy(gameObject); //[cite: 19]
                }
            }
            return; //[cite: 19]
        }

        if (((1 << other.gameObject.layer) & obstacleLayer) != 0) //[cite: 19]
        {
            Destroy(gameObject); //[cite: 19]
        }
    }
}