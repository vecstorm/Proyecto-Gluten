using UnityEngine;

public class ProjectileEnemyAI : EnemyAI
{
    [Header("Target Detection:")]
    [SerializeField] private float areaRadius = 13f;

    [Header("Movement:")]
    [SerializeField] private float speed = 2;
    [SerializeField] private float minChasingDelay = 3f;
    [SerializeField] private float maxChasingDelay = 5f;

    [Header("Edge Detection:")]
    [SerializeField] private Transform edgeCheck;

    [Header("Attack:")]
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 6;
    [SerializeField] private int ampedProjectileCount = 4;
    [SerializeField] private float attackCooldown = 1.5f;

    private Transform targetTransform;
    private Vector3 lastTargetPos;

    private int projectileCount;

    private float timer;

    private void Awake()
    {
        InitializeBasicComponents();
    }

    private void Start()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation.eulerAngles;
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case ENEMY_STATE.PATROL:
                OnPatrol();
                break;

            case ENEMY_STATE.CHASING:
                OnChasing();
                break;

            case ENEMY_STATE.ATTACK:
                OnAttack();
                break;

            case ENEMY_STATE.RAMMING:
                //OnRamming();
                break;

            case ENEMY_STATE.SEARCHING:
                //OnSearching();
                break;

            case ENEMY_STATE.RETREAT:
                //OnRetreat();
                break;

            case ENEMY_STATE.DEAD:
                break;
        }

        externalMovement = Vector3.Lerp(externalMovement, Vector3.zero, externalMovementDecay * Time.fixedDeltaTime);

        rb.linearVelocity = localMovement + externalMovement;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, areaRadius);
    }

    private void OnPatrol()
    {
        if (TargetDetectedOnArea(areaRadius))
        {
            ToAttack();
            return;
        }

        if (Vector3.Distance(transform.position, spawnPosition) >= 0.1f)
        {
            if (TargetBehind(spawnPosition))
            {
                Flip();
            }
            
            localMovement = (spawnPosition - transform.position).normalized * speed;
        }
        else
        {
            localMovement = Vector3.zero;
        }
    }

    private void OnChasing()
    {
        if (TargetDetectedOnArea(areaRadius))
        {
            ToAttack();
            return;
        }

        timer -= Time.fixedDeltaTime;

        if (TargetBehind(targetTransform.position)) Flip();

        localMovement = new Vector3(GetTargetDirection().x * speed, rb.linearVelocity.y);

        if (!DetectedEdge())
        {
            //rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            localMovement = new Vector3(0, rb.linearVelocity.y, 0);
        }

        if (timer <= 0)
        {
            ToPatrol();
        }
    }

    private void OnAttack()
    {
        if (!TargetDetectedOnArea(areaRadius))
        {
            ToChasing();
            return;
        }

        timer -= Time.fixedDeltaTime;

        lastTargetPos = targetTransform.position;

        if (timer <= 0)
        {
            projectileCount++;

            GameObject newProjectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            newProjectile.GetComponent<EnemyProjectileController>().SetMovement(projectileSpeed, targetTransform.position);

            if (projectileCount >= ampedProjectileCount)
            {
                projectileCount = 0;
                newProjectile.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            }
            
            timer = attackCooldown;
        }

        if (TargetBehind(targetTransform.position))
        {
            Flip();
        }
    }

    private void ToPatrol()
    {
        state = ENEMY_STATE.PATROL;
    }

    private void ToChasing()
    {
        projectileCount--;

        timer = Random.Range(minChasingDelay, maxChasingDelay);

        state = ENEMY_STATE.CHASING;
    }

    private void ToAttack()
    {
        localMovement = Vector3.zero;
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

    // Devuelve la direccion hacia el objetivo
    private Vector3 GetTargetDirection()
    {
        Vector3 targetDirection = Vector3.zero;

        if (targetTransform != null)
        {
            targetDirection = targetTransform.position - transform.position;
        }

        return targetDirection.normalized;
    }

    // Devuelve si ha detectado al objetivo dentro de un area determinada
    private bool TargetDetectedOnArea(float radius)
    {
        Collider2D[] detectionArea = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D collider in detectionArea)
        {
            if (collider.CompareTag("Player") && !ObstacleBlocksTarget(collider.transform.position, areaRadius))
            {
                lastTargetPos = collider.transform.position;
                targetTransform = collider.transform;

                return true;
            }
        }

        return false;
    }

    // Devuelve si esta detectando suelo delante suyo
    private bool DetectedEdge()
    {
        Debug.DrawRay(edgeCheck.position, Vector3.down, Color.red);
        return Physics2D.Raycast(edgeCheck.position, Vector3.down, 1f, groundLayer);
    }

    public override void ResetEnemy()
    {
        throw new System.NotImplementedException();
    }
}
