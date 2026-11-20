using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ProjectileEnemyAI : TroopAI
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
    [SerializeField] private int projectileDamage = 10;
    [SerializeField] private int chargedProjectileDamage = 20;
    [SerializeField] private float projectileSpeed = 6;
    [SerializeField] private int ampedProjectileCount = 4;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Delays:")]
    [SerializeField] private float minSearchingDelay = 3f;
    [SerializeField] private float maxSearchingDelay = 5f;
    [SerializeField] private float newSpawnPositionDelay = 3f;

    private Vector3 lastTargetPos;
    private float lastSpawnDistance;

    private Vector3 dynamicSpawnPosition;

    private int projectileCount;

    private bool spawnInvertedDirection;

    private float timer;

    #region MAIN LOOP

    private void Awake()
    {
        InitializeBasicComponents();
    }

    private void Start()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation.eulerAngles;
        spawnInvertedDirection = invertedDirection;
    }

    private void OnEnable()
    {
        dynamicSpawnPosition = spawnPosition;
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

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(dynamicSpawnPosition, 0.1f);
    }

    #endregion

    #region STATE BEHAVIOUR

    // ========================================================= FUNCIONES DE COMPORTAMIENTO DE ESTADO ========================================================= //

    // Comportamiento en estado PATROL
    private void OnPatrol()
    {
        if (TargetDetectedOnArea(areaRadius))
        {
            ToChasing();
            return;
        }

        if (Vector3.Distance(transform.position, dynamicSpawnPosition) >= 0.1f)
        {
            Debug.Log("Moving to new Dynamic Spawn Point");

            if (TargetBehind(dynamicSpawnPosition))
            {
                Flip();
            }

            Vector3 movementDirection = (dynamicSpawnPosition - transform.position).normalized;

            localMovement = new Vector3(movementDirection.x * speed, rb.linearVelocity.y);

            if (!DetectedEdge(edgeCheck.position))
            {
                localMovement = new Vector3(0, rb.linearVelocity.y);
            }

            float currentSpawnDistance = Vector3.Distance(transform.position, dynamicSpawnPosition);

            if (currentSpawnDistance < lastSpawnDistance - 0.05f)
            {
                timer = newSpawnPositionDelay;
            }
            else
            {
                timer -= Time.fixedDeltaTime;
            }

            if (timer <= 0)
            {
                dynamicSpawnPosition = transform.position;

                timer = newSpawnPositionDelay;
            }

            lastSpawnDistance = currentSpawnDistance;
        }
        else
        {
            if (invertedDirection != spawnInvertedDirection)
            {
                Flip();
            }

            localMovement = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    // Comportamiento en estado CHASING
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

    // Comportamiento en estado ATTACK
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

            EnemyProjectileController projectileController = newProjectile.GetComponent<EnemyProjectileController>();
            projectileController.SetMovement(projectileSpeed, targetTransform.position);
            projectileController.SetContactDamage(projectileDamage);

            if (projectileCount >= ampedProjectileCount)
            {
                projectileCount = 0;
                newProjectile.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                projectileController.SetContactDamage(chargedProjectileDamage);
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

    // Comportamiento en estado SEARCHING
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

        timer -= Time.fixedDeltaTime;

        if (timer <= 0)
        {
            ToPatrol();
        }
    }

    #endregion

    #region STATE TRANSITION

    // ========================================================= FUNCIONES DE TRANSICIÓN DE ESTADO ========================================================= //

    // Comportamiento de transición a PATROL
    private void ToPatrol()
    {
        Debug.Log("Changed to PATROL");

        timer = newSpawnPositionDelay;

        lastSpawnDistance = Vector3.Distance(transform.position, dynamicSpawnPosition);

        localMovement = new Vector3(0, rb.linearVelocity.y, 0);

        state = ENEMY_STATE.PATROL;
    }

    // Comportamiento de transición a CHASING
    private void ToChasing()
    {
        Debug.Log("Changed to CHASING");

        projectileCount--;

        if (projectileCount <= 0) projectileCount = 0;

        localMovement = new Vector3(0, rb.linearVelocity.y, 0);

        state = ENEMY_STATE.CHASING;
    }

    // Comportamiento de transición a ATTACK
    private void ToAttack()
    {
        Debug.Log("Changed to ATTACK");

        timer = attackCooldown;

        localMovement = new Vector3(0, rb.linearVelocity.y, 0);

        state = ENEMY_STATE.ATTACK;
    }

    // Comportamiento de transición a SEARCHING
    private void ToSearching()
    {
        Debug.Log("Changed to SEARCHING");

        localMovement = new Vector3(0, rb.linearVelocity.y, 0);

        timer = Random.Range(minSearchingDelay, maxSearchingDelay);

        state = ENEMY_STATE.SEARCHING;
    }

    // Comportamiento de transición a DEAD
    protected void ToDead()
    {
        isDead = true;
        state = ENEMY_STATE.DEAD;
        gameObject.SetActive(false);
    }

    #endregion

    #region STATE CONDITION

    // ========================================================= FUNCIONES DE CONDICIÓN DE ESTADO ========================================================= //

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

    #endregion

    #region UTILITIES

    // ========================================================= FUNCIONES DE UTILIDAD ========================================================= //

    // Si colisiona con el player, le inflinge daño
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

    // Logica de reseteo del enemigo
    public override void ResetEnemy()
    {
        if (!canRespawn) return;

        gameObject.SetActive(true);
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

    #endregion
}
