using UnityEngine;

public class Fireball : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 3f;

    [Header("Daño")]
    [SerializeField] private int damage = 1;

    private int direction = 1;

    public void SetDirection(int dir)
    {
        direction = dir;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dir;
        transform.localScale = scale;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(
            Vector2.right *
            direction *
            speed *
            Time.deltaTime,
            Space.World
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Golpeó: " + other.name);

        TenguState tengu =
            other.GetComponent<TenguState>();

        if (tengu == null)
            tengu = other.GetComponentInParent<TenguState>();

        if (tengu != null)
        {
            Debug.Log("Tengu encontrado");

            tengu.TakeHit(damage);

            Destroy(gameObject);
            return;
        }
    }
}