using UnityEngine;

/// <summary>
/// IA del enemigo Tengu para un plataformero 2D. Reescrita desde cero
/// alrededor de un único principio: UNA sola fuente de verdad por cosa.
///
///   - Una sola variable decide el estado (estadoActual).
///   - Una sola función decide hacia dónde mira el sprite (AplicarOrientacion).
///   - Una sola variable decide el movimiento horizontal (moveDirectionX),
///     y se resetea en el MISMO frame en que se entra a Atacando — nunca
///     queda movimiento "residual" de un estado anterior.
///   - Todas las distancias (detección, persecución, ataque) usan
///     SIEMPRE transform.position — nunca Collider2D.bounds. Mezclar
///     ambas referencias fue la causa de los bugs anteriores (el bounds
///     se desplaza cuando el sprite se flipea via localScale negativo).
///   - La detección y el ataque tienen FILTROS DE ALTURA INDEPENDIENTES:
///     maxHeightDifference (generoso, para perseguir entre plataformas
///     con desnivel) y attackHeightTolerance (estricto, para que solo
///     golpee si el jugador está realmente a su mismo nivel, no saltando
///     a la altura de la cabeza).
///   - Rango de ataque y detección usan SIEMPRE distancia horizontal pura
///     (Mathf.Abs en X), nunca Vector2.Distance — así no hay desfasajes
///     por diferencias de altura entre pivotes.
///
/// Sin NavMesh, sin pathfinding, sin saltos. Solo distancias en X y dos
/// filtros de altura en Y (uno para detectar, otro para atacar).
/// </summary>
public class TenguAI : MonoBehaviour
{
    private enum EstadoTengu
    {
        Patrullando,
        Persiguiendo,
        Atacando
    }

    [Header("Referencias")]
    [SerializeField] private Transform patrolPointA;
    [SerializeField] private Transform patrolPointB;
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private TenguState tenguState;

