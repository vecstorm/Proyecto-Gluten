using System.Collections;
using TMPro;
using UnityEngine;
using static UnityEngine.LowLevelPhysics2D.PhysicsShape;

[RequireComponent (typeof(Rigidbody2D))]
public class FinalBossHand : MonoBehaviour
{
    [SerializeField] private Transform originPoint;
    [SerializeField] private float returningSpeed = 20f;
    [SerializeField] private float offsetSpeed = 3f;

    private float movementOffset;
    private float incomingOffset;

    private bool isBusy = false;

    private Rigidbody2D rb;

    private bool armCollided = false;
    private bool onOrigin = false;
    private bool colorFlashing = false;

    private CapsuleCollider2D damageCollider;

    private Vector3 spawnRotation;


    public bool ArmCollided => armCollided;

    public bool OnOrigin => onOrigin;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        spawnRotation = transform.rotation.eulerAngles;

        damageCollider = GetComponent<CapsuleCollider2D>();
    }

    private void FixedUpdate()
    {
        if (!isBusy)
        {
            movementOffset = Mathf.Lerp(movementOffset, incomingOffset, offsetSpeed * Time.fixedDeltaTime);

            Vector3 targetPosition = originPoint.position + Vector3.right * movementOffset;

            rb.MovePosition(Vector3.MoveTowards(transform.position, targetPosition, returningSpeed * Time.fixedDeltaTime));

            if (Vector3.Distance(transform.position, originPoint.position) <= 0.01f)
            {
                onOrigin = true;
            }
        }
    }

    public void ReturnToOrigin()
    {
        armCollided = false;
        onOrigin = false;
        isBusy = false;
    }

    public void ResetRotation()
    {
        transform.rotation = Quaternion.Euler(spawnRotation);
    }

    public void ResetHand()
    {
        ReturnToOrigin();
        ResetRotation();
    }

    public void MoveToPosition(Vector3 targetPosition, float speed)
    {
        isBusy = true;
        rb.MovePosition(Vector3.MoveTowards(transform.position, targetPosition, speed * Time.fixedDeltaTime));
    }

    public void MoveToDirection(Vector3 targetDirection, float speed)
    {
        isBusy = true;
        rb.linearVelocity = targetDirection * speed;
    }

    public void MoveTillCollision(Vector3 targetDirection, float speed)
    {
        if (armCollided)
        {
            Debug.Log("COLLIDED AND STOPPED");
            return;
        }

        isBusy = true;

        rb.linearVelocity = targetDirection * speed;
    }

    public void SetColliderActive(bool value)
    {
        damageCollider.enabled = value;
    }

    public void SetMovementOffset(float offset)
    {
        incomingOffset = offset;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Ground") && isBusy)
        {
            ContactPoint2D contact = collision.GetContact(0);

            if (Mathf.Abs(contact.normal.y) > 0.5f)
            {
                Debug.Log("Arm: " + gameObject.name + " has collided with: " + collision.gameObject.name);
                armCollided = true;
            }
        }
    }
}
