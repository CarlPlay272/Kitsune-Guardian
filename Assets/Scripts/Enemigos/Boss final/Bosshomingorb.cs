using UnityEngine;

public class BossHomingOrb : MonoBehaviour
{
    public enum LostTargetBehavior
    {
        ContinueStraight,
        StopFollowing
    }

    [Header("Configuración")]
    [SerializeField] private float speed = 3.5f; //[cite: 16]
    [SerializeField] private float duration = 6f; //[cite: 16]
    [SerializeField] private int damage = 10; //[cite: 16]
    [SerializeField] private LayerMask playerLayer; //[cite: 16]
    [SerializeField] private LostTargetBehavior behaviorOnLostTarget = LostTargetBehavior.ContinueStraight; //[cite: 16]

    private Transform target;
    private IInvisibilityProvider invisibilityProvider;
    private Vector2 lastKnownDirection = Vector2.right; //[cite: 16]
    private bool hasTarget = true; //[cite: 16]

    public void Initialize(Transform playerTransform, IInvisibilityProvider invisibility)
    {
        target = playerTransform; //[cite: 16]
        invisibilityProvider = invisibility; //[cite: 16]
        Destroy(gameObject, duration); //[cite: 16]

        Rigidbody2D rb = GetComponent<Rigidbody2D>(); //[cite: 16]
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic; //[cite: 16]
            rb.gravityScale = 0f; //[cite: 16]
            rb.linearVelocity = Vector2.zero; //[cite: 16]
        }
    }

    private void Update()
    {
        bool playerInvisible = invisibilityProvider != null && invisibilityProvider.IsInvisible; //[cite: 16]

        if (target != null && !playerInvisible)
        {
            hasTarget = true; //[cite: 16]
            lastKnownDirection = ((Vector2)target.position - (Vector2)transform.position).normalized; //[cite: 16]
        }
        else
        {
            hasTarget = false; //[cite: 16]
        }

        if (hasTarget)
        {
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime); //[cite: 16]
        }
        else if (behaviorOnLostTarget == LostTargetBehavior.ContinueStraight)
        {
            transform.Translate(lastKnownDirection * speed * Time.deltaTime, Space.World); //[cite: 16]
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0) //[cite: 16]
        {
            if (hasTarget) //[cite: 16]
            {
                IDamageable damageable = other.GetComponent<IDamageable>(); //[cite: 16]
                if (damageable == null) damageable = other.GetComponentInParent<IDamageable>(); //[cite: 16]

                if (damageable != null)
                {
                    damageable.TakeDamage(damage); //[cite: 16]
                    Destroy(gameObject); //[cite: 16]
                }
            }
        }
    }
}