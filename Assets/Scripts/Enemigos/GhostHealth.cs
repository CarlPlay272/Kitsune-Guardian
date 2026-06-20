using UnityEngine;

public class GhostHealth : MonoBehaviour
{
    [Header("Vida de la Fantasma")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currentHealth;

    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private Collider2D[] collidersToDisable;
    [SerializeField] private Rigidbody2D rb;

    [Header("Configuración de Muerte")]
    [SerializeField] private float tiempoParaDestruir = 0.25f; // Reducido al mínimo para una desaparición limpia y fluida

    private bool isDead = false;
    public bool IsDead => isDead;

    void Awake()
    {
        currentHealth = maxHealth;

        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if (collidersToDisable == null || collidersToDisable.Length == 0)
            collidersToDisable = GetComponentsInChildren<Collider2D>();
    }

    public void TakeHit(int damage = 1)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log("👻 [FANTASMA] Recibió daño. Vida restante: " + currentHealth);

        // Forzar al Animator a limpiar el movimiento antes de entrar al dolor
        if (animator != null)
        {
            animator.SetBool("IsChasing", false);
        }

        // Avisar a la IA que suspenda el Dash físico por el impacto
        ZoneBossGhostAI ia = GetComponent<ZoneBossGhostAI>();
        if (ia != null) ia.InterrumpirPorGolpe();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null)
            {
                animator.SetTrigger("Hurt");
            }
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("👻 [FANTASMA] Eliminada.");

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        foreach (Collider2D col in collidersToDisable)
        {
            if (col != null) col.enabled = false;
        }

        if (GameController.Instance != null)
        {
            GameController.Instance.SumarPunto(1);
        }

        // Destrucción inmediata para evitar retrasos toscos en la arena
        Destroy(gameObject, tiempoParaDestruir);
    }
}