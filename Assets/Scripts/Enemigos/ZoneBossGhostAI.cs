using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GhostHealth))]
public class ZoneBossGhostAI : MonoBehaviour
{
    public enum TipoEnemigo { MeleeConDash, TiradoraEstatica }
    private enum EstadoGhost { Retornando, BuscandoAlZorro, EjecutandoAtaque, Exhausta, RecibiendoDano }

    [Header("Configuración de Tipo")]
    [SerializeField] private TipoEnemigo tipoDeFantasma = TipoEnemigo.MeleeConDash;

    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GhostHealth ghostHealth;

    [Header("Movimiento Base")]
    [SerializeField] private float floatSpeed = 2.5f;
    [SerializeField] private float floatMagnitude = 0.25f;
    [SerializeField] private float movementSpeed = 3.5f;

    [Header("Configuración General de Ataque")]
    [SerializeField] private float attackDamage = 15f;

    [Header("Configuración Tipo Melee (Dash Esquivable)")]
    [SerializeField] private float radioAlertaDash = 4.5f;
    [SerializeField] private float velocidadDash = 18f;
    [SerializeField] private float duracionDash = 0.4f;
    [SerializeField] private float tiempoExhausta = 1.8f;
    [SerializeField] private float radioRafagaAtaque = 3.2f;

    [Header("Configuración Tipo Tiradora (Inteligente)")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float radioFrenadoTiro = 6.5f;
    [SerializeField] private float cooldownDisparo = 1.8f;
    [SerializeField] private float offsetAlturaMiras = 0.8f;
    [Tooltip("Distancia vertical fija que mantendrá por encima del jugador.")]
    [SerializeField] private float alturaDeseadaSobrePlayer = 3.5f;
    [Tooltip("Si el jugador se acerca a menos de esta distancia horizontal, la tiradora retrocederá.")]
    [SerializeField] private float radioRetiradaEvasiva = 4.0f;

    private EstadoGhost estadoActual = EstadoGhost.Retornando;
    private KitsuneController playerController;
    private Transform playerTransform;
    private Vector3 originalSpawnPosition;

    private bool kitsuneEnZonaJefe = false;
    private float localTimer = 0f;
    private float nextShootTime = 0f;
    private bool ejecutandoRutinaAtaque = false;
    private Coroutine rutinaActual;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (ghostHealth == null) ghostHealth = GetComponent<GhostHealth>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Start()
    {
        originalSpawnPosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<KitsuneController>();
            playerTransform = playerObj.transform;
        }
    }

    public void KitsuneEntroALaZona() { kitsuneEnZonaJefe = true; }
    public void KitsuneSalioDeLaZona() { kitsuneEnZonaJefe = false; }

    public void InterrumpirPorGolpe()
    {
        if (rutinaActual != null)
        {
            StopCoroutine(rutinaActual);
        }

        ejecutandoRutinaAtaque = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
        }

        if (ghostHealth != null && !ghostHealth.IsDead)
        {
            estadoActual = EstadoGhost.RecibiendoDano;
            StartCoroutine(RutinaRecuperacionPostGolpe());
        }
    }

    IEnumerator RutinaRecuperacionPostGolpe()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.5f);

        if (ghostHealth != null && !ghostHealth.IsDead)
        {
            if (animator != null)
            {
                animator.ResetTrigger("Hurt");
                animator.SetBool("IsChasing", true);
            }
            estadoActual = EstadoGhost.BuscandoAlZorro;
        }
    }

    void Update()
    {
        if (ghostHealth != null && ghostHealth.IsDead)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        localTimer += Time.deltaTime;

        bool playerEsInvisible = playerController != null && playerController.IsInvisible;
        bool debePerseguir = kitsuneEnZonaJefe && !playerEsInvisible && playerTransform != null;

        if (estadoActual != EstadoGhost.EjecutandoAtaque &&
            estadoActual != EstadoGhost.Exhausta &&
            estadoActual != EstadoGhost.RecibiendoDano)
        {
            if (debePerseguir)
                estadoActual = EstadoGhost.BuscandoAlZorro;
            else
                estadoActual = EstadoGhost.Retornando;
        }

        if (estadoActual == EstadoGhost.BuscandoAlZorro && tipoDeFantasma == TipoEnemigo.TiradoraEstatica)
        {
            GestionarAtaqueDistancia();
        }

        SincronizarAnimator();
    }

    void FixedUpdate()
    {
        if (ghostHealth != null && ghostHealth.IsDead) return;
        if (ejecutandoRutinaAtaque) return;
        if (estadoActual == EstadoGhost.RecibiendoDano)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        float offsetSin = Mathf.Sin(localTimer * floatSpeed) * floatMagnitude;
        Vector2 posicionDestino = rb.position;

        if (estadoActual == EstadoGhost.BuscandoAlZorro && playerTransform != null)
        {
            AplicarOrientacion(playerTransform.position.x);

            if (tipoDeFantasma == TipoEnemigo.MeleeConDash)
            {
                float distanciaAlPlayer = Vector2.Distance(transform.position, playerTransform.position);
                posicionDestino = Vector2.MoveTowards(rb.position, playerTransform.position, movementSpeed * Time.fixedDeltaTime);

                if (distanciaAlPlayer <= radioAlertaDash && !ejecutandoRutinaAtaque)
                {
                    rutinaActual = StartCoroutine(RutinaDashMelee());
                }
            }
            else if (tipoDeFantasma == TipoEnemigo.TiradoraEstatica)
            {
                // 🔥 LÓGICA DE TIRADORA AVANZADA
                // 1. Calcular la posición ideal manteniendo siempre el "High Ground" sobre el Kitsune
                Vector2 objetivoFlotante = new Vector2(playerTransform.position.x, playerTransform.position.y + alturaDeseadaSobrePlayer);

                float distanciaHorizontal = Mathf.Abs(rb.position.x - playerTransform.position.x);
                float distanciaReal = Vector2.Distance(rb.position, playerTransform.position);

                // 2. Comportamiento Evasivo (Kiteo): Si el Kitsune se acerca demasiado, retroceder horizontalmente
                if (distanciaHorizontal < radioRetiradaEvasiva)
                {
                    // Determinar hacia dónde huir (en dirección opuesta al jugador)
                    float direccionHuidaX = (rb.position.x > playerTransform.position.x) ? 1f : -1f;

                    // Su nuevo objetivo horizontal será alejarse un rango seguro, manteniendo la altura fija
                    objetivoFlotante = new Vector2(playerTransform.position.x + (direccionHuidaX * radioFrenadoTiro), playerTransform.position.y + alturaDeseadaSobrePlayer);
                    posicionDestino = Vector2.MoveTowards(rb.position, objetivoFlotante, movementSpeed * Time.fixedDeltaTime);
                }
                // 3. Persecución a distancia: Si el jugador está muy lejos, se acerca horizontalmente pero SIN bajar de nivel
                else if (distanciaReal > radioFrenadoTiro)
                {
                    posicionDestino = Vector2.MoveTowards(rb.position, objetivoFlotante, movementSpeed * Time.fixedDeltaTime);
                }
                // 4. Mantener la altura si está en rango óptimo de tiro
                else
                {
                    // Si ya está en la distancia correcta, solo corrige su altura para no caer al suelo
                    Vector2 soloAltura = new Vector2(rb.position.x, playerTransform.position.y + alturaDeseadaSobrePlayer);
                    posicionDestino = Vector2.MoveTowards(rb.position, soloAltura, movementSpeed * Time.fixedDeltaTime);
                }
            }
        }
        else if (estadoActual == EstadoGhost.Retornando)
        {
            AplicarOrientacion(originalSpawnPosition.x);
            posicionDestino = Vector2.MoveTowards(rb.position, originalSpawnPosition, movementSpeed * Time.fixedDeltaTime);
        }
        else if (estadoActual == EstadoGhost.Exhausta)
        {
            posicionDestino = rb.position;
        }

        // Aplicar el suave flote místico sinusoidal en el eje Y
        posicionDestino.y += offsetSin * Time.fixedDeltaTime;
        rb.MovePosition(posicionDestino);
    }

    IEnumerator RutinaDashMelee()
    {
        ejecutandoRutinaAtaque = true;
        estadoActual = EstadoGhost.EjecutandoAtaque;

        if (animator != null) animator.SetTrigger("Attack");

        Vector2 puntoUltimaPosicionPlayer = playerTransform.position;
        Vector2 direccionDash = (puntoUltimaPosicionPlayer - rb.position).normalized;

        float tiempoTranscurrido = 0f;
        while (tiempoTranscurrido < duracionDash)
        {
            tiempoTranscurrido += Time.deltaTime;
            rb.MovePosition(rb.position + direccionDash * velocidadDash * Time.deltaTime);
            yield return null;
        }

        Collider2D[] objetosImpactados = Physics2D.OverlapCircleAll(transform.position, radioRafagaAtaque);
        foreach (Collider2D col in objetosImpactados)
        {
            if (col.CompareTag("Player"))
            {
                KitsuneHealth h = col.GetComponent<KitsuneHealth>() ??
                                  col.GetComponentInParent<KitsuneHealth>() ??
                                  col.GetComponentInChildren<KitsuneHealth>();

                if (h != null && !h.IsDead)
                {
                    h.TakeDamage(attackDamage);
                    Debug.Log("💥 [FANTASMA MELEE] ¡Ataque de largo alcance conectado!");
                    break;
                }
            }
        }

        estadoActual = EstadoGhost.Exhausta;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(tiempoExhausta);

        ejecutandoRutinaAtaque = false;
        estadoActual = EstadoGhost.BuscandoAlZorro;
    }

    void GestionarAtaqueDistancia()
    {
        if (bulletPrefab == null) return;
        if (Time.time < nextShootTime) return;

        float distanciaAlPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanciaAlPlayer <= radioFrenadoTiro + 2f)
        {
            nextShootTime = Time.time + cooldownDisparo;

            if (animator != null) animator.SetTrigger("Attack");

            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            GhostBullet scriptBala = bullet.GetComponent<GhostBullet>();
            if (scriptBala != null)
            {
                Vector3 objetivoCorregido = playerTransform.position + new Vector3(0f, offsetAlturaMiras, 0f);
                scriptBala.InicializarDireccion(objetivoCorregido);
            }
        }
    }

    void AplicarOrientacion(float targetX)
    {
        bool debeMirarDerecha = targetX > transform.position.x;
        Vector3 escala = transform.localScale;
        escala.x = debeMirarDerecha ? Mathf.Abs(escala.x) : -Mathf.Abs(escala.x);
        transform.localScale = escala;
    }

    void SincronizarAnimator()
    {
        if (animator == null) return;

        if (estadoActual == EstadoGhost.RecibiendoDano)
        {
            animator.SetBool("IsChasing", false);
            return;
        }

        bool moviendo = estadoActual == EstadoGhost.BuscandoAlZorro || estadoActual == EstadoGhost.Retornando;
        animator.SetBool("IsChasing", moviendo);
    }

    void OnDrawGizmosSelected()
    {
        if (tipoDeFantasma == TipoEnemigo.MeleeConDash)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radioAlertaDash);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radioRafagaAtaque);
        }
        else
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, radioFrenadoTiro);

            // Gizmo Celeste: Para ver en el editor el rango donde la tiradora empezará a huir
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, radioRetiradaEvasiva);
        }
    }
}