using UnityEngine;

public class FireGlow : MonoBehaviour
{
    public float speed = 2f;
    public float scaleAmount = 0.1f;

    private Vector3 startScale;

    void Start()
    {
        startScale = transform.localScale;
    }

    void Update()
    {
        float scale =
            1 + Mathf.Sin(Time.time * speed) * scaleAmount;

        transform.localScale =
            startScale * scale;
    }
}