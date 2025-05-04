using UnityEngine;

public class FallingBlocks : MonoBehaviour
{
    [SerializeField] private GameObject[] fallingBlocks;
    [SerializeField] private float triggerRadius = 1f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float delayBeforeFall = 0.5f;

    private bool[] activatedBlocks;
    private float[] playerStayTimers;

    private void Start()
    {
        int length = fallingBlocks.Length;
        activatedBlocks = new bool[length];
        playerStayTimers = new float[length];

        for (int i = 0; i < length; i++)
        {
            Rigidbody2D rb = fallingBlocks[i].GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.simulated = true;
                rb.bodyType = RigidbodyType2D.Static;
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < fallingBlocks.Length; i++)
        {
            if (activatedBlocks[i]) continue;

            Vector2 blockPos = fallingBlocks[i].transform.position;
            Collider2D hit = Physics2D.OverlapCircle(blockPos, triggerRadius, playerLayer);

            if (hit != null && hit.CompareTag("Player"))
            {
                playerStayTimers[i] += Time.deltaTime;

                if (playerStayTimers[i] >= delayBeforeFall)
                {
                    Rigidbody2D rb = fallingBlocks[i].GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.bodyType = RigidbodyType2D.Dynamic;
                        activatedBlocks[i] = true;
                    }
                }
            }
            else
            {
                // Eğer oyuncu çıkarsa süre sıfırlansın
                playerStayTimers[i] = 0f;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (fallingBlocks == null) return;

        Gizmos.color = Color.cyan;
        foreach (var block in fallingBlocks)
        {
            if (block != null)
                Gizmos.DrawWireSphere(block.transform.position, triggerRadius);
        }
    }
}
