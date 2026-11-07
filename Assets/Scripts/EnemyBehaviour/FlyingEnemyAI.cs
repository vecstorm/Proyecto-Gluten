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

    #region MAIN LOOP

    private void Awake()
    {
        InitializeBasicComponents();
        DecideMovementPoint();
    }

    private void Start()
    {
        state = ENEMY_STATE.PATROL;
        defaultState = state;
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case ENEMY_STATE.IDLE:
                //OnIdle();
                break;

            case ENEMY_STATE.PATROL:
                OnPatrol();
                break;

            case ENEMY_STATE.CHASING:
                //OnChasing();
                break;

            case ENEMY_STATE.ATTACK:
                //OnAttack();
                break;

            case ENEMY_STATE.SEARCHING:
                //OnSearching();
                break;

            case ENEMY_STATE.STUNNED:
                //OnStunned();
                break;

            case ENEMY_STATE.DEAD:
                break;
        }
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

    #endregion

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
