using System.Collections;
using UnityEngine;

public class WaterZone : MonoBehaviour
{
    [Header("Daño al salir")]
    [SerializeField] private bool aplicarDanioAlSalir = true;
    [SerializeField] private float danioAlSalir = 5f;
    [SerializeField] private float retrasoDanioSalida = 0.25f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        KitsuneController kitsuneController = other.GetComponentInParent<KitsuneController>();

        if (kitsuneController != null)
        {
            kitsuneController.EnterWater();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        KitsuneController kitsuneController = other.GetComponentInParent<KitsuneController>();
        KitsuneHealth kitsuneHealth = other.GetComponentInParent<KitsuneHealth>();

        if (kitsuneController != null)
        {
            kitsuneController.ExitWater();
        }

        if (aplicarDanioAlSalir && kitsuneHealth != null && !kitsuneHealth.IsDead)
        {
            StartCoroutine(AplicarDanioSalida(kitsuneHealth));
        }
    }

    private IEnumerator AplicarDanioSalida(KitsuneHealth kitsuneHealth)
    {
        yield return new WaitForSeconds(retrasoDanioSalida);

        if (kitsuneHealth != null && !kitsuneHealth.IsDead)
        {
            kitsuneHealth.TakeDamage(danioAlSalir);
        }
    }
}