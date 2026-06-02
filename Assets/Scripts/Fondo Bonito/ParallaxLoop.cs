using UnityEngine;

public class ParallaxLoop : MonoBehaviour
{
    [SerializeField] private Transform cam;

    [Range(0f, 1f)]
    [SerializeField] private float parallaxFactor = 0.5f;

    private float startCamX;

    private Transform[] layers;

    private float spriteWidth;

    void Start()
    {
        if (cam == null)
            cam = Camera.main.transform;

        startCamX = cam.position.x;

        layers = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            layers[i] = transform.GetChild(i);
        }

        spriteWidth = layers[0]
            .GetComponent<SpriteRenderer>()
            .bounds.size.x;
    }

    void LateUpdate()
    {
        // Movimiento parallax CORRECTO
        float camDelta =
            (startCamX - cam.position.x)
            * parallaxFactor;

        transform.position = new Vector3(
            camDelta,
            transform.position.y,
            transform.position.z
        );

        // Loop infinito
        foreach (Transform layer in layers)
        {
            float distance =
                cam.position.x - layer.position.x;

            if (distance >= spriteWidth)
            {
                layer.position +=
                    Vector3.right * spriteWidth * 2f;
            }
            else if (distance <= -spriteWidth)
            {
                layer.position +=
                    Vector3.left * spriteWidth * 2f;
            }
        }
    }
}