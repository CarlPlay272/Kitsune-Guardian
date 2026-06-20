using UnityEngine;

/// <summary>
/// IA para el enemigo volador (Weona que Vuela) del Nivel 2.
/// Diseñado bajo el principio de "Única fuente de verdad" para evitar desfasajes físicos.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(TenguState))]
public class FlyingGhostAI : MonoBehaviour
{
    private enum EstadoGhost
    {
        FlotandoEnSitio,
        PersiguiendoVuelo,
        Atacando
    }

    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private TenguState ghostState;

    [Header("Movimiento Flotante (Matemática Seno)")]
    [SerializeField] private float floatSpeed = 3f;       // Velocidad del balanceo vertical espectral
    [SerializeField] private float floatMagnitude = 0.3f;  // Amplitud del vaivén vertical

    [Header("Persecución en Arena")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float maxVerticalRange = 5f; // Rango vertical para seguir en plataformas

    [Header("Ataque / Emboscada")]
    [SerializeField] private float attackRange = 1.6f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackDamage = 15f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false; // Desactivado para no spamear tu consola

    private EstadoGhost estadoActual = EstadoGhost.FlotandoEnSitio;
    private float lastAttackTime = -999f;
    private float moveDirectionX = 0f;
    private float baseHeightY; // Altura base de referencia para la flotación
    private float localTimer = 0f;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (ghostState == null) ghostState = GetComponent<TenguState>();
    }

    void Start()
    {
        baseHeightY = transform.position.y;

        // Auto-busca al Kitsune por tag si no lo arrastraste en el Inspector
        if (player == null && GameObject.FindGameObjectWithTag("Player") != null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Update()
    {
        // Si el componente TenguState de la Naya dice que murió, frena todo
        if (ghostState != null && ghostState.IsDead)
        {
            DetenerMovimientoCompleto();
            return;
        }

        localTimer += Time.deltaTime;
        bool playerDetected = JugadorDetectado();
        bool playerInAttackRange = playerDetected && JugadorEnRangoDeAtaque();

        switch (estadoActual)
        {
            case EstadoGhost.FlotandoEnSitio:
                moveDirectionX = 0f;
                if (playerDetected)
                    CambiarEstado(EstadoGhost.PersiguiendoVuelo);
                break;

            case EstadoGhost.PersiguiendoVuelo:
                if (!playerDetected)
                {
                    baseHeightY = transform.position.y; // Guarda la altura donde quedó fija
                    CambiarEstado(EstadoGhost.FlotandoEnSitio);
                    break;
                }

                if (playerInAttackRange)
                {
                    CambiarEstado(EstadoGhost.Atacando);
                    break;
                }

                ActualizarPersecucionHorizontal();
                break;

            case EstadoGhost.Atacando:
                moveDirectionX = 0f;
                if (rb != null) rb.linearVelocity = Vector2.zero;

                if (player != null) AplicarOrientacion(player.position.x);

                if (!playerInAttackRange)
                {
                    CambiarEstado(EstadoGhost.PersiguiendoVuelo);
                    break;
                }

                IntentarAtacar();
                break;
        }

        SincronizarAnimator();
    }

    void FixedUpdate()
    {
        if (ghostState != null && ghostState.IsDead) return;

        // 1. Cálculo de vaivén vertical continuo usando función Seno
        float offsetSin = Mathf.Sin(localTimer * floatSpeed) * floatMagnitude;
        float targetY = baseHeightY + offsetSin;

        // 2. Cálculo de avance horizontal en X
        float targetX = rb.position.x;
        if (estadoActual == EstadoGhost.PersiguiendoVuelo && moveDirectionX != 0f)
        {
            targetX += moveDirectionX * chaseSpeed * Time.fixedDeltaTime;

            // Suavemente desplaza su altura base hacia el eje Y del jugador (Persecución 2D aérea)
            baseHeightY = Mathf.MoveTowards(baseHeightY, player.position.y, chaseSpeed * 0.6f * Time.fixedDeltaTime);
        }

        rb.MovePosition(new Vector2(targetX, targetY));
    }

    void ActualizarPersecucionHorizontal()
    {
        if (player == null) return;
        float diferenciaX = player.position.x - transform.position.x;
        moveDirectionX = Mathf.Sign(diferenciaX);
        AplicarOrientacion(player.position.x);
    }

    void IntentarAtacar()
    {
        if (Time.time < lastAttackTime + attackCooldown)
            return;

        lastAttackTime = Time.time;

        if (animator != null)
            animator.SetTrigger("Attack");

        DealDamageToKitsune();
    }

    public void DealDamageToKitsune()
    {
        if (player == null || !JugadorEnRangoDeAtaque()) return;

        KitsuneHealth kitsuneHealth = player.GetComponent<KitsuneHealth>() ?? player.GetComponentInParent<KitsuneHealth>();

        if (kitsuneHealth != null && !kitsuneHealth.IsDead)
        {
            kitsuneHealth.TakeDamage(attackDamage);
            if (debugLogs) Debug.Log("[Ghost] Ataque espiritual conectado al Kitsune!");
        }
    }

    bool JugadorDetectado()
    {
        if (player == null) return false;
        float distanciaX = Mathf.Abs(transform.position.x - player.position.x);
        float distanciaY = Mathf.Abs(transform.position.y - player.position.y);
        return distanciaX <= detectionRange && distanciaY <= maxVerticalRange;
    }

    bool JugadorEnRangoDeAtaque()
    {
        if (player == null) return false;
        float distanciaX = Mathf.Abs(transform.position.x - player.position.x);
        float distanciaY = Mathf.Abs(transform.position.y - player.position.y);
        return distanciaX <= attackRange && distanciaY <= attackRange;
    }

    void AplicarOrientacion(float targetX)
    {
        bool debeMirarDerecha = targetX > transform.position.x;
        Vector3 escala = transform.localScale;
        escala.x = debeMirarDerecha ? Mathf.Abs(escala.x) : -Mathf.Abs(escala.x);
        transform.localScale = escala;
    }

    void CambiarEstado(EstadoGhost nuevoEstado)
    {
        if (estadoActual == nuevoEstado) return;
        estadoActual = nuevoEstado;
    }

    void SincronizarAnimator()
    {
        if (animator == null) return;
        bool isChasing = estadoActual == EstadoGhost.PersiguiendoVuelo;
        animator.SetBool("IsChasing", isChasing);
    }

    void DetenerMovimientoCompleto()
    {
        moveDirectionX = 0f;
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    void OnDrawGizmosSelected()
    {
        // Rango de visión de la fantasma (Caja Azul Celeste)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(detectionRange * 2f, maxVerticalRange * 2f, 0.1f));

        // Rango de ataque (Esfera Roja)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}