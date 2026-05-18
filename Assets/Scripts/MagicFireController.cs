using UnityEngine;

public class MagicFireController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform tengu;
    [SerializeField] private GameObject purpleVisual;
    [SerializeField] private GameObject blueVisual;

    [Header("Rango de corrupción")]
    [SerializeField] private float corruptionRadius = 6f;

    [Header("Estado")]
    [SerializeField] private bool forceBlueIfNoTengu = true;

    private TenguState tenguState;

    void Start()
    {
        if (tengu != null)
        {
            tenguState = tengu.GetComponent<TenguState>();
        }

        UpdateFireState();
    }

    void Update()
    {
        UpdateFireState();
    }

    void UpdateFireState()
    {
        if (tengu == null)
        {
            if (forceBlueIfNoTengu)
                SetBlue();
            else
                SetPurple();
            return;
        }

        if (tenguState != null && tenguState.IsDead)
        {
            SetBlue();
            return;
        }

        float distance = Vector2.Distance(transform.position, tengu.position);

        if (distance <= corruptionRadius)
            SetPurple();
        else
            SetBlue();
    }

    void SetPurple()
    {
        if (purpleVisual != null) purpleVisual.SetActive(true);
        if (blueVisual != null) blueVisual.SetActive(false);
    }

    void SetBlue()
    {
        if (purpleVisual != null) purpleVisual.SetActive(false);
        if (blueVisual != null) blueVisual.SetActive(true);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, corruptionRadius);
    }
}