    [Header("Movimiento")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float pointReachDistance = 0.2f;

    [Header("Espera en puntos de patrulla")]
    [SerializeField] private float waitTimeAtPoint = 1.5f;

    [Header("Detección")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float maxHeightDifference = 1.5f;

    [Header("Ataque")]
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackHeightTolerance = 0.4f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackDamage = 10f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private EstadoTengu estadoActual = EstadoTengu.Patrullando;

    private Transform patrolTarget;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    private float lastAttackTime = -999f;
    private float moveDirectionX = 0f;

    public Transform Graphics => transform;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (tenguState == null) tenguState = GetComponent<TenguState>();
    }

    void Start()
    {
        patrolTarget = patrolPointB != null ? patrolPointB : patrolPointA;
    }

    void Update()
    {
        if (tenguState != null && tenguState.IsDead)
        {
            DetenerMovimientoCompleto();
            SincronizarAnimator();
            return;
        }

        bool playerDetected = JugadorDetectado();
        bool playerInAttackRange = playerDetected && JugadorEnRangoDeAtaque();

        if (debugLogs)
        {
            float alturaDebug = Mathf.Abs(transform.position.y - player.position.y);
            Debug.Log(
                "Detectado: " + playerDetected +
                " | Ataque: " + playerInAttackRange +
                " | DistX: " + DistanciaHorizontalAlJugador() +
                " | Altura: " + alturaDebug
            );
        }

        switch (estadoActual)
        {
            case EstadoTengu.Patrullando:
                ActualizarPatrulla();
                if (playerDetected)
                    CambiarEstado(EstadoTengu.Persiguiendo, "jugador detectado");
                break;

            case EstadoTengu.Persiguiendo:
                if (!playerDetected)
                {
                    if (debugLogs) Debug.Log("[Tengu] Objetivo perdido (fuera de rango o de altura).");
                    VolverAPatrullaMasCercana();
                    CambiarEstado(EstadoTengu.Patrullando, "objetivo perdido");
                    break;
                }

                if (playerInAttackRange)
                {
                    CambiarEstado(EstadoTengu.Atacando, "jugador en rango de ataque");
                    break;
                }

                ActualizarPersecucion();
                break;

            case EstadoTengu.Atacando:
                // Se frena ANTES de cualquier otra cosa, en el mismo frame
                // en que se evalúa este estado. No hay margen para que
                // quede movimiento residual.
                DetenerMovimientoHorizontal();
                AplicarOrientacion(player.position.x);

                if (!playerDetected)
                {
                    if (debugLogs) Debug.Log("[Tengu] Objetivo perdido durante el ataque.");
                    VolverAPatrullaMasCercana();
                    CambiarEstado(EstadoTengu.Patrullando, "objetivo perdido");
                    break;
                }

                if (!playerInAttackRange)
                {
                    CambiarEstado(EstadoTengu.Persiguiendo, "jugador salió del rango de ataque");
                    break;
                }

                IntentarAtacar();
                break;
        }

        SincronizarAnimator();

        if (debugLogs)
            Debug.Log("Estado: " + estadoActual + " | DistX: " + DistanciaHorizontalAlJugador());
    }

    void FixedUpdate()
    {
        if (tenguState != null && tenguState.IsDead) return;

        // El movimiento SOLO ocurre fuera del estado Atacando. No hay
        // ninguna otra ruta de código que mueva al Tengu.
        if (estadoActual == EstadoTengu.Atacando) return;

        if (moveDirectionX != 0f && rb != null)
        {
            float speed = estadoActual == EstadoTengu.Persiguiendo ? chaseSpeed : patrolSpeed;
            Vector2 nuevaPosicion = rb.position + new Vector2(moveDirectionX * speed * Time.fixedDeltaTime, 0f);
            rb.MovePosition(nuevaPosicion);
        }
    }

    // ---------------------------------------------------------------
    // PATRULLA
    // ---------------------------------------------------------------

    void ActualizarPatrulla()
    {
        if (patrolPointA == null || patrolPointB == null || patrolTarget == null)
        {
            moveDirectionX = 0f;
            return;
        }

        if (isWaiting)
        {
            moveDirectionX = 0f;
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                patrolTarget = patrolTarget == patrolPointA ? patrolPointB : patrolPointA;
                AplicarOrientacion(patrolTarget.position.x);
            }
            return;
        }

        float diferenciaX = patrolTarget.position.x - transform.position.x;

        if (Mathf.Abs(diferenciaX) <= pointReachDistance)
        {
            isWaiting = true;
            waitTimer = waitTimeAtPoint;
            moveDirectionX = 0f;
            return;
        }

        moveDirectionX = Mathf.Sign(diferenciaX);
        AplicarOrientacion(patrolTarget.position.x);
    }

    void VolverAPatrullaMasCercana()
    {
        if (patrolPointA == null || patrolPointB == null) return;

        float distA = Mathf.Abs(transform.position.x - patrolPointA.position.x);
        float distB = Mathf.Abs(transform.position.x - patrolPointB.position.x);

        patrolTarget = distA <= distB ? patrolPointA : patrolPointB;
        isWaiting = false;
    }

    // ---------------------------------------------------------------
    // PERSECUCIÓN
    // ---------------------------------------------------------------

    void ActualizarPersecucion()
    {
        float diferenciaX = player.position.x - transform.position.x;
        moveDirectionX = Mathf.Sign(diferenciaX);
        AplicarOrientacion(player.position.x);
    }

    // ---------------------------------------------------------------
    // ATAQUE
    // ---------------------------------------------------------------

    void IntentarAtacar()
    {
        if (Time.time < lastAttackTime + attackCooldown)
            return;

        lastAttackTime = Time.time;

        if (debugLogs) Debug.Log("[Tengu] Inicio de ataque.");

        if (animator != null)
            animator.SetTrigger("Attack");

        // Daño inmediato y simple. Si tu animación de ataque tiene un
        // frame de impacto claro, mové esta llamada a un Animation Event
        // que invoque DealDamage() y borrá la línea de abajo.
        DealDamage();
    }

