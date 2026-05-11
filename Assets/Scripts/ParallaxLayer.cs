using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private Transform cam;
    [SerializeField] private float parallaxFactor = 0.2f;

    private float startX;
    private float startCamX;

    void Start()
    {
        startX = transform.position.x;
        startCamX = cam.position.x;
    }

    void LateUpdate()
    {
        float camDelta = cam.position.x - startCamX;
        transform.position = new Vector3(startX + camDelta * parallaxFactor, transform.position.y, transform.position.z);
    }
}