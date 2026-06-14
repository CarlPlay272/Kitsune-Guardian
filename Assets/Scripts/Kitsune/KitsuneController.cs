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

    private float moveInput;
    private bool isGrounded;
    private bool facingRight = true;
    private bool isInWater = false;
    private bool controlBloqueado = false;

    private bool isInvisible = false;
    private bool invisibilityOnCooldown = false;
    private float invisibilityEndTime = 0f;

    private bool isDashing = false;
    private bool dashOnCooldown = false;
    private float lastTapLeftTime = -999f;
    private float lastTapRightTime = -999f;
    private int dashDirection = 0;
    private bool dashDamageApplied = false;

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
    }

    void Start()
    {
        if (rb != null)
            rb.gravityScale = normalGravityScale;
    }

    void Update()
    {
        if (!controlBloqueado)
            moveInput = Input.GetAxisRaw("Horizontal");
        else
            moveInput = 0f;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!controlBloqueado && !isDashing && Input.GetButtonDown("Jump"))
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

        if (isInvisible && Time.time >= invisibilityEndTime)
        {
            DesactivarInvisibilidad();
        }

        if (!controlBloqueado && !isDashing)
        {
            if (moveInput > 0 && !facingRight)
                Flip();
            else if (moveInput < 0 && facingRight)
                Flip();
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveInput));
            animator.SetBool("IsGrounded", isGrounded);
        }
    }

    void FixedUpdate()
    {
        if (controlBloqueado || isDashing) return;

        float currentSpeed = isInWater ? waterMoveSpeed : moveSpeed;

        Vector2 velocity = rb.linearVelocity;
        velocity.x = moveInput * currentSpeed;

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
        if (isDashing || dashOnCooldown || controlBloqueado) return;

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

        moveInput = 0f;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
    }

    public void DesbloquearControles()
    {
        controlBloqueado = false;
    }
}