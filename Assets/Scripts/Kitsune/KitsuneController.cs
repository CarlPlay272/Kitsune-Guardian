using System.Collections;
using UnityEngine;

public class KitsuneController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 22f;

    [Header("Agua")]
    [SerializeField] private float waterMoveSpeed = 3.5f;
    [SerializeField] private float waterJumpForce = 8f;
    [SerializeField] private float normalGravityScale = 3f;
    [SerializeField] private float waterGravityScale = 1f;
    [SerializeField] private float maxFallSpeedInWater = -4f;

    [Header("Suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Invisibilidad")]
    [SerializeField] private KeyCode teclaInvisibilidad = KeyCode.Q;
    [SerializeField] private float duracionMaximaInvisibilidad = 4f;
    [SerializeField] private float cooldownInvisibilidad = 6f;
    [SerializeField] private float alphaInvisible = 0.18f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private float doubleTapWindow = 0.25f;
    [SerializeField] private bool invulnerableDuringDash = true;
    [SerializeField] private float dashHitRange = 1.2f;

    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;

    [Header("Disparo (Mecánica Cuphead 8-Dirs)")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private KeyCode shootKey = KeyCode.J;
    [SerializeField] private KeyCode lockMovementKey = KeyCode.LeftControl; // Ctrl para quedarse estático
    [SerializeField] private float shootCooldown = 0.15f;
    [SerializeField] private float spiritCostPerShot = 1f;

    [Header("Recuperación Espíritu")]
    [SerializeField] private float spiritRecoveryInterval = 5f;
    [SerializeField] private float spiritRecoveryAmount = 2f;

    private float nextShootTime = 0f;
    private float nextSpiritRecoveryTime = 0f;

    private KitsuneSpirit kitsuneSpirit;

    private float moveInput;
    private bool isGrounded;
    private bool facingRight = true;
    private bool isInWater = false;
    private bool controlBloqueado = false;
    private bool isMovementLocked = false; // Flag del modo estático

    private bool isInvisible = false;
    private bool invisibilityOnCooldown = false;
    private float invisibilityEndTime = 0f;

    private bool isDashing = false;
    private bool dashOnCooldown = false;
    private float lastTapLeftTime = -999f;
    private float lastTapRightTime = -999f;
    private int dashDirection = 0;
    private bool dashDamageApplied = false;
    private bool ghostDashDamageApplied = false; // Flag estricta para golpear solo a una fantasma por Dash

    private SpriteRenderer[] spriteRenderers;
    private Collider2D[] playerColliders;

    public bool IsInvisible => isInvisible;
    public bool InvisibilityOnCooldown => invisibilityOnCooldown;
    public float TiempoRestanteInvisibilidad => isInvisible ? Mathf.Max(0f, invisibilityEndTime - Time.time) : 0f;
    public bool IsDashing => isDashing;

    void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponent<Animator>();

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        playerColliders = GetComponentsInChildren<Collider2D>();
        kitsuneSpirit = GetComponent<KitsuneSpirit>();
    }

    void Start()
    {
        if (rb != null)
            rb.gravityScale = normalGravityScale;
    }

    void Update()
    {
        // Activa el modo estático si sostienes Ctrl
        isMovementLocked = Input.GetKey(lockMovementKey);

        if (!controlBloqueado && !isMovementLocked)
            moveInput = Input.GetAxisRaw("Horizontal");
        else
            moveInput = 0f;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Voltear sprite con A/D estando quieto en Ctrl
        if (isMovementLocked && !controlBloqueado && !isDashing)
        {
            if (Input.GetKeyDown(KeyCode.D) && !facingRight)
                Flip();
            else if (Input.GetKeyDown(KeyCode.A) && facingRight)
                Flip();
        }

        if (!controlBloqueado && !isDashing && Input.GetButtonDown("Jump") && !isMovementLocked)
        {
            if (isInWater)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, waterJumpForce);

                if (animator != null)
                    animator.SetTrigger("Jump");
            }
            else if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

                if (animator != null)
                    animator.SetTrigger("Jump");
            }
        }

        GestionarInputInvisibilidad();
        GestionarInputDash();
        GestionarInputDisparo();
        RecuperarEspirituInvisible();

        if (isInvisible && Time.time >= invisibilityEndTime)
        {
            DesactivarInvisibilidad();
        }

        if (!controlBloqueado && !isDashing && !isMovementLocked)
        {
            if (moveInput > 0 && !facingRight)
                Flip();
            else if (moveInput < 0 && facingRight)
                Flip();
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", isMovementLocked ? 0f : Mathf.Abs(moveInput));
            animator.SetBool("IsGrounded", isGrounded);
        }
    }

    void GestionarInputDisparo()
    {
        if (GameController.Instance == null)
            return;

        if (!GameController.Instance.DisparoDesbloqueado)
            return;

        if (controlBloqueado)
            return;

        if (isDashing)
            return;

        if (fireballPrefab == null)
            return;

        if (shootPoint == null)
            return;

        if (kitsuneSpirit == null)
            return;

        if (Time.time < nextShootTime)
            return;

        if (!Input.GetKeyDown(shootKey))
            return;

        if (!kitsuneSpirit.ConsumeSpirit(spiritCostPerShot))
        {
            Debug.Log("Sin espíritu suficiente");
            return;
        }

        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }

        nextShootTime = Time.time + shootCooldown;

        // ===============================================================
        // SISTEMA DE APUNTADO EN 8 DIRECCIONES (CUPHEAD STYLE)
        // ===============================================================
        Vector2 direccionDisparo = facingRight ? Vector2.right : Vector2.left;

        float vertical = 0f;
        if (Input.GetKey(KeyCode.W)) vertical = 1f;  // Arriba
        if (Input.GetKey(KeyCode.S)) vertical = -1f; // Abajo

        float horizontal = 0f;
        if (Input.GetKey(KeyCode.D)) horizontal = 1f;   // Derecha
        if (Input.GetKey(KeyCode.A)) horizontal = -1f;  // Izquierda

        // Si se está presionando una dirección vertical, recalculamos el vector
        if (vertical != 0f)
        {
            // Si además se presiona izquierda o derecha, se vuelve diagonal
            if (horizontal != 0f)
            {
                direccionDisparo = new Vector2(horizontal, vertical).normalized;
            }
            else
            {
                // Si no hay input horizontal, dispara vertical puro (Arriba o Abajo)
                direccionDisparo = new Vector2(0f, vertical);
            }
        }
        else if (isMovementLocked && horizontal != 0f)
        {
            // Si está en Ctrl y solo presiona horizontal, respeta esa dirección
            direccionDisparo = new Vector2(horizontal, 0f);
        }

        GameObject fireball = Instantiate(
            fireballPrefab,
            shootPoint.position,
            Quaternion.identity
        );

        Fireball fireballScript = fireball.GetComponent<Fireball>();

        if (fireballScript != null)
        {
            fireballScript.SetDirection(direccionDisparo);
        }

        Debug.Log("Disparo realizado en dirección: " + direccionDisparo);
    }

    void RecuperarEspirituInvisible()
    {
        if (!isInvisible)
            return;

        if (kitsuneSpirit == null)
            return;

        if (Time.time < nextSpiritRecoveryTime)
            return;

        nextSpiritRecoveryTime =
            Time.time + spiritRecoveryInterval;

        kitsuneSpirit.AddSpirit(
            spiritRecoveryAmount
        );
    }

    void FixedUpdate()
    {
        if (controlBloqueado || isDashing) return;

        float currentSpeed = isInWater ? waterMoveSpeed : moveSpeed;

        Vector2 velocity = rb.linearVelocity;

        // Si está apuntando con Ctrl, la velocidad horizontal se clava a cero
        velocity.x = isMovementLocked ? 0f : moveInput * currentSpeed;

        if (isInWater && velocity.y < maxFallSpeedInWater)
            velocity.y = maxFallSpeedInWater;

        rb.linearVelocity = velocity;
    }

    void GestionarInputInvisibilidad()
    {
        if (GameController.Instance == null) return;
        if (!GameController.Instance.InvisibilidadDesbloqueada) return;
        if (!Input.GetKeyDown(teclaInvisibilidad)) return;
        if (isDashing) return;

        if (isInvisible)
        {
            DesactivarInvisibilidad();
            return;
        }

        if (invisibilityOnCooldown)
            return;

        ActivarInvisibilidad();
    }

    void ActivarInvisibilidad()
    {
        isInvisible = true;
        invisibilityEndTime = Time.time + duracionMaximaInvisibilidad;

        nextSpiritRecoveryTime = Time.time + spiritRecoveryInterval;

        AplicarTransparencia(alphaInvisible);
        IgnorarColisionConTengus(true);

        Debug.Log("Kitsune invisible ACTIVADO");
    }

    void DesactivarInvisibilidad()
    {
        if (!isInvisible) return;

        isInvisible = false;

        if (!isDashing)
            IgnorarColisionConTengus(false);

        AplicarTransparencia(1f);

        StartCoroutine(CooldownInvisibilidadRutina());

        Debug.Log("Kitsune invisible DESACTIVADO");
    }

    IEnumerator CooldownInvisibilidadRutina()
    {
        invisibilityOnCooldown = true;
        yield return new WaitForSeconds(cooldownInvisibilidad);
        invisibilityOnCooldown = false;
        Debug.Log("Cooldown de invisibilidad terminado");
    }

    void GestionarInputDash()
    {
        if (GameController.Instance == null) return;
        if (!GameController.Instance.DashDesbloqueado) return;
        if (isDashing || dashOnCooldown || controlBloqueado || isMovementLocked) return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (Time.time - lastTapLeftTime <= doubleTapWindow)
            {
                IniciarDash(-1);
                return;
            }

            lastTapLeftTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (Time.time - lastTapRightTime <= doubleTapWindow)
            {
                IniciarDash(1);
                return;
            }

            lastTapRightTime = Time.time;
        }
    }

    void IniciarDash(int direction)
    {
        if (isDashing || dashOnCooldown) return;

        StartCoroutine(DashRutina(direction));
    }

    IEnumerator DashRutina(int direction)
    {
        isDashing = true;
        dashOnCooldown = true;
        dashDirection = direction;
        dashDamageApplied = false;
        ghostDashDamageApplied = false; // Limpia el candado al empezar la carrera
        controlBloqueado = true;

        if (direction > 0 && !facingRight)
            Flip();
        else if (direction < 0 && facingRight)
            Flip();

        if (invulnerableDuringDash)
        {
            IgnorarColisionConTengus(true);
        }

        rb.gravityScale = 0f;

        float tiempo = 0f;

        while (tiempo < dashDuration)
        {
            tiempo += Time.deltaTime;
            rb.linearVelocity = new Vector2(direction * dashSpeed, 0f);

            IntentarDanarTenguConDash(direction);
            IntentarDanarFantasmaConDash(); // Ejecuta el ataque contra las fantasmas lentas

            yield return null;
        }

        rb.gravityScale = isInWater ? waterGravityScale : normalGravityScale;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (invulnerableDuringDash && !isInvisible)
        {
            IgnorarColisionConTengus(false);
        }

        isDashing = false;
        controlBloqueado = false;

        yield return new WaitForSeconds(dashCooldown);
        dashOnCooldown = false;
    }

    void IntentarDanarTenguConDash(int direction)
    {
        if (!isDashing) return;
        if (dashDamageApplied) return;

        GameObject[] tengus = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject tengu in tengus)
        {
            if (tengu == null) continue;

            TenguState tenguState = tengu.GetComponent<TenguState>();
            if (tenguState == null || tenguState.IsDead) continue;

            float distancia = Vector2.Distance(transform.position, tengu.transform.position);
            if (distancia > dashHitRange) continue;

            TenguAI tenguAI = tengu.GetComponent<TenguAI>();
            bool tenguMiraDerecha = true;

            if (tenguAI != null && tenguAI.Graphics != null)
                tenguMiraDerecha = tenguAI.Graphics.localScale.x > 0f;

            bool golpePorEspalda;

            if (tenguMiraDerecha)
                golpePorEspalda = transform.position.x < tengu.transform.position.x;
            else
                golpePorEspalda = transform.position.x > tengu.transform.position.x;

            // CORREGIDO: Se repara el error de sintaxis ortográfica aquí
            bool dashVaHaciaElTengu =
                (direction == 1 && transform.position.x <= tengu.transform.position.x + 0.5f) ||
                (direction == -1 && transform.position.x >= tengu.transform.position.x - 0.5f);

            if (golpePorEspalda && dashVaHaciaElTengu)
            {
                tenguState.TakeHit(1);
                dashDamageApplied = true;
                Debug.Log("¡Dash por retaguardia conectado al Tengu!");
                return;
            }
        }
    }

    // ===============================================================
    // MECÁNICA DE EMERGENCIA: ATACA SOLAMENTE A UNA FANTASMA POR DASH
    // ===============================================================
    void IntentarDanarFantasmaConDash()
    {
        if (!isDashing || ghostDashDamageApplied) return;

        ZoneBossGhostAI[] fantasmas = Object.FindObjectsByType<ZoneBossGhostAI>(FindObjectsSortMode.None);
        foreach (ZoneBossGhostAI ghost in fantasmas)
        {
            if (ghost == null) continue;

            GhostHealth health = ghost.GetComponent<GhostHealth>();
            if (health == null || health.IsDead) continue;

            float distancia = Vector2.Distance(transform.position, ghost.transform.position);

            if (distancia <= dashHitRange)
            {
                health.TakeHit(1); // Le descuenta vida de emergencia
                ghostDashDamageApplied = true; // Bloquea el daño en esta carrera
                Debug.Log("💥 [DASH] Ataque de emergencia conectado con éxito a una sola fantasma.");
                return;
            }
        }
    }

    void AplicarTransparencia(float alpha)
    {
        if (spriteRenderers == null) return;

        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr == null) continue;

            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    void IgnorarColisionConTengus(bool ignorar)
    {
        GameObject[] tengus = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject tengu in tengus)
        {
            Collider2D[] tenguColliders = tengu.GetComponentsInChildren<Collider2D>();

            foreach (Collider2D playerCol in playerColliders)
            {
                if (playerCol == null) continue;

                foreach (Collider2D tenguCol in tenguColliders)
                {
                    if (tenguCol == null) continue;
                    Physics2D.IgnoreCollision(playerCol, tenguCol, ignorar);
                }
            }
        }
    }

    void Flip()
    {
        facingRight = !facingRight;

        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    public void EnterWater()
    {
        isInWater = true;
        rb.gravityScale = waterGravityScale;
    }

    public void ExitWater()
    {
        isInWater = false;
        rb.gravityScale = normalGravityScale;
    }

    public bool IsInWater()
    {
        return isInWater;
    }

    public void AplicarKnockback(Vector2 fuerza, float duracionBloqueo)
    {
        StartCoroutine(KnockbackRutina(fuerza, duracionBloqueo));
    }

    private IEnumerator KnockbackRutina(Vector2 fuerza, float duracionBloqueo)
    {
        controlBloqueado = true;
        rb.linearVelocity = fuerza;

        yield return new WaitForSeconds(duracionBloqueo);

        controlBloqueado = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, dashHitRange);
    }

    public void BloquearControles()
    {
        controlBloqueado = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void DesbloquearControles()
    {
        controlBloqueado = false;
    }
}