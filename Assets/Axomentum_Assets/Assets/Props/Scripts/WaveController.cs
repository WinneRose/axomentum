using UnityEngine;
using System.Collections;

public class WaveController : MonoBehaviour
{
    public Transform iceBlockTransform;
    public float speed = 7.0f;
    public float startOffsetBehindBlock = 15.0f;
    public float offsetAfterBlock = 20.0f;
    public string iceBlockTag = "IceBlock";
    public float waitTime = 4.0f;

    private SpriteRenderer spriteRenderer;
    private Collider2D waveCollider;
    private float initialY;
    private bool isWaiting = false;
    private bool hasHitBlock = false;
    private float dynamicEndX = float.MaxValue;
    private bool waveCycleStarted = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        waveCollider = GetComponent<Collider2D>();
        if (spriteRenderer == null) Debug.LogError("WaveController: SpriteRenderer bulunamadý!", gameObject);
        if (waveCollider == null) Debug.LogError("WaveController: Collider2D bulunamadý!", gameObject);
    }

    void Start()
    {
        initialY = transform.position.y;
        SetWaveState(false);
        isWaiting = true;
    }

    void Update()
    {
        if (!waveCycleStarted || isWaiting || iceBlockTransform == null)
        {
            return;
        }

        transform.Translate(Vector2.right * speed * Time.deltaTime);

        if (hasHitBlock)
        {
            if (transform.position.x >= dynamicEndX)
            {
                StartCoroutine(RecycleWave());
            }
        }
    }

    public void StartWaveCycle()
    {
        if (waveCycleStarted) return;

        Debug.Log("Wave Cycle Starting!");
        waveCycleStarted = true;
        isWaiting = false;
        ResetWaveToStart();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isWaiting && !hasHitBlock && other.CompareTag(iceBlockTag))
        {
            hasHitBlock = true;
            dynamicEndX = transform.position.x + offsetAfterBlock;
        }
    }

    void SetWaveState(bool active)
    {
        if (spriteRenderer != null) spriteRenderer.enabled = active;
        if (waveCollider != null) waveCollider.enabled = active;
    }

    void ResetWaveToStart()
    {
        float targetStartX;
        if (iceBlockTransform != null)
        {
            float blockCurrentX = iceBlockTransform.position.x;
            targetStartX = blockCurrentX - startOffsetBehindBlock;
        }
        else
        {
            targetStartX = transform.position.x;
        }

        hasHitBlock = false;
        dynamicEndX = float.MaxValue;
        transform.position = new Vector3(targetStartX, initialY, transform.position.z);
        SetWaveState(true);
        isWaiting = false;
    }

    IEnumerator RecycleWave()
    {
        isWaiting = true;
        SetWaveState(false);
        yield return new WaitForSeconds(waitTime);
        ResetWaveToStart();
    }
}