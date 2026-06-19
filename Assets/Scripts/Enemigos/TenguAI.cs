using System.Collections;
using UnityEngine;

public class TenguAI : MonoBehaviour
{
    private enum EstadoTengu
    {
        Patrullando,
        Esperando,
        Persiguiendo,
        Atacando,
        Regresando,
        Muerto
    }

    [Header("Referencias")]
    [SerializeField] private Transform patrolPointA;
    [SerializeField] private Transform patrolPointB;
    [SerializeField] private Transform player;
    [SerializeField] private Transform graphics;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private TenguState tenguState;

    [Header("Movimiento")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 7f;
    [SerializeField] private float returnSpeed = 3f;
    [SerializeField] private float pointReachDistance = 0.35f;

    [Header("Espera")]
    [SerializeField] private float waitTimeMin = 1.2f;
    [SerializeField] private float waitTimeMax = 2f;

    [Header("Detección")]
    [SerializeField] private float detectionRange = 9f;
    [SerializeField] private float closeDetectionRange = 1.7f;
    [SerializeField] private float verticalDetectionHeight = 2.4f;
    [SerializeField] private float combatHeightOffset = -0.55f;

    [Header("Persecución")]
    [SerializeField] private float stopChaseDistance = 2f;
    [SerializeField] private float maxChaseDistanceFromOrigin = 10f;

    [Header("Ataque")]
    [SerializeField] private float attackRadius = 1.1f;
    [SerializeField] private float attackConnectRadius = 2f;
    [SerializeField] private float frontalDamage = 16f;
    [SerializeField] private float arribaDamage = 55f;
    [SerializeField] private float attackCooldown = 0.45f;
    [SerializeField] private float attackStopTime = 0.22f;
    [SerializeField] private float frontalMeleeLockDistance = 2f;
    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private EstadoTengu estadoActual = EstadoTengu.Patrullando;
    private Transform currentPatrolTarget;
    private Vector2 spawnPosition;
    private Vector2 movementTarget;
    private float currentMoveSpeed = 0f;
    private float lastAttackTime = -999f;
    private bool waitingCoroutineRunning = false;

    private float attackStopTimer = 0f;
    private bool attackDamagePending = false;
    private float attackDamageMoment = 0f;

    public Transform Graphics => graphics;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (tenguState == null)
            tenguState = GetComponent<TenguState>();
    }

    void Start()
    {
        spawnPosition = rb.position;

        if (patrolPointA != null && patrolPointB != null)
            currentPatrolTarget = patrolPointB;

        if (player == null && GameController.Instance != null && GameController.Instance.Player != null)
            player = GameController.Instance.Player.transform;
    }

