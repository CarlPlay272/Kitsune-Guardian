using UnityEngine;

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

    [Header("Espera")]
    [SerializeField] private float waitTimeAtPoint = 1.5f;

    [Header("Detección")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float maxHeightDifference = 2.5f;

    [Header("Ataque")]
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackHeightTolerance = 0.6f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackDamage = 10f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private EstadoTengu estadoActual = EstadoTengu.Patrullando;

    private Transform patrolTarget;
    private float waitTimer;
    private bool isWaiting;

    private float lastAttackTime = -999f;
    private float moveDirectionX;

    // FIX: requerido por KitsuneController
    public Transform Graphics => transform;

    void Awake()
    {
        animator ??= GetComponent<Animator>();
        rb ??= GetComponent<Rigidbody2D>();
        tenguState ??= GetComponent<TenguState>();

        if (player == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");
            if (obj != null) player = obj.transform;
        }
    }

    void Start()
    {
        patrolTarget = patrolPointA != null ? patrolPointA : patrolPointB;
    }

    void Update()
    {
        if (tenguState != null && tenguState.IsDead)
        {
            DetenerMovimientoCompleto();
            SincronizarAnimator();
            return;
        }

        if (player == null) return;

        // 🔥 NUEVO: detectar invisibilidad del Kitsune
        KitsuneController kitsune = player.GetComponent<KitsuneController>();
        bool playerInvisible = kitsune != null && kitsune.IsInvisible;

        bool detected = !playerInvisible && JugadorDetectado();
        bool inAttack = detected && JugadorEnRangoDeAtaque();

        switch (estadoActual)
        {
            case EstadoTengu.Patrullando:
                ActualizarPatrulla();
                if (detected)
                    CambiarEstado(EstadoTengu.Persiguiendo, "detected");
                break;

            case EstadoTengu.Persiguiendo:
                if (!detected)
                {
                    VolverAPatrullaMasCercana();
                    CambiarEstado(EstadoTengu.Patrullando, "lost or invisible");
                    break;
                }

                if (inAttack)
                {
                    CambiarEstado(EstadoTengu.Atacando, "attack range");
                    break;
                }

                ActualizarPersecucion();
                break;

            case EstadoTengu.Atacando:
                DetenerMovimientoHorizontal();
                AplicarOrientacion(player.position.x);

                // 🔥 IMPORTANTE: si se vuelve invisible durante ataque, se cancela
                if (!detected)
                {
                    CambiarEstado(EstadoTengu.Persiguiendo, "became invisible");
                    break;
                }

                if (!inAttack)
                {
                    CambiarEstado(EstadoTengu.Persiguiendo, "out of range");
                    break;
                }

                IntentarAtacar();
                break;
        }

        SincronizarAnimator();
    }

    // -------------------------------------------------------
    // DETECCIÓN
    // -------------------------------------------------------

    bool JugadorDetectado()
    {
        float dx = Mathf.Abs(transform.position.x - player.position.x);
        float dy = Mathf.Abs(transform.position.y - player.position.y);

        return dx <= detectionRange && dy <= maxHeightDifference;
    }

    bool JugadorEnRangoDeAtaque()
    {
        float dx = Mathf.Abs(transform.position.x - player.position.x);
        float dy = Mathf.Abs(transform.position.y - player.position.y);

        return dx <= attackRange && dy <= attackHeightTolerance;
    }

    // -------------------------------------------------------
    // MOVIMIENTO
    // -------------------------------------------------------

    void ActualizarPatrulla()
    {
        if (patrolTarget == null) return;

        float dir = patrolTarget.position.x - transform.position.x;

        if (Mathf.Abs(dir) <= pointReachDistance)
        {
            isWaiting = true;
            waitTimer = waitTimeAtPoint;
            moveDirectionX = 0;
            return;
        }

        moveDirectionX = Mathf.Sign(dir);
        AplicarOrientacion(patrolTarget.position.x);
    }

    void ActualizarPersecucion()
    {
        moveDirectionX = Mathf.Sign(player.position.x - transform.position.x);
        AplicarOrientacion(player.position.x);
    }

    void FixedUpdate()
    {
        if (estadoActual == EstadoTengu.Atacando) return;

        if (rb != null && moveDirectionX != 0)
        {
            float speed = estadoActual == EstadoTengu.Persiguiendo ? chaseSpeed : patrolSpeed;
            rb.MovePosition(rb.position + new Vector2(moveDirectionX * speed * Time.fixedDeltaTime, 0));
        }
    }

    // -------------------------------------------------------
    // ATAQUE
    // -------------------------------------------------------

    void IntentarAtacar()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;

        animator?.SetTrigger("Attack");
        DealDamage();
    }

    public void DealDamage()
    {
        if (!JugadorEnRangoDeAtaque()) return;

        KitsuneController kitsune = player.GetComponent<KitsuneController>();

        // 🔥 SEGURIDAD EXTRA: NO DAÑO SI INVISIBLE
        if (kitsune != null && kitsune.IsInvisible)
            return;

        KitsuneHealth hp = player.GetComponent<KitsuneHealth>();

        if (hp != null && !hp.IsDead)
            hp.TakeDamage(attackDamage);
    }

    // -------------------------------------------------------
    // ESTADOS
    // -------------------------------------------------------

    void CambiarEstado(EstadoTengu nuevo, string reason)
    {
        if (estadoActual == nuevo) return;

        estadoActual = nuevo;

        if (nuevo == EstadoTengu.Atacando)
            DetenerMovimientoHorizontal();
    }

    // -------------------------------------------------------
    // UTILIDADES
    // -------------------------------------------------------

    void AplicarOrientacion(float targetX)
    {
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (targetX > transform.position.x ? 1 : -1);
        transform.localScale = s;
    }

    void DetenerMovimientoHorizontal()
    {
        moveDirectionX = 0;
        if (rb != null)
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    void DetenerMovimientoCompleto()
    {
        moveDirectionX = 0;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    void VolverAPatrullaMasCercana()
    {
        if (patrolPointA == null || patrolPointB == null) return;

        float da = Mathf.Abs(transform.position.x - patrolPointA.position.x);
        float db = Mathf.Abs(transform.position.x - patrolPointB.position.x);

        patrolTarget = da < db ? patrolPointA : patrolPointB;
    }

    void SincronizarAnimator()
    {
        if (animator == null) return;

        animator.SetBool("IsChasing", estadoActual != EstadoTengu.Patrullando);
    }
}