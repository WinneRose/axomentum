using UnityEngine;
using System.Collections;

public class IceBlockController : MonoBehaviour
{
    public float moveSpeed = 2.0f;
    public float shakeDuration = 0.8f;
    public float shakeMagnitude = 0.1f;
    public float rotationMagnitude = 2.0f;
    public float rotationSpeed = 50f;
    public float riseHeight = 0.5f;
    public string waveTag = "Wave";

    private bool isShaking = false;
    private Quaternion initialRotationDuringEffect;

    void Update()
    {
        transform.Translate(Vector3.right * moveSpeed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(waveTag) && !isShaking)
        {
            initialRotationDuringEffect = transform.localRotation;
            StartCoroutine(ShakeAndRiseCoroutine());
        }
    }

    IEnumerator ShakeAndRiseCoroutine()
    {
        isShaking = true;
        float elapsed = 0.0f;
        Vector3 originalPosition = transform.localPosition;

        while (elapsed < shakeDuration)
        {
            float dt = Time.deltaTime;
            float progress = elapsed / shakeDuration;

            float xOffset = Random.Range(-1f, 1f) * shakeMagnitude;
            float yOffset = Random.Range(-1f, 1f) * shakeMagnitude;
            Vector3 shakeOffset = new Vector3(xOffset, yOffset, 0);

            float verticalRise = Mathf.Sin(progress * Mathf.PI) * riseHeight;
            Vector3 riseOffset = new Vector3(0, verticalRise, 0);

            float zRotation = Mathf.Sin(Time.time * rotationSpeed) * rotationMagnitude;

            Vector3 expectedPositionWithoutEffect = originalPosition + (Vector3.right * moveSpeed * elapsed);

            transform.localPosition = expectedPositionWithoutEffect + shakeOffset + riseOffset;
            transform.localRotation = initialRotationDuringEffect * Quaternion.Euler(0, 0, zRotation);

            elapsed += dt;
            yield return null;
        }

        Vector3 finalExpectedPosition = originalPosition + (Vector3.right * moveSpeed * shakeDuration);
        transform.localPosition = finalExpectedPosition;
        transform.localRotation = initialRotationDuringEffect;

        isShaking = false;
    }
}