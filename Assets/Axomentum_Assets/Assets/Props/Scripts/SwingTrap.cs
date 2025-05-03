using UnityEngine;

public class SwingTrap : MonoBehaviour
{
    [SerializeField] private Transform hinge; // The trap GameObject with Rigidbody2D and HingeJoint2D
    [SerializeField] private BoxCollider2D triggerCollider;
    [SerializeField] private float swingAngle = 90f;
    [SerializeField] private float swingDuration = 0.5f;
    [SerializeField] private bool reverse = true;

    private bool hasSwung = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasSwung)
        {
            StartCoroutine(SwingOnce());
            hasSwung = true;
            triggerCollider.enabled = false;
        }
    }

    private System.Collections.IEnumerator SwingOnce()
    {
        float timer = 0f;
        float startAngle = hinge.eulerAngles.z;
        float endAngle = startAngle + swingAngle;

        while (timer < swingDuration)
        {
            timer += Time.deltaTime;
            float angle = Mathf.Lerp(startAngle, endAngle, timer / swingDuration);
            hinge.rotation = Quaternion.Euler(0, 0, angle);
            yield return null;
            
        }

        if (reverse)
        {
            yield return new WaitForSeconds(0.2f);
            timer = 0f;
            while (timer < swingDuration)
            {
                timer += Time.deltaTime;
                float angle = Mathf.Lerp(endAngle, startAngle, timer / swingDuration);
                hinge.rotation = Quaternion.Euler(0, 0, angle);
                yield return null;
            }
        }
    }
}