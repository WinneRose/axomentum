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
    [SerializeField] private Transform endLine;

    [Header("Wave Settings")]
    public WaveController[] waveControllerReference;
    public Vector2 waveRandomOffsetRange = new Vector2(0.3f, 0.2f); // X and Y range
    public float waveSpacing = 1.2f; // distance between waves

    [SerializeField] private bool startActivated = false;
    private bool isActivated = false;
    private bool isShaking = false;
    private Quaternion initialRotationDuringEffect;

    void Start()
    {
        isActivated = startActivated;
        if (isActivated && waveControllerReference != null)
        {
            RandomizeAndStartWaves();
        }
    }

    void Update()
    {
        if (!isActivated) return;

        // Check if we reached or passed the endLine
        if (endLine != null && transform.position.x >= endLine.position.x)
        {
            isActivated = false;
            Debug.Log("IceBlock reached end line.");

            // Stop all active wave cycles
            foreach (var wave in waveControllerReference)
            {
                if (wave != null)
                    wave.StopWaveCycle();
            }

            return;
        }


        // Move the ice block if not reached
        transform.Translate(Vector3.right * moveSpeed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isActivated && other.CompareTag(waveTag) && !isShaking)
        {
            initialRotationDuringEffect = transform.localRotation;
            StartCoroutine(ShakeAndRiseCoroutine());
        }
    }

    public void ActivateBlock()
    {
        if (isActivated) return;

        Debug.Log("IceBlock Activated!");
        isActivated = true;

        if (waveControllerReference != null && waveControllerReference.Length > 0)
        {
            StartCoroutine(DelayedStartWaveCycle(3.0f));
        }
    }

    private IEnumerator DelayedStartWaveCycle(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (waveControllerReference != null)
        {
            RandomizeAndStartWaves();
        }
    }

    private void RandomizeAndStartWaves()
    {
        float startX = transform.position.x + 1f;
        float currentX = startX;
        float baseY = transform.position.y;

        for (int i = 0; i < waveControllerReference.Length; i++)
        {
            var wave = waveControllerReference[i];
            if (wave == null) continue;

            // Random gap between waves (makes spacing natural)
            float gap = Random.Range(1.0f, 6f);  // Increase this range for more variety
            float yOffset = Random.Range(-0.5f, 0.5f); // Vertical bobbing

            Vector3 newPosition = new Vector3(currentX, baseY + yOffset, 0);
            wave.transform.position = newPosition;

            float delay = Random.Range(0.05f, 0.3f);
            StartCoroutine(DelayedWaveStart(wave, delay));

            currentX += gap; // Increase X for next wave
        }
    }


    private IEnumerator DelayedWaveStart(WaveController wave, float delay)
    {
        yield return new WaitForSeconds(delay);
        wave.StartWaveCycle();
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
