using UnityEngine;

public class ProjectileEnemyAI : EnemyAI
{
    [Header("Target Detection:")]
    [SerializeField] private float personalBubbleRadius = 7f;
    [SerializeField] private float areaRadius = 15f;
    [SerializeField] private float attackAreaRadius = 12f;

    [Header("Movement:")]
    [SerializeField] private float speed = 2;

    [Header("Edge Detection:")]
    [SerializeField] private Transform edgeCheck;
    [SerializeField] private Transform backEdgeCheck;

    [Header("Attack:")]
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 6;
    [SerializeField] private int ampedProjectileCount = 4;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Delays:")]
    [SerializeField] private float minSearchingDelay = 3f;
    [SerializeField] private float maxSearchingDelay = 5f;

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

    private void OnEnable()
    {
        characterStats.OnDeath += ToDead;
        characterStats.OnDamageTaken += FlashColor;
        characterStats.OnDamageTaken += AddKnockback;
    }

    private void OnDisable()
    {
        characterStats.OnDeath -= ToDead;
        characterStats.OnDamageTaken -= FlashColor;
        characterStats.OnDamageTaken -= AddKnockback;
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

            case ENEMY_STATE.SEARCHING:
                OnSearching();
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

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackAreaRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, personalBubbleRadius);
    }

    private void OnPatrol()
    {
        if (TargetDetectedOnArea(areaRadius))
        {
            ToChasing();
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
            if (spawnRotation != transform.rotation.eulerAngles)
            {
                Flip();
            }

            localMovement = Vector3.zero;
        }
    }

    private void OnChasing()
    {
        if (!TargetDetectedOnArea(areaRadius))
        {
            ToSearching();
            return;
        }

        if (TargetDetectedOnArea(attackAreaRadius))
        {
            ToAttack();
            return;
        }

        lastTargetPos = targetTransform.position;

        if (TargetBehind(targetTransform.position)) Flip();

        localMovement = new Vector3(GetTargetDirection().x * speed, rb.linearVelocity.y);

        if (!DetectedEdge(edgeCheck.position))
        {
            localMovement = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    private void OnAttack()
    {
        if (!TargetDetectedOnArea(attackAreaRadius))
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

        localMovement = new Vector3(0, rb.linearVelocity.y, 0);

        if (TargetDetectedOnArea(personalBubbleRadius))
        {
            Vector3 movementDirection = (transform.position - targetTransform.position).normalized;
            localMovement = new Vector3(movementDirection.x * speed, rb.linearVelocity.y, 0);

            if (!DetectedEdge(backEdgeCheck.position))
            {
                localMovement = new Vector3(0, rb.linearVelocity.y, 0);
            }
        }

        if (TargetBehind(targetTransform.position))
        {
            Flip();
        }
    }

    private void OnSearching()
    {
        if (TargetDetectedOnArea(areaRadius))
        {
            ToChasing();
            return;
        }

        Vector3 movementPosition = new Vector3(lastTargetPos.x, transform.position.y, transform.position.z);

        if (!DetectedEdge(edgeCheck.position))
        {
            movementPosition = transform.position;
        }

        if (Vector3.Distance(transform.position, movementPosition) < 0.1f)
        {
            timer -= Time.fixedDeltaTime;

            if (timer <= 0)
            {
                ToPatrol();
            }
        }
    }

    private void ToPatrol()
    {
        Debug.Log("Changed to PATROL");

        localMovement = new Vector3(0, rb.linearVelocity.y, 0);

        state = ENEMY_STATE.PATROL;
    }

    private void ToChasing()
    {
        Debug.Log("Changed to CHASING");

        projectileCount--;

        if (projectileCount <= 0) projectileCount = 0;

        localMovement = new Vector3(0, rb.linearVelocity.y, 0);

        state = ENEMY_STATE.CHASING;
    }

    private void ToAttack()
    {
        Debug.Log("Changed to ATTACK");

        timer = attackCooldown;

        localMovement = new Vector3(0, rb.linearVelocity.y, 0);

        state = ENEMY_STATE.ATTACK;
    }

    private void ToSearching()
    {
        Debug.Log("Changed to SEARCHING");

        localMovement = new Vector3(0, rb.linearVelocity.y, 0);

        timer = Random.Range(minSearchingDelay, maxSearchingDelay);

        state = ENEMY_STATE.SEARCHING;
    }

    protected void ToDead()
    {
        isDead = true;
        state = ENEMY_STATE.DEAD;
        gameObject.SetActive(false);
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
            if (collider.CompareTag("Player") && !ObstacleBlocksTarget(collider.transform.position, radius))
            {
                lastTargetPos = collider.transform.position;
                targetTransform = collider.transform;

                return true;
            }
        }

        return false;
    }

    // Devuelve si esta detectando suelo delante suyo
    private bool DetectedEdge(Vector3 edgeCheck)
    {
        Debug.DrawRay(edgeCheck, Vector3.down, Color.red);
        return Physics2D.Raycast(edgeCheck, Vector3.down, 1f, groundLayer);
    }

    public override void ResetEnemy()
    {
        ResetColor();
        transform.position = spawnPosition;
        transform.rotation = Quaternion.Euler(spawnRotation);
        invertedDirection = transform.rotation == Quaternion.Euler(0, 180, 0);
        state = defaultState;
        isDead = false;

        localMovement = Vector3.zero;
        externalMovement = Vector3.zero;
        characterStats.FullHeal();
    }
}
