using UnityEngine;

public class FloatingIceberg : MonoBehaviour
{
    public float floatStrength = 0.5f;
    public float floatSpeed = 2f;

    private Vector3 originalPos;

    void Start()
    {
        originalPos = transform.position;
    }

    void Update()
    {
        transform.position = originalPos + Vector3.up * Mathf.Sin(Time.time * floatSpeed) * floatStrength;
    }
}
