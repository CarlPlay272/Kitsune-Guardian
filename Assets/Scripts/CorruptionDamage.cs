using UnityEngine;

public class CorruptionDamage : MonoBehaviour
{
    [Header("Daño por corrupción")]
    [SerializeField] private float damagePerTick = 10f;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private bool damageOnEnter = true;

    private float nextDamageTime = 0f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Algo entró al trigger de corrupción: " + other.name);

        KitsuneHealth kitsuneHealth = other.GetComponentInParent<KitsuneHealth>();

        if (kitsuneHealth == null) return;
        if (kitsuneHealth.IsDead) return;

        Debug.Log("Kitsune entró en corrupción: " + gameObject.name);

        if (damageOnEnter)
        {
            kitsuneHealth.TakeDamage(damagePerTick);
            nextDamageTime = Time.time + damageInterval;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        KitsuneHealth kitsuneHealth = other.GetComponentInParent<KitsuneHealth>();

        if (kitsuneHealth == null) return;
        if (kitsuneHealth.IsDead) return;

        if (Time.time >= nextDamageTime)
        {
            Debug.Log("Corrupción dañando a Kitsune: " + gameObject.name);
            kitsuneHealth.TakeDamage(damagePerTick);
            nextDamageTime = Time.time + damageInterval;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<KitsuneHealth>() != null)
        {
            Debug.Log("Kitsune salió de corrupción: " + gameObject.name);
            nextDamageTime = 0f;
        }
    }
}