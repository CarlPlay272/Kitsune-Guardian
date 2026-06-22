using UnityEngine;

public class TenguDeathHook : MonoBehaviour
{
    private OniBoss boss;

    public void Init(OniBoss b)
    {
        boss = b;
    }

    public void Die()
    {
        boss?.NotifyEnemyDeath();
        Destroy(gameObject);
    }
}