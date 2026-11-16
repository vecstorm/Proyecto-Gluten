using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
public class FinalBossHand : MonoBehaviour
{
    private bool isBusy = false;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void MoveToPosition(Vector3 targetPosition, float speed)
    {
        if (Vector3.Distance(transform.position, targetPosition) <= 0.1f)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        rb.linearVelocity = (targetPosition - transform.position).normalized * speed;
    }
}
