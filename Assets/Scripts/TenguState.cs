using UnityEngine;

public class TenguState : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private int currentHealth;

    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private Collider2D[] collidersToDisable;
    [SerializeField] private Rigidbody2D rb;

    [Header("Estado")]
    [SerializeField] private bool isDead = false;

    public bool IsDead => isDead;

    void Awake()
    {
        currentHealth = maxHealth;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (collidersToDisable == null || collidersToDisable.Length == 0)
            collidersToDisable = GetComponentsInChildren<Collider2D>();
    }

    public void TakeHit(int damage = 1)
    {
        if (isDead) return;

        currentHealth -= damage;

        Debug.Log("Tengu recibió daño. Vida restante: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;

        if (animator != null)
            animator.SetTrigger("Die");

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        foreach (Collider2D col in collidersToDisable)
        {
            if (col != null)
                col.enabled = false;
        }

        if (GameController.Instance != null)
        {
            GameController.Instance.ActivarPlataformaSalto();
            GameController.Instance.SumarPunto(1);
        }

        Debug.Log("Tengu derrotado");
    }
}