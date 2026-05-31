using System.Collections;
using UnityEngine;

public class PortalMetaController : MonoBehaviour
{
    [Header("Destino")]
    [SerializeField] private Transform destinoTeletransporte;
    [SerializeField] private Vector2 empujeSalida = new Vector2(4f, 2f);

    [Header("Bloqueo sin llave")]
    [SerializeField] private bool requiereLlave = true;
    [SerializeField] private float danioBloqueo = 10f;
    [SerializeField] private Vector2 empujeBloqueo = new Vector2(6f, 2f);
    [SerializeField] private float duracionKnockback = 0.25f;
    [SerializeField] private GameObject voidBloqueoVisual;
    [SerializeField] private float duracionVoidVisual = 0.6f;

    [Header("Control")]
    [SerializeField] private float cooldownPortalJugador = 0.8f;

    private bool mostrandoVoid = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        KitsuneHealth kitsuneHealth = other.GetComponentInParent<KitsuneHealth>();
        if (kitsuneHealth == null) return;
        if (kitsuneHealth.IsDead) return;

        KitsuneController kitsuneController = kitsuneHealth.GetComponent<KitsuneController>();
        Rigidbody2D rb = kitsuneHealth.GetComponent<Rigidbody2D>();
        KitsunePortalState portalState = kitsuneHealth.GetComponent<KitsunePortalState>();

        if (rb == null || kitsuneController == null || portalState == null) return;
        if (!portalState.PuedeUsarPortal) return;

        if (requiereLlave && (GameController.Instance == null || !GameController.Instance.TieneLlave))
        {
            RechazarJugador(kitsuneHealth, kitsuneController, portalState, kitsuneHealth.transform.position);
            return;
        }

        TeletransportarJugador(kitsuneHealth.transform, rb, portalState);
    }

    private void RechazarJugador(KitsuneHealth kitsuneHealth, KitsuneController kitsuneController, KitsunePortalState portalState, Vector3 posicionJugador)
    {
        kitsuneHealth.TakeDamage(danioBloqueo);

        float direccion = posicionJugador.x < transform.position.x ? -1f : 1f;
        Vector2 fuerzaFinal = new Vector2(direccion * empujeBloqueo.x, empujeBloqueo.y);

        kitsuneController.AplicarKnockback(fuerzaFinal, duracionKnockback);
        portalState.BloquearPortales(cooldownPortalJugador);

        if (voidBloqueoVisual != null && !mostrandoVoid)
        {
            StartCoroutine(MostrarVoidTemporal());
        }
    }

    private void TeletransportarJugador(Transform jugador, Rigidbody2D rb, KitsunePortalState portalState)
    {
        if (destinoTeletransporte == null)
        {
            Debug.LogWarning("PortalMetaController: destinoTeletransporte no asignado.");
            return;
        }

        jugador.position = destinoTeletransporte.position;
        rb.linearVelocity = empujeSalida;
        portalState.BloquearPortales(cooldownPortalJugador);
    }

    private IEnumerator MostrarVoidTemporal()
    {
        mostrandoVoid = true;
        voidBloqueoVisual.SetActive(true);

        yield return new WaitForSeconds(duracionVoidVisual);

        if (voidBloqueoVisual != null)
            voidBloqueoVisual.SetActive(false);

        mostrandoVoid = false;
    }
}