    void Update()
    {
        if (attackStopTimer > 0f)
            attackStopTimer -= Time.deltaTime;

        if (attackDamagePending && Time.time >= attackDamageMoment)
            ResolverImpactoPendiente();

        if (tenguState != null && tenguState.IsDead)
        {
            CambiarEstado(EstadoTengu.Muerto);
            currentMoveSpeed = 0f;
            ActualizarAnimacion(0f, false);
            return;
        }

        if (player == null)
        {
            EjecutarPatrulla();
            return;
        }

        KitsuneController kitsuneController = player.GetComponent<KitsuneController>();
        if (kitsuneController == null)
            kitsuneController = player.GetComponentInParent<KitsuneController>();

        bool playerInvisible = kitsuneController != null && kitsuneController.IsInvisible;

        float distanceToPlayer = Vector2.Distance(rb.position, player.position);
        float distanceFromOrigin = Vector2.Distance(rb.position, spawnPosition);

        bool playerInFront = JugadorEstaAlFrente();
        bool playerVeryClose = distanceToPlayer <= closeDetectionRange;
        bool playerVeryCloseFront = JugadorMuyCercaFrontal();
        bool playerOnTop = JugadorEstaEncima();
        bool playerInsideAttackZone = EstaJugadorEnZonaDeAtaque();
        bool shouldMeleeLock = DebeBloquearseParaAtacar();

        bool playerAboveOrNear =
            Mathf.Abs(player.position.x - transform.position.x) <= closeDetectionRange + 0.25f &&
            Mathf.Abs(player.position.y - AlturaCombateTengu()) <= verticalDetectionHeight;

        if (playerInvisible)
        {
            playerVeryClose = false;
            playerVeryCloseFront = false;
            playerOnTop = false;
            playerInsideAttackZone = false;
            shouldMeleeLock = false;
            playerAboveOrNear = false;
        }

        bool playerDetected = !playerInvisible && (
            (playerInFront && distanceToPlayer <= detectionRange) ||
            playerVeryClose ||
            playerAboveOrNear ||
            playerOnTop
        );

        bool playerInsideChaseZone = distanceFromOrigin <= maxChaseDistanceFromOrigin;

        switch (estadoActual)
        {
            case EstadoTengu.Patrullando:
                if (playerOnTop || shouldMeleeLock)
                {
                    StopAllCoroutines();
                    waitingCoroutineRunning = false;
                    CambiarEstado(EstadoTengu.Atacando);
                }
                else if (playerDetected && playerInsideChaseZone)
                {
                    StopAllCoroutines();
                    waitingCoroutineRunning = false;
                    CambiarEstado(EstadoTengu.Persiguiendo);
                }
                else
                {
                    EjecutarPatrulla();
                }
                break;

            case EstadoTengu.Esperando:
                if (playerOnTop || shouldMeleeLock)
                {
                    StopAllCoroutines();
                    waitingCoroutineRunning = false;
                    CambiarEstado(EstadoTengu.Atacando);
                }
                else if (playerDetected && playerInsideChaseZone)
                {
                    StopAllCoroutines();
                    waitingCoroutineRunning = false;
                    CambiarEstado(EstadoTengu.Persiguiendo);
                }
                else
                {
                    currentMoveSpeed = 0f;
                    ActualizarAnimacion(0f, false);
                }
                break;

            case EstadoTengu.Persiguiendo:
                if (!playerInsideChaseZone)
                {
                    CambiarEstado(EstadoTengu.Regresando);
                }
                else if (shouldMeleeLock || playerInsideAttackZone || playerVeryCloseFront || playerOnTop || distanceToPlayer <= stopChaseDistance || playerAboveOrNear)
                {
                    CambiarEstado(EstadoTengu.Atacando);
                }
                else if (!playerDetected)
                {
                    CambiarEstado(EstadoTengu.Regresando);
                }
                else
                {
                    PrepararPersecucion();
                }
                break;

            case EstadoTengu.Atacando:
                if (!playerInsideChaseZone)
                {
                    CambiarEstado(EstadoTengu.Regresando);
                }
                else if (!attackDamagePending && !shouldMeleeLock && !playerInsideAttackZone && !playerVeryCloseFront && !playerOnTop && distanceToPlayer > frontalMeleeLockDistance + 0.75f && !playerAboveOrNear)
                {
                    CambiarEstado(EstadoTengu.Persiguiendo);
                }
                else if (playerInvisible)
                {
                    CambiarEstado(EstadoTengu.Regresando);
                }
                else
                {
                    PrepararAtaque();
                }
                break;

            case EstadoTengu.Regresando:
                if (playerOnTop || shouldMeleeLock)
                {
                    CambiarEstado(EstadoTengu.Atacando);
                }
                else if (playerDetected && playerInsideChaseZone)
                {
                    CambiarEstado(EstadoTengu.Persiguiendo);
                }
                else
                {
                    PrepararRegreso();
                }
                break;

            case EstadoTengu.Muerto:
                currentMoveSpeed = 0f;
                ActualizarAnimacion(0f, false);
                break;
        }
    }