    /// <summary>
    /// Aplica daño si el jugador sigue en rango. Pública para poder
    /// engancharse también desde un Animation Event.
    /// </summary>
    public void DealDamage()
    {
        if (player == null)
            return;

        if (!JugadorEnRangoDeAtaque())
            return;

        KitsuneHealth kitsuneHealth = player.GetComponent<KitsuneHealth>();

        if (kitsuneHealth == null)
            kitsuneHealth = player.GetComponentInParent<KitsuneHealth>();

        if (kitsuneHealth == null)
            kitsuneHealth = player.GetComponentInChildren<KitsuneHealth>();

        if (kitsuneHealth != null && !kitsuneHealth.IsDead)
        {
            kitsuneHealth.TakeDamage(attackDamage);

            if (debugLogs) Debug.Log("[Tengu] DAÑO APLICADO");
        }
    }

    // ---------------------------------------------------------------
    // DETECCIÓN (distancia horizontal pura + filtro de altura)
    // ---------------------------------------------------------------

    bool JugadorDetectado()
    {
        if (player == null)
            return false;

        float altura = Mathf.Abs(transform.position.y - player.position.y);
        float distanciaX = Mathf.Abs(transform.position.x - player.position.x);

        return altura <= maxHeightDifference &&
               distanciaX <= detectionRange;
    }

    /// <summary>
    /// Filtro de ataque INDEPENDIENTE del de detección. Usa su propia
    /// tolerancia de altura (attackHeightTolerance), mucho más estricta
    /// que maxHeightDifference, para que el Tengu solo golpee cuando el
    /// jugador está realmente a su mismo nivel — no cuando salta y queda
    /// a la altura de la cabeza pero dentro del rango "amplio" de detección.
    /// </summary>
    bool JugadorEnRangoDeAtaque()
    {
        if (player == null)
            return false;

        float altura = Mathf.Abs(transform.position.y - player.position.y);

        return DistanciaHorizontalAlJugador() <= attackRange &&
               altura <= attackHeightTolerance;
    }

    float DistanciaHorizontalAlJugador()
    {
        if (player == null)
            return 999f;

        return Mathf.Abs(transform.position.x - player.position.x);
    }

    // ---------------------------------------------------------------
    // MOVIMIENTO / ORIENTACIÓN — funciones únicas, llamadas desde un
    // solo lugar por frame en cada estado.
    // ---------------------------------------------------------------

    void DetenerMovimientoHorizontal()
    {
        moveDirectionX = 0f;
        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    void DetenerMovimientoCompleto()
    {
        moveDirectionX = 0f;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    void AplicarOrientacion(float targetX)
    {
        bool debeMirarDerecha = targetX > transform.position.x;

        Vector3 escala = transform.localScale;

        if (debeMirarDerecha)
            escala.x = Mathf.Abs(escala.x);
        else
            escala.x = -Mathf.Abs(escala.x);

        transform.localScale = escala;
    }

    // ---------------------------------------------------------------
    // ESTADO / ANIMATOR
    // ---------------------------------------------------------------

    void CambiarEstado(EstadoTengu nuevoEstado, string razon)
    {
        if (estadoActual == nuevoEstado) return;

        if (debugLogs)
            Debug.Log("[Tengu] Estado: " + estadoActual + " -> " + nuevoEstado + " (" + razon + ")");

        estadoActual = nuevoEstado;

        // Al cambiar de estado, jamás se arrastra movimiento del estado
        // anterior: se limpia acá, en el único punto de transición.
        if (nuevoEstado == EstadoTengu.Atacando)
            DetenerMovimientoHorizontal();
    }

    void SincronizarAnimator()
    {
        if (animator == null) return;

        bool isChasing = estadoActual == EstadoTengu.Persiguiendo || estadoActual == EstadoTengu.Atacando;
        float speed = Mathf.Abs(moveDirectionX) * (estadoActual == EstadoTengu.Persiguiendo ? chaseSpeed : patrolSpeed);

        animator.SetBool("IsChasing", isChasing);
        animator.SetFloat("Speed", speed);
    }

    void OnDrawGizmosSelected()
    {
        // Caja amarilla: zona de detección (amplia, tolera desnivel para perseguir)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(detectionRange * 2f, maxHeightDifference * 2f, 0.1f));

        // Caja roja: zona de ataque real (debe verse chata y ancha, no vertical)
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(attackRange * 2f, attackHeightTolerance * 2f, 0.1f));
    }
}