using System.Collections;
using UnityEditor;
using UnityEngine;

public class FlyingEnemyAI : EnemyAI
{
    [Header("Target Detection:")]
    [SerializeField] private float areaRadius = 10f;

    [Header("Attack:")]
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int projectileDamage = 10;
    [SerializeField] private float projectileSpeed = 6f;
    [SerializeField] private float attackCooldown = 1.5f;

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

    private Transform targetTransform;
    private Vector3 lastTargetPos;

    private float timer;

    private float speed = 3f;

    #region MAIN LOOP

    private void Awake()
    {
        InitializeBasicComponents();
        DecideMovementPoint();
    }

    private void Start()
    {
        //state = ENEMY_STATE.PATROL;
        //defaultState = state;
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
                OnAttack();
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
        if (!showGizmos) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, personalBubbleRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, transitableAreaRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, areaRadius);
    
        if (movementPoint != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(movementPoint, 0.1f);
        }
    }

    #endregion

    private void OnPatrol()
    {
        if (TargetDetectedOnArea(areaRadius)) ToAttack();

        //rb.MovePosition(Vector3.MoveTowards(transform.position, movementPoint, speed * Time.fixedDeltaTime));

        /*if (Vector3.Distance(transform.position, movementPoint) < 0.1f)
        {
            if (!decidingDirection) StartCoroutine(WaitToMove());
        }*/
    }

    private void OnAttack()
    {
        if (!TargetDetectedOnArea(areaRadius)) ToPatrol();

        timer += Time.fixedDeltaTime;

        if (timer >= attackCooldown)
        {
            Debug.Log("Projectile shot!");
            GameObject newProjectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            newProjectile.GetComponent<EnemyProjectileController>().SetMovement(projectileSpeed, targetTransform.position);

            timer = 0;
        }

        if (PlayerBehind() != invertedDirection)
        {
            Flip();
        }
    }

    private IEnumerator WaitToMove()
    {
        decidingDirection = true;

        yield return new WaitForSeconds(Random.Range(minMovementDelay, maxMovementDelay));

        DecideMovementPoint();
        decidingDirection = false;
    }

    private void ToPatrol()
    {
        ResetTimer();
        state = ENEMY_STATE.PATROL;
    }

    private void ToAttack()
    {
        ResetTimer();
        state = ENEMY_STATE.ATTACK;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Limpia los objetivos del ataque anterior
            hitTargets.Clear();

            CombatDamage damage = new CombatDamage(contactDamage, characterStats, false);
            CombatManager.MakeDamage(transform, collision, hitTargets, damage, characterStats.Team);
        }
    }

    // Devuelve si ha detectado al objetivo dentro de un area determinada
    private bool TargetDetectedOnArea(float radius)
    {
        Collider2D[] detectionArea = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D collider in detectionArea)
        {
            if (collider.CompareTag("Player"))
            {
                lastTargetPos = collider.transform.position;
                targetTransform = collider.transform;

                return true;
            }
        }

        return false;
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

    private bool PlayerBehind()
    {
        float deltaX = targetTransform.position.x - transform.position.x;

        return deltaX < 0;
    }

    private void ResetTimer()
    {
        timer = 0;
    }

    protected override void ResetEnemy()
    {
        throw new System.NotImplementedException();
    }
}
