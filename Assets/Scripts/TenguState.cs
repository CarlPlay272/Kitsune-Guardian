using UnityEngine;

public class TenguState : MonoBehaviour
{
    [SerializeField] private bool isDead = false;

    public bool IsDead => isDead;

    public void MarkAsDead()
    {
        isDead = true;
    }
}