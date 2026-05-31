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

    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;

    private float moveInput;
    private bool isGrounded;
    private bool facingRight = true;
    private bool isInWater = false;
    private bool controlBloqueado = false;

    void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (rb != null)
        {
            rb.gravityScale = normalGravityScale;
        }
    }

    void Update()
    {
        if (!controlBloqueado)
        {
            moveInput = Input.GetAxisRaw("Horizontal");
        }
        else
        {
            moveInput = 0f;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!controlBloqueado && Input.GetButtonDown("Jump"))
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

        if (!controlBloqueado)
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
        if (controlBloqueado) return;

        float currentSpeed = isInWater ? waterMoveSpeed : moveSpeed;

        Vector2 velocity = rb.linearVelocity;
        velocity.x = moveInput * currentSpeed;

        if (isInWater && velocity.y < maxFallSpeedInWater)
        {
            velocity.y = maxFallSpeedInWater;
        }

        rb.linearVelocity = velocity;
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
    }
}