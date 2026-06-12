using UnityEngine;

public class JumpPadController : MonoBehaviour
{
    [Header("Impulso")]
    [SerializeField] private float fuerzaSaltoVertical = 32f;
    [SerializeField] private float impulsoHorizontal = 0f;

    [Header("Control")]
    [SerializeField] private bool resetearVelocidadVertical = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        KitsuneController kitsuneController = other.GetComponentInParent<KitsuneController>();
        if (kitsuneController == null) return;

        Rigidbody2D rb = kitsuneController.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        Vector2 velocidadActual = rb.linearVelocity;

        if (resetearVelocidadVertical)
        {
            velocidadActual.y = 0f;
        }

        velocidadActual.y += fuerzaSaltoVertical;
        velocidadActual.x += impulsoHorizontal;

        rb.linearVelocity = velocidadActual;
    }
}