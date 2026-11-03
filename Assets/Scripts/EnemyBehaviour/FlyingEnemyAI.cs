using System.Collections;
using UnityEditor;
using UnityEngine;

public class FlyingEnemyAI : EnemyAI
{
    [Header("Flying Parameters:")]
    [SerializeField] private float personalBubbleRadius = 2f;
    [SerializeField] private float transitableAreaRadius = 6f;

    private bool decidingDirection = false;
    private Vector3 movementPoint;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, personalBubbleRadius);

        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, transitableAreaRadius);

        if (movementPoint != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(movementPoint, 0.1f);
        }
    }

    protected override void OnPatrol()
    {
        if (!decidingDirection) StartCoroutine(WaitToMove());
        //Vector3 movementDirection = Quaternion.Euler(0f, 0f, RandomAngle()) * Vector3.right;

        //rb.linearVelocity = movementDirection.normalized * speed;
    }

    private IEnumerator WaitToMove()
    {
        decidingDirection = true;

        yield return new WaitForSeconds(3f);

        DecideMovementPoint();
        decidingDirection = false;
    }

    private void DecideMovementPoint()
    {
        Vector3 candidatePoint = transform.position + (Vector3)Random.insideUnitCircle * transitableAreaRadius;

        Collider2D areaCheck = Physics2D.OverlapCircle(candidatePoint, personalBubbleRadius);

        if (areaCheck != null)
        {
            
        }

        movementPoint = candidatePoint;
    }

    private float RandomAngle()
    {
        return Random.Range(0f, 360f);
    }
}
