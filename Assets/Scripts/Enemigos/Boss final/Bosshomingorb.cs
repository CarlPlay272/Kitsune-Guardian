using UnityEngine;

/// <summary>
/// Orbe perseguidor usado por el ataque "Ojo Demoníaco".
/// Persigue al jugador mientras este sea visible. Si el jugador activa la invisibilidad,
/// el orbe pierde el objetivo: según configuración, sigue recto o se detiene.
/// Este ataque existe específicamente para darle utilidad táctica a la invisibilidad.
/// </summary>
public class BossHomingOrb : MonoBehaviour
{
    public enum LostTargetBehavior
    {
        ContinueStraight,
        StopFollowing
    }

    [Header("Configuración")]
    [SerializeField] private float speed = 3.5f;
    [SerializeField] private float duration = 6f;
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LostTargetBehavior behaviorOnLostTarget = LostTargetBehavior.ContinueStraight;

    private Transform target;
    private IInvisibilityProvider invisibilityProvider;
    private Vector2 lastKnownDirection = Vector2.right;
    private bool hasTarget = true;

    /// <summary>
    /// Debe llamarse justo después de instanciar el orbe.
    /// </summary>
    public void Initialize(Transform playerTransform, IInvisibilityProvider invisibility)
    {
        target = playerTransform;
        invisibilityProvider = invisibility;
        Destroy(gameObject, duration);
    }

    private void Update()
    {
        bool playerInvisible = invisibilityProvider != null && invisibilityProvider.IsInvisible;

        if (target != null && !playerInvisible)
        {
            hasTarget = true;
            lastKnownDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;
        }
        else
        {
            hasTarget = false;
        }

        if (hasTarget)
        {
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }
        else if (behaviorOnLostTarget == LostTargetBehavior.ContinueStraight)
        {
            transform.Translate(lastKnownDirection * speed * Time.deltaTime, Space.World);
        }
        // Si es StopFollowing, el orbe simplemente no se mueve mientras no tenga objetivo.
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            // Solo daña si el orbe efectivamente tenía objetivo en el momento del impacto.
            if (hasTarget)
            {
                IDamageable damageable = other.GetComponent<IDamageable>();
                damageable?.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}