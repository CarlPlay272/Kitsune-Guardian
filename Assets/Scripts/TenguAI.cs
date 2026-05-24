using UnityEngine;

public class TenguAI : MonoBehaviour
{
    [Header("References")]
    public Transform patrolPointA;
    public Transform patrolPointB;
    public Transform player;
    public Transform graphics;
    public Animator animator;
    public Rigidbody2D rb;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.5f;
    public float pointReachDistance = 0.2f;

    [Header("Detection")]
    public float detectionRange = 5f;
    public float attackRange = 1.2f;

    [Header("Attack")]
    public float damage = 10f;
    public float attackCooldown = 1.2f;

    private Transform currentPatrolTarget;
    private bool isChasing = false;
    private bool isDead = false;
    private float lastAttackTime = -999f;
    private Vector2 movementTarget;
    private float currentMoveSpeed = 0f;

    void Start()
    {
        currentPatrolTarget = patrolPointB;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }

        if (isChasing)
        {
            animator.SetBool("IsChasing", true);

            if (distanceToPlayer > attackRange)
            {
                PrepareChase();
            }
            else
            {
                PrepareAttack();
            }
        }
        else
        {
            animator.SetBool("IsChasing", false);
            PreparePatrol();
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (currentMoveSpeed > 0f)
        {
            Vector2 newPosition = Vector2.MoveTowards(rb.position, movementTarget, currentMoveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);
        }
    }

    void PreparePatrol()
    {
        if (patrolPointA == null || patrolPointB == null) return;

        movementTarget = currentPatrolTarget.position;
        currentMoveSpeed = patrolSpeed;

        float speedValue = Vector2.Distance(rb.position, movementTarget) > 0.01f ? patrolSpeed : 0f;
        animator.SetFloat("Speed", speedValue);

        FlipTowards(movementTarget);

        if (Vector2.Distance(transform.position, movementTarget) <= pointReachDistance)
        {
            currentPatrolTarget = currentPatrolTarget == patrolPointA ? patrolPointB : patrolPointA;
        }
    }

    void PrepareChase()
    {
        movementTarget = player.position;
        currentMoveSpeed = chaseSpeed;

        animator.SetFloat("Speed", chaseSpeed);
        FlipTowards(movementTarget);
    }

    void PrepareAttack()
    {
        currentMoveSpeed = 0f;
        animator.SetFloat("Speed", 0f);
        FlipTowards(player.position);

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            animator.SetTrigger("Attack");

            KitsuneHealth kitsuneHealth = player.GetComponent<KitsuneHealth>();
            if (kitsuneHealth != null)
            {
                kitsuneHealth.TakeDamage(damage);
            }
        }
    }

    void FlipTowards(Vector2 target)
    {
        if (graphics == null) return;

        Vector3 scale = graphics.localScale;

        if (target.x < transform.position.x)
        {
            scale.x = -Mathf.Abs(scale.x);
        }
        else if (target.x > transform.position.x)
        {
            scale.x = Mathf.Abs(scale.x);
        }

        graphics.localScale = scale;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        animator.SetTrigger("Hurt");
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        currentMoveSpeed = 0f;
        animator.SetTrigger("Dead");

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}