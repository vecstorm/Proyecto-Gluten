using UnityEngine;

public class RotateTowardsDirection : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        float angle = Mathf.Atan2(-rb.linearVelocity.y, -rb.linearVelocity.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
