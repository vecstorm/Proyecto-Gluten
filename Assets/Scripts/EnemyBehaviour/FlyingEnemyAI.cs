using System.Collections;
using UnityEditor;
using UnityEngine;

public class FlyingEnemyAI : EnemyAI
{
    [Header("Flying Parameters:")]
    [SerializeField] private float personalBubbleRadius = 2f;
    [SerializeField] private float transitableAreaRadius = 6f;
    [SerializeField] private float minFloorDistance = 3f;
    [SerializeField] private float maxFloorDistance = 7f;

    [Header("Flying Movement:")]
    [SerializeField] private float minMovementDelay = 3f;
    [SerializeField] private float maxMovementDelay = 7f;

    private bool decidingDirection = false;
    private Vector3 movementPoint;
    private float speed = 3f;

    private void Awake()
    {
        characterStats = GetComponent<CharacterStats>();
        DecideMovementPoint();
    }

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

    private void OnPatrol()
    {
        rb.MovePosition(Vector3.MoveTowards(transform.position, movementPoint, speed * Time.fixedDeltaTime));

        if (Vector3.Distance(transform.position, movementPoint) < 0.1f)
        {
            if (!decidingDirection) StartCoroutine(WaitToMove());
        }
    }

    private IEnumerator WaitToMove()
    {
        decidingDirection = true;

        yield return new WaitForSeconds(Random.Range(minMovementDelay, maxMovementDelay));

        DecideMovementPoint();
        decidingDirection = false;
    }

    private void DecideMovementPoint()
    {
        Vector3 candidatePoint = Vector3.zero;

        bool validPoint = false;

        while (!validPoint)
        {
            candidatePoint = transform.position + (Vector3)Random.insideUnitCircle * transitableAreaRadius;
            Collider2D areaCheck = Physics2D.OverlapCircle(candidatePoint, personalBubbleRadius);

            if (areaCheck == null)
            {
                validPoint = true;
            }
        }

        movementPoint = candidatePoint;
    }

    private float RandomAngle()
    {
        return Random.Range(0f, 360f);
    }
}
