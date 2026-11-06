using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum ENEMY_STATE { IDLE, PATROL, CHASING, ATTACK, SEARCHING, DEAD };

    [Header("Debug:")]
    [SerializeField] private ENEMY_STATE state;
    [SerializeField] private ENEMY_STATE defaultState;
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private bool canRespawn = true;

    [Header("Movement:")]
    [SerializeField] protected float speed = 2f;
    [SerializeField] private float probabilityToTurn = 30f;

    [Header("Target Detection:")]
    [SerializeField] private float areaRadius = 3f;
    [SerializeField] private float detectedAreaRadius = 10f;
    private bool detectedAreaActive = true;
    private VisionCone detection;

    [Header("Ground Detection:")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Attack:")]
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackDetectionRange = 3f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackRadius = 1f;
    [SerializeField] private float attackDuration = 0.1f;
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("Delays:")]
    [SerializeField] private float minPatrolDelay = 0.5f;
    [SerializeField] private float maxPatrolDelay = 2f;
    [SerializeField] private float minChaseDelay = 0.2f;
    [SerializeField] private float maxChaseDelay = 0.7f;
    [SerializeField] private float minIdleDelay = 7f;
    [SerializeField] private float maxIdleDelay = 20f;

    private bool alreadyAttacking = false;
    private bool hitboxCreated = false;

    protected Rigidbody2D rb;
    protected bool invertedDirection = false;
    private Transform targetTransform;

    private bool waitingToPatrol = false;
    private bool waitingToChase = false;
    private bool waitingToDecideIdle = false;
    private bool isDead = false;

    private Vector3 lastTargetPos;
    private Vector3 spawnPosition;
    private Vector3 spawnRotation;

    private CharacterStats characterStats;

    //public CombatTeam Team => CombatTeam.Enemy;

    private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

    #region MAIN LOOP

    private void Awake()
    {
        characterStats = GetComponent<CharacterStats>();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        detection = GetComponent<VisionCone>();
        state = ENEMY_STATE.PATROL;
        defaultState = state;
        spawnPosition = transform.position;
        spawnRotation = transform.rotation.eulerAngles;
    }

    private void OnEnable()
    {
        invertedDirection = transform.rotation == Quaternion.Euler(0, 180, 0);
        characterStats.OnDeath += ToDead;

        if (isDead)
        {
            ResetEnemy();
        }
    }

    private void OnDisable()
    {
        characterStats.OnDeath -= ToDead;
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case ENEMY_STATE.IDLE:
                OnIdle();
                break;

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
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        if (hitboxCreated)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
            Gizmos.DrawSphere(transform.position + GetTargetDirection() * attackRange, attackRadius);

            Gizmos.color = Color.orange;
            Gizmos.DrawWireSphere(transform.position + GetTargetDirection() * attackRange, attackRadius);
        }

        if (detectedAreaActive)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, state == ENEMY_STATE.PATROL || state == ENEMY_STATE.IDLE ? areaRadius : detectedAreaRadius);
        }

        if (lastTargetPos != Vector3.zero)
        {
            Gizmos.color = Color.purple;
            Gizmos.DrawSphere(lastTargetPos, 0.1f);
        }
    }

    #endregion

    #region STATE BEHAVIOUR

    // ========================================================= FUNCIONES DE COMPORTAMIENTO DE ESTADO ========================================================= //

    // Comportamiento en estado IDLE
    private void OnIdle()
    {
        if (TargetDetected() || TargetDetectedOnArea(areaRadius)) ToChasing();

        ToPatrol();

        rb.linearVelocity = Vector3.zero;
    }

    // Comportamiento en estado PATROL
    protected virtual void OnPatrol()
    {
        if (TargetDetected() || TargetDetectedOnArea(areaRadius)) ToChasing();

        ToIdle();

        rb.linearVelocity = new Vector3((invertedDirection ? -1 : 1) * speed, rb.linearVelocity.y);

        if (!DetectedGround())
        {
            Flip();
        }
    }

    // Comportamiento en estado CHASING
    private void OnChasing()
    {
        if (!TargetDetectedOnArea(detectedAreaRadius)) ToSearching();
        if (targetTransform != null && OnAttackRange(targetTransform.position)) ToAttack();

        if (PlayerBehind() != invertedDirection) Flip();

        rb.linearVelocity = new Vector3(GetTargetDirection().x * (speed * 2f), rb.linearVelocity.y);

        if (!DetectedGround())
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    // Comportamiento en estado SEARCHING
    private void OnSearching()
    {
        if (TargetDetectedOnArea(detectedAreaRadius)) ToChasing();

        Vector3 movementPosition = new Vector3(lastTargetPos.x, transform.position.y, transform.position.z);
        
        if (!DetectedGround())
        {
            movementPosition = transform.position;
        }

        rb.MovePosition(Vector3.MoveTowards(transform.position, movementPosition, (speed * 2f) * Time.fixedDeltaTime));

        if (Vector3.Distance(transform.position, movementPosition) < 0.1f)
        {
            ToPatrol();
        }
    }

    // Comportamiento en estado ATTACK
    private void OnAttack()
    {
        if (TargetDetectedOnArea(detectedAreaRadius) && !OnAttackRange(targetTransform.position)) ToChasing();
        if (!TargetDetectedOnArea(detectedAreaRadius) && !OnAttackRange(targetTransform.position)) ToSearching();

        rb.linearVelocity = Vector3.zero;

        if (!alreadyAttacking)
        {
            StartCoroutine(AttackProcess());
        }

        if (hitboxCreated)
        {
            Vector3 colliderPosition = transform.position + GetTargetDirection() * attackRange;

            Collider2D[] hits = Physics2D.OverlapCircleAll(colliderPosition, attackRadius);

            CombatDamage damage = new CombatDamage(attackDamage, characterStats, colliderPosition, true);

            CombatManager.MakeDamage(transform, hits, hitTargets, damage, characterStats.Team);
        }
    }

    #endregion

    #region STATE TRANSITION

    // ========================================================= FUNCIONES DE TRANSICIÓN DE ESTADO ========================================================= //

    private void ToIdle()
    {
        if (!waitingToDecideIdle) StartCoroutine(WaitToIdle());
    }

    // Comportamiento de transición a CHASING
    private void ToChasing()
    {
        if (!waitingToChase) StartCoroutine(WaitToChase());
    }

    // Comportamiento de transición a PATROL
    private void ToPatrol()
    {
        if (!waitingToPatrol) StartCoroutine(WaitToPatrol());
    }

    // Comportamiento de transición a SEARCHING
    private void ToSearching()
    {
        Debug.Log("Changed to SEARCHING");
        state = ENEMY_STATE.SEARCHING;
    }

    // Comportamiento de transición a ATTACK
    private void ToAttack()
    {
        Debug.Log("Changed to ATTACK");
        state = ENEMY_STATE.ATTACK;
    }

    // Comportamiento de transición a DEAD
    private void ToDead()
    {
        isDead = true;
        state = ENEMY_STATE.DEAD;
        gameObject.SetActive(false);
    }

    // Espera un determinado tiempo a volver a estado PATROL, a menos que vuelva a detectar al objetivo
    private IEnumerator WaitToPatrol()
    {
        waitingToPatrol = true;

        yield return new WaitForSeconds(Random.Range(minPatrolDelay, maxPatrolDelay));

        if (!TargetDetectedOnArea(areaRadius))
        {
            Debug.Log("Changed to PATROL");

            RandomFlip();

            lastTargetPos = Vector3.zero;
            detection.EnableVision();
            state = ENEMY_STATE.PATROL;
        }

        waitingToPatrol = false;
    }

    // Espera un determinado tiempo a volver a estado CHASING, a menos que vuelva a detectar al objetivo
    private IEnumerator WaitToChase()
    {
        waitingToChase = true;

        yield return new WaitForSeconds(Random.Range(minChaseDelay, maxChaseDelay));

        if (TargetDetected() || TargetDetectedOnArea(detectedAreaRadius))
        {
            Debug.Log("Changed to CHASING");
            detection.DisableVision();
            state = ENEMY_STATE.CHASING;
        }

        waitingToChase = false;
    }

    private IEnumerator WaitToIdle()
    {
        waitingToDecideIdle = true;

        detection.EnableVision();

        yield return new WaitForSeconds(Random.Range(minIdleDelay, maxIdleDelay));

        if (!TargetDetected() || !TargetDetectedOnArea(detectedAreaRadius))
        {
            state = ENEMY_STATE.IDLE;
        }

        waitingToDecideIdle = false;
    }

    #endregion

    #region STATE CONDITION

    // ========================================================= FUNCIONES DE CONDICIÓN DE ESTADO ========================================================= //

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

    // Bucle de ataque del enemigo
    private IEnumerator AttackProcess()
    {
        alreadyAttacking = true;
        hitboxCreated = true;

        // Limpia los objetivos del ataque anterior
        hitTargets.Clear();

        yield return new WaitForSeconds(attackDuration);

        hitboxCreated = false;

        yield return new WaitForSeconds(attackCooldown);

        alreadyAttacking = false;
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

    // Devuelve si ha detectado a su objetivo
    private bool TargetDetected()
    {
        if (detection.GetTargetTransform() != null)
        {
            targetTransform = detection.GetTargetTransform();
        }

        return detection.DetectedTarget();
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

    // Devuelve si el objetivo esta en rango de ataque
    private bool OnAttackRange(Vector3 targetPos)
    {
        return Vector3.Distance(targetPos, transform.position) <= attackDetectionRange;
    }

    // Devuelve si esta detectando suelo delante suyo
    private bool DetectedGround()
    {
        Debug.DrawRay(groundCheck.position, Vector3.down, Color.red);
        return Physics2D.Raycast(groundCheck.position, Vector3.down, 1f, groundLayer);
    }

    // Gira al enemigo si colisiona con un elemento del escenario
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Ground"))
        {
            ContactPoint2D contact = collision.GetContact(0);

            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                Flip();
            }
        }
    }

    // Gira al enemigo en direccion contraria
    private void Flip()
    {
        invertedDirection = !invertedDirection;

        transform.Rotate(0, 180, 0);
    }

    // Da una probabilidad de que el enemigo se de la vuelta
    private void RandomFlip()
    {
        ENEMY_STATE[] validStates = { ENEMY_STATE.IDLE };

        if (validStates.Contains(state) && Random.Range(0f, 100f) <= probabilityToTurn)
        {
            Flip();
        }
    }

    // Devuelve si el jugador esta detras del enemigo o no
    private bool PlayerBehind()
    {
        float deltaX = targetTransform.position.x - transform.position.x;

        return deltaX < 0;
    }

    private void ResetEnemy()
    {
        if (!canRespawn) return;

        alreadyAttacking = false;
        hitboxCreated = false;
        waitingToPatrol = false;
        waitingToChase = false;
        waitingToDecideIdle = false;

        rb.linearVelocity = Vector3.zero;
        isDead = false;
        state = defaultState;
        characterStats.FullHeal();
        transform.position = spawnPosition;
        transform.rotation = Quaternion.Euler(spawnRotation);
        detection.EnableVision();
    }

    #endregion
}
