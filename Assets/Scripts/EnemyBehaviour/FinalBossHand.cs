using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
public class FinalBossHand : MonoBehaviour
{
    [SerializeField] private Transform originPoint;

    private bool isBusy = false;

    private Rigidbody2D rb;

    private bool armCollided = false;
    private bool returning = false;
    private bool colorFlashing = false;

    private CapsuleCollider2D damageCollider;

    private Vector3 spawnRotation;

    private float returningSpeed = 20f;

    public bool ArmCollided => armCollided;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        spawnRotation = transform.rotation.eulerAngles;

        damageCollider = GetComponent<CapsuleCollider2D>();
    }

    private void FixedUpdate()
    {
        if (returning && !isBusy)
        {
            rb.MovePosition(Vector3.MoveTowards(transform.position, originPoint.position, returningSpeed * Time.fixedDeltaTime));

            if (Vector3.Distance(transform.position, originPoint.position) <= 0.01f)
            {
                returning = false;
            }
        }
    }

    public void ReturnToOrigin()
    {
        armCollided = false;
        returning = true;
        isBusy = false;
    }

    public void ResetRotation()
    {
        transform.rotation = Quaternion.Euler(spawnRotation);
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Ground"))
        {
            armCollided = true;
        }
    }
}
