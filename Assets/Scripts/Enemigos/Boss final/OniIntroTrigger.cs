using UnityEngine;

public class OniIntroTrigger : MonoBehaviour
{
    [SerializeField] private OniIntroCinematicController cinematic;
    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        if (!other.CompareTag("Player")) return;

        triggered = true;

        cinematic.PlayCinematic();
    }
}