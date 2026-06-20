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
    [SerializeField] private float movementSpeed = 3.5f;    // SUBIDO: De 3f a 3.5f para que te persigan más rápido

    [Header("Configuración General de Ataque")]
    [SerializeField] private float attackDamage = 15f;

    [Header("Configuración Tipo Melee (Dash Esquivable)")]
    [SerializeField] private float radioAlertaDash = 4.5f;   // SUBIDO: Te detecta un pelo antes para iniciar la embestida
    [SerializeField] private float velocidadDash = 18f;      // SUBIDO: De 16f a 18f ¡Ahora se lanza a toda hostia![cite: 1]
    [SerializeField] private float duracionDash = 0.4f;      // AJUSTADO: Menos duración porque va más rápido, manteniendo la distancia[cite: 1]
    [SerializeField] private float tiempoExhausta = 1.8f;
    [Tooltip("El radio del golpe circular. Subido para alcanzar al jugador desde más lejos.")]
    [SerializeField] private float radioRafagaAtaque = 3.2f; // SUBIDO: De 2.5f a 3.2f para que te pegue desde más lejos aunque lo esquives[cite: 1]

    [Header("Configuración Tipo Tiradora")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float radioFrenadoTiro = 6.5f;
    [SerializeField] private float cooldownDisparo = 1.8f;    // AJUSTADO: Dispara un pelo más rápido para meter más presión[cite: 1]
    [Tooltip("Ajusta qué tan arriba apunta la bala para compensar el pivote bajo del Kitsune.")]
    [SerializeField] private float offsetAlturaMiras = 0.8f;

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
            if (animator != null) animator.SetBool("IsChasing", true);
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
            float distanciaAlPlayer = Vector2.Distance(transform.position, playerTransform.position);

            if (tipoDeFantasma == TipoEnemigo.MeleeConDash)
            {
                posicionDestino = Vector2.MoveTowards(rb.position, playerTransform.position, movementSpeed * Time.fixedDeltaTime);

                if (distanciaAlPlayer <= radioAlertaDash && !ejecutandoRutinaAtaque)
                {
                    rutinaActual = StartCoroutine(RutinaDashMelee());
                }
            }
            else if (tipoDeFantasma == TipoEnemigo.TiradoraEstatica)
            {
                if (distanciaAlPlayer > radioFrenadoTiro)
                {
                    posicionDestino = Vector2.MoveTowards(rb.position, playerTransform.position, movementSpeed * Time.fixedDeltaTime);
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

        // HITBOX EXTENDIDO: Escanea un área mucho mayor (3.2m) para conectar golpes lejanos de forma implacable[cite: 1]
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
        bool moviendo = estadoActual == EstadoGhost.BuscandoAlZorro || estadoActual == EstadoGhost.Retornando;
        animator.SetBool("IsChasing", moviendo);
    }

    void OnDrawGizmosSelected()
    {
        if (tipoDeFantasma == TipoEnemigo.MeleeConDash)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radioAlertaDash);

            // Círculo Amarillo: Muestra visualmente el nuevo rango gigante del golpe[cite: 1]
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radioRafagaAtaque);
        }
        else
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, radioFrenadoTiro);
        }
    }
}