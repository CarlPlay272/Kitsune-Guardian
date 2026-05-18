using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private Transform cam;
    [Range(0f, 1f)]
    [SerializeField] private float parallaxFactorX = 0.2f;

    private float startX;
    private float startCamX;

    void Start()
    {
        if (cam == null)
            cam = Camera.main.transform;

        startX = transform.position.x;
        startCamX = cam.position.x;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        float camDeltaX = cam.position.x - startCamX;
        float newX = startX + camDeltaX * parallaxFactorX;

        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }
}