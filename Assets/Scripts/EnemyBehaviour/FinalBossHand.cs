using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
public class FinalBossHand : MonoBehaviour
{
    private bool isBusy = false;

    private Rigidbody2D rb;

    private bool armCollided = false;

    public bool ArmCollided => armCollided;

    private Vector3 originPosition;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        originPosition = transform.position;
    }
    
    public void MoveToPosition(Vector3 targetPosition, float speed)
    {
        rb.MovePosition(Vector3.MoveTowards(transform.position, targetPosition, speed * Time.fixedDeltaTime));
    }

    public void MoveToDirection(Vector3 targetDirection, float speed)
    {
        rb.linearVelocity = targetDirection * speed;
    }

    public void MoveTillCollision(Vector3 targetDirection, float speed)
    {
        if (armCollided)
        {
            Debug.Log("COLLIDED AND STOPPED");
            return;
        }

        rb.linearVelocity = targetDirection * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Ground"))
        {
            armCollided = true;
        }
    }
}