    void FixedUpdate()
    {
        if (estadoActual == EstadoTengu.Muerto) return;

        if (estadoActual == EstadoTengu.Atacando || attackStopTimer > 0f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (currentMoveSpeed > 0f)
        {
            Vector2 newPosition = Vector2.MoveTowards(rb.position, movementTarget, currentMoveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);
        }
    }

    void EjecutarPatrulla()
    {
        if (patrolPointA == null || patrolPointB == null || currentPatrolTarget == null)
        {
            currentMoveSpeed = 0f;
            ActualizarAnimacion(0f, false);
            return;
        }

        movementTarget = new Vector2(currentPatrolTarget.position.x, rb.position.y);

        FlipTowards(movementTarget);
        currentMoveSpeed = patrolSpeed;
        ActualizarAnimacion(patrolSpeed, false);

        if (Mathf.Abs(rb.position.x - movementTarget.x) <= pointReachDistance)
        {
            rb.position = new Vector2(movementTarget.x, rb.position.y);
            currentMoveSpeed = 0f;
            CambiarEstado(EstadoTengu.Esperando);

            if (!waitingCoroutineRunning)
                StartCoroutine(WaitAtPatrolPoint());
        }
    }

    IEnumerator WaitAtPatrolPoint()
    {
        waitingCoroutineRunning = true;
        ActualizarAnimacion(0f, false);

        float waitTime = Random.Range(waitTimeMin, waitTimeMax);
        yield return new WaitForSeconds(waitTime);

        currentPatrolTarget = currentPatrolTarget == patrolPointA ? patrolPointB : patrolPointA;
        FlipTowards(currentPatrolTarget.position);

        waitingCoroutineRunning = false;
        CambiarEstado(EstadoTengu.Patrullando);
    }

    void PrepararPersecucion()
    {
        if (DebeBloquearseParaAtacar())
        {
            currentMoveSpeed = 0f;
            rb.linearVelocity = Vector2.zero;

            ActualizarAnimacion(0f, true);
            return;
        }

        float desiredDistance = frontalMeleeLockDistance - 0.2f;

        float targetX;

        if (player.position.x > transform.position.x)
            targetX = player.position.x - desiredDistance;
        else
            targetX = player.position.x + desiredDistance;

        movementTarget = new Vector2(targetX, rb.position.y);

        FlipTowards(player.position);

        currentMoveSpeed = chaseSpeed;

        ActualizarAnimacion(chaseSpeed, true);
    }

    void PrepararAtaque()
    {
        currentMoveSpeed = 0f;
        rb.linearVelocity = Vector2.zero;
        FlipTowards(player.position);
        ActualizarAnimacion(0f, true);

        bool canStartAttack =
            DebeBloquearseParaAtacar() ||
            EstaJugadorEnZonaDeAtaque() ||
            JugadorMuyCercaFrontal() ||
            JugadorEstaEncima();

        if (!canStartAttack && !attackDamagePending)
            return;

        if (!attackDamagePending && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            attackStopTimer = attackStopTime;
            attackDamagePending = true;
            attackDamageMoment = Time.time + 0.08f;

            if (animator != null)
                animator.SetTrigger("Attack");
        }
    }

    void ResolverImpactoPendiente()
    {
        attackDamagePending = false;

        KitsuneController kitsuneController = player.GetComponent<KitsuneController>();
        if (kitsuneController == null)
            kitsuneController = player.GetComponentInParent<KitsuneController>();

        if (kitsuneController != null && kitsuneController.IsInvisible)
            return;

        KitsuneHealth kitsuneHealth = ObtenerKitsuneHealth();
        if (kitsuneHealth == null || kitsuneHealth.IsDead)
            return;

        bool puedeConectar =
            DebeBloquearseParaAtacar() ||
            EstaJugadorEnRangoExtendidoDeGolpe() ||
            JugadorMuyCercaFrontal() ||
            JugadorEstaEncima();

        if (!puedeConectar)
            return;

        float damageToApply = JugadorEstaEncima() ? arribaDamage : frontalDamage;
        kitsuneHealth.TakeDamage(damageToApply);

        if (debugLogs)
            Debug.Log("[Tengu] Ataque conectado. Daño: " + damageToApply);
    }

    void PrepararRegreso()
    {
        Transform nearestPoint = ObtenerPuntoPatrullaMasCercano();
        if (nearestPoint == null)
        {
            CambiarEstado(EstadoTengu.Patrullando);
            return;
        }

        movementTarget = new Vector2(nearestPoint.position.x, rb.position.y);
        FlipTowards(movementTarget);
        currentMoveSpeed = returnSpeed;
        ActualizarAnimacion(returnSpeed, false);

        if (Mathf.Abs(rb.position.x - movementTarget.x) <= pointReachDistance)
        {
            rb.position = new Vector2(movementTarget.x, rb.position.y);
            currentPatrolTarget = nearestPoint == patrolPointA ? patrolPointB : patrolPointA;
            CambiarEstado(EstadoTengu.Esperando);

            if (!waitingCoroutineRunning)
                StartCoroutine(WaitAtPatrolPoint());
        }
    }

    float AlturaCombateTengu()
    {
        return transform.position.y + combatHeightOffset;
    }

    bool DebeBloquearseParaAtacar()
    {
        if (player == null) return false;

        float dx = Mathf.Abs(player.position.x - transform.position.x);
        float dy = Mathf.Abs(player.position.y - AlturaCombateTengu());

        bool enfrente = JugadorEstaAlFrente();
        bool cercaHorizontal = dx <= frontalMeleeLockDistance;
        bool cercaVertical = dy <= 0.95f;

        return enfrente && cercaHorizontal && cercaVertical;
    }

    bool EstaJugadorEnZonaDeAtaque()
    {
        if (attackPoint == null || player == null) return false;

        Vector2 center = attackPoint.position;
        return Vector2.Distance(center, player.position) <= attackRadius;
    }

    bool EstaJugadorEnRangoExtendidoDeGolpe()
    {
        if (attackPoint == null || player == null) return false;

        Vector2 center = attackPoint.position;
        return Vector2.Distance(center, player.position) <= attackConnectRadius;
    }

    bool JugadorMuyCercaFrontal()
    {
        if (player == null) return false;

        float dx = Mathf.Abs(player.position.x - transform.position.x);
        float dy = Mathf.Abs(player.position.y - AlturaCombateTengu());

        bool cercaHorizontal = dx <= frontalMeleeLockDistance;
        bool cercaVertical = dy <= 0.95f;

        return cercaHorizontal && cercaVertical;
    }

    bool JugadorEstaEncima()
    {
        if (player == null) return false;

        float dx = Mathf.Abs(player.position.x - transform.position.x);
        float dy = player.position.y - transform.position.y;

        bool centradoEnX = dx <= 1.35f;
        bool encimaEnY = dy > 0.45f && dy <= verticalDetectionHeight + 0.6f;

        return centradoEnX && encimaEnY;
    }

    KitsuneHealth ObtenerKitsuneHealth()
    {
        if (player == null) return null;

        KitsuneHealth kitsuneHealth = player.GetComponent<KitsuneHealth>();
        if (kitsuneHealth == null)
            kitsuneHealth = player.GetComponentInParent<KitsuneHealth>();
        if (kitsuneHealth == null)
            kitsuneHealth = player.GetComponentInChildren<KitsuneHealth>();

        return kitsuneHealth;
    }

    Transform ObtenerPuntoPatrullaMasCercano()
    {
        if (patrolPointA == null) return patrolPointB;
        if (patrolPointB == null) return patrolPointA;

        float distA = Mathf.Abs(rb.position.x - patrolPointA.position.x);
        float distB = Mathf.Abs(rb.position.x - patrolPointB.position.x);

        return distA <= distB ? patrolPointA : patrolPointB;
    }

    bool JugadorEstaAlFrente()
    {
        if (player == null) return false;

        float offsetX = player.position.x - transform.position.x;

        if (EstaMirandoDerecha())
            return offsetX >= -0.45f;

        return offsetX <= 0.45f;
    }

    bool EstaMirandoDerecha()
    {
        if (graphics == null) return true;
        return graphics.localScale.x > 0f;
    }

    void FlipTowards(Vector2 target)
    {
        if (graphics == null) return;

        Vector3 scale = graphics.localScale;

        if (target.x < transform.position.x)
            scale.x = -Mathf.Abs(scale.x);
        else if (target.x > transform.position.x)
            scale.x = Mathf.Abs(scale.x);

        graphics.localScale = scale;
    }

    void ActualizarAnimacion(float speedValue, bool isChasing)
    {
        if (animator == null) return;

        animator.SetBool("IsChasing", isChasing);
        animator.SetFloat("Speed", speedValue);
    }

    void CambiarEstado(EstadoTengu nuevoEstado)
    {
        if (estadoActual == nuevoEstado) return;

        if (debugLogs)
            Debug.Log("[Tengu] Estado: " + estadoActual + " -> " + nuevoEstado);

        estadoActual = nuevoEstado;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, closeDetectionRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(Application.isPlaying ? spawnPosition : (Vector2)transform.position, maxChaseDistanceFromOrigin);

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);

            Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
            Gizmos.DrawWireSphere(attackPoint.position, attackConnectRadius);
        }

        Gizmos.color = Color.green;
        Vector3 lockCenter = new Vector3(transform.position.x, AlturaCombateTengu(), transform.position.z);
        Gizmos.DrawWireCube(lockCenter, new Vector3(frontalMeleeLockDistance * 2f, 1.9f, 0.1f));

        Vector3 boxCenter = new Vector3(transform.position.x, AlturaCombateTengu(), transform.position.z);
        Vector3 boxSize = new Vector3((closeDetectionRange + 0.25f) * 2f, verticalDetectionHeight * 2f, 0.1f);
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
}