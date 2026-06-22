using System.Collections;
using UnityEngine;

public class TenguState : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxHealth = 1; //
    [SerializeField] private int currentHealth; //

    [Header("Referencias")]
    [SerializeField] private Animator animator; //
    [SerializeField] private Collider2D[] collidersToDisable; //
    [SerializeField] private Rigidbody2D rb; //

    [Header("Estado")]
    [SerializeField] private bool isDead = false; //

    public bool IsDead => isDead; //

    void Awake()
    {
        currentHealth = maxHealth; //

        if (animator == null)
            animator = GetComponent<Animator>(); //

        if (rb == null)
            rb = GetComponent<Rigidbody2D>(); //

        if (collidersToDisable == null || collidersToDisable.Length == 0)
            collidersToDisable = GetComponentsInChildren<Collider2D>(); //
    }

    public void TakeHit(int damage = 1)
    {
        if (isDead)
            return; //

        currentHealth -= damage; //

        Debug.Log("Tengu recibió daño. Vida restante: " + currentHealth); //

        if (currentHealth <= 0)
        {
            Die(); //
        }
        else
        {
            if (animator != null)
            {
                Debug.Log("ACTIVANDO HURT"); //
                animator.SetTrigger("Hurt"); //
            }
        }
    }

    private void Die()
    {
        if (isDead)
            return; //

        isDead = true; //

        if (animator != null)
        {
            animator.SetTrigger("Death"); //
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; //
            rb.angularVelocity = 0f; //
            rb.simulated = false; //
        }

        foreach (Collider2D col in collidersToDisable)
        {
            if (col != null)
            {
                col.enabled = false; //
            }
        }

        if (GameController.Instance != null)
        {
            GameController.Instance.ActivarPlataformaSalto(); //
            GameController.Instance.SumarPunto(1); //
        }

        Debug.Log("Tengu derrotado"); //

        // 🔥 PARCHE DE LIMPIEZA AUTOMÁTICA: Iniciamos una rutina para borrar el cuerpo
        // de la escena tras 2 segundos de animación, evitando sprites fantasmas pegados.
        StartCoroutine(RutinaDestruccionCuerpo());
    }

    private IEnumerator RutinaDestruccionCuerpo()
    {
        yield return new WaitForSeconds(2f);
        DestroyEnemy();
    }

    public void DestroyEnemy()
    {
        Destroy(gameObject); //
    }

    public float ObtenerPorcentajeVida()
    {
        if (maxHealth <= 0) return 0f; //
        return (float)currentHealth / maxHealth; //
    }
}