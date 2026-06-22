using UnityEngine;

public class Fireball : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 3f;

    [Header("Daño")]
    [SerializeField] private int damage = 1;

    private Vector2 moveDirection = Vector2.right;

    public void SetDirection(Vector2 dir)
    {
        moveDirection = dir.normalized;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(
            moveDirection *
            speed *
            Time.deltaTime,
            Space.World
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<BossGhostTrigger>() != null || other.GetComponent<Fireball>() != null)
        {
            return;
        }

        Debug.Log("Fuego golpeó a: " + other.name);

        // ===============================================================
        // DETECCIÓN DEL JEFE FINAL: ONI BOSS
        // ===============================================================
        OniBoss oni = other.GetComponent<OniBoss>() ?? other.GetComponentInParent<OniBoss>();
        if (oni != null)
        {
            oni.TakeDamage(damage); // Aplica el daño místico directo
            Destroy(gameObject);
            return;
        }

        // ===============================================================
        // DETECCIÓN EXTRA: PURIFICACIÓN DE BLOQUEOS DE VACÍO
        // ===============================================================
        CorruptionBlock bloqueCorrupto = other.GetComponent<CorruptionBlock>() ?? other.GetComponentInParent<CorruptionBlock>();
        if (bloqueCorrupto != null)
        {
            bloqueCorrupto.DestruirBloqueo();
            Destroy(gameObject);
            return;
        }

        TenguState tengu = other.GetComponent<TenguState>() ?? other.GetComponentInParent<TenguState>();
        if (tengu != null)
        {
            tengu.TakeHit(damage);
            Destroy(gameObject);
            return;
        }

        GhostHealth ghost = other.GetComponent<GhostHealth>() ?? other.GetComponentInParent<GhostHealth>();
        if (ghost != null)
        {
            ghost.TakeHit(damage);
            Destroy(gameObject);
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
            return;
        }
    }
}