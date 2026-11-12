using UnityEngine;

public class ProjectileEnemyAI : EnemyAI
{
    [Header("Target Detection:")]
    [SerializeField] private float areaRadius = 13f;

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
    }

    private void OnChasing()
    {

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

        if (PlayerBehind() != invertedDirection)
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
        state = ENEMY_STATE.CHASING;
    }

    private void ToAttack()
    {
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
            if (collider.CompareTag("Player") && !ObstacleBlocksTarget(collider.transform.position, areaRadius))
            {
                lastTargetPos = collider.transform.position;
                targetTransform = collider.transform;

                return true;
            }
        }

        return false;
    }

    // Devuelve si el jugador esta detras del enemigo o no
    private bool PlayerBehind()
    {
        float deltaX = targetTransform.position.x - transform.position.x;

        return deltaX < 0;
    }

    public override void ResetEnemy()
    {
        throw new System.NotImplementedException();
    }
}
