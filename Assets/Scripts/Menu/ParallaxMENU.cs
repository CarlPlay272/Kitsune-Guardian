using UnityEngine;

public class ParallaxMENU : MonoBehaviour
{
    public float move_speed = 0.05f;

    void Update()
    {
        transform.position += Vector3.left * move_speed * Time.deltaTime;
    }
}