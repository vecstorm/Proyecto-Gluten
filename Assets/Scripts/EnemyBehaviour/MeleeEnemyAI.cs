using System.Collections;
using System.Linq;
using UnityEngine;

public class MeleeEnemyAI : EnemyAI
{
    [Header("Movement:")]
    [SerializeField] protected float speed = 2f;
    [SerializeField] private float probabilityToTurn = 30f;
    [SerializeField] private float stunnedTimerDuration = 1f;

    [Header("Target Detection:")]
    [SerializeField] private float areaRadius = 3f;
    [SerializeField] private float detectedAreaRadius = 10f;
    private bool detectedAreaActive = true;
    private VisionCone detection;

    [Header("Edge Detection:")]
    [SerializeField] private Transform edgeCheck;

    [Header("Attack:")]
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackDetectionRange = 3f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackRadius = 1f;
    [SerializeField] private float attackDuration = 0.1f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float attackPreparationTime = 0.5f;
    [SerializeField] private float attackPreparationForce = 5f;

    [Header("Delays:")]
    [SerializeField] private float minPatrolDelay = 0.5f;
    [SerializeField] private float maxPatrolDelay = 2f;
    [SerializeField] private float minChaseDelay = 0.2f;
    [SerializeField] private float maxChaseDelay = 0.7f;
    [SerializeField] private float minIdleDelay = 7f;
    [SerializeField] private float maxIdleDelay = 20f;
    [SerializeField] private float minSearchingDelay = 1f;
    [SerializeField] private float maxSearchingDelay = 1.5f;

    private bool alreadyAttacking = false;
    private bool hitboxCreated = false;
    private Transform targetTransform;

    private Vector3 lastTargetPos;

    private float timer;

    #region MAIN LOOP

    private void Awake()
    {
        InitializeBasicComponents();
    }

    private void Start()
    {
        detection = GetComponent<VisionCone>();
        spawnPosition = transform.position;
        spawnRotation = transform.rotation.eulerAngles;
    }

    private void OnEnable()
    {
        characterStats.OnDeath += ToDead;
        characterStats.OnStunApplied += ToStunned;
        characterStats.OnDamageTaken += FlashColor;
        characterStats.OnDamageTaken += AddKnockback;
    }

    private void OnDisable()
    {
        characterStats.OnDeath -= ToDead;
        characterStats.OnStunApplied -= ToStunned;
        characterStats.OnDamageTaken -= FlashColor;
        characterStats.OnDamageTaken -= AddKnockback;
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

            case ENEMY_STATE.PREPARING_CHASE:
                OnPreparingChase();
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

            case ENEMY_STATE.STUNNED:
                OnStunned();
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

        if (hitboxCreated)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
            Gizmos.DrawSphere(transform.position + transform.right * attackRange, attackRadius);

            Gizmos.color = Color.orange;
            Gizmos.DrawWireSphere(transform.position + transform.right * attackRange, attackRadius);
        }

        if (detectedAreaActive)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, state == ENEMY_STATE.PATROL || state == ENEMY_STATE.IDLE || state == ENEMY_STATE.PREPARING_CHASE ? areaRadius : detectedAreaRadius);
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
        if (TargetDetected() || TargetDetectedOnArea(areaRadius)) 
        {
            ToPreparingChase();
            return;
        }

        timer -= Time.fixedDeltaTime;

        if (timer <= 0)
        {
            ToPatrol();
        }
        
        //rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        localMovement = new Vector3(0, rb.linearVelocity.y, 0);
    }

    // Comportamiento en estado PATROL
    protected virtual void OnPatrol()
    {
        if (TargetDetected() || TargetDetectedOnArea(areaRadius))
        {
            ToPreparingChase();
            return;
        }

        timer -= Time.fixedDeltaTime;

        if (timer <= 0)
        {
            ToIdle();
            return;
        }

        //rb.linearVelocity = new Vector3((invertedDirection ? -1 : 1) * speed, rb.linearVelocity.y);
        localMovement = new Vector3((invertedDirection ? -1 : 1) * speed, rb.linearVelocity.y);

        if (!DetectedEdge())
        {
            Flip();
        }
    }

    private void OnPreparingChase()
    {
        timer -= Time.fixedDeltaTime;

        rb.linearVelocity = rb.linearVelocity;

        if (timer <= 0)
        {
            if (!TargetDetected() && !TargetDetectedOnArea(areaRadius))
            {
                ToPatrol();
                return;
            }

            ToChasing();
        }
    }

    // Comportamiento en estado CHASING
    private void OnChasing()
    {
        if (!TargetDetectedOnArea(detectedAreaRadius)) 
        {
            ToSearching();
            return;
        }

        if (targetTransform != null && OnAttackRange(targetTransform.position)) 
        {
            ToAttack();
            return;
        }
        
        if (PlayerBehind() != invertedDirection) Flip();

        //rb.linearVelocity = new Vector3(GetTargetDirection().x * (speed * 2f), rb.linearVelocity.y);
        localMovement = new Vector3(GetTargetDirection().x * (speed * 2f), rb.linearVelocity.y);

        if (!DetectedEdge())
        {
            //rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            localMovement = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    // Comportamiento en estado SEARCHING
    private void OnSearching()
    {
        if (TargetDetectedOnArea(detectedAreaRadius)) 
        {
            ToPreparingChase();
            return;
        }
        
        Vector3 movementPosition = new Vector3(lastTargetPos.x, transform.position.y, transform.position.z);
        
        if (!DetectedEdge())
        {
            movementPosition = transform.position;
        }

        rb.MovePosition(Vector3.MoveTowards(transform.position, movementPosition, (speed * 2f) * Time.fixedDeltaTime));

        if (Vector3.Distance(transform.position, movementPosition) < 0.1f)
        {
            timer -= Time.fixedDeltaTime;

            if (timer <= 0)
            {
                ToPatrol();
            }
        }
    }

    private void OnStunned()
    {
        timer -= Time.fixedDeltaTime;

        //rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        localMovement = new Vector3(0, rb.linearVelocity.y, 0);

        if (timer <= 0f)
        {
            stunnedIcon.SetActive(false);
            ToPatrol();
        }
    }

    // Comportamiento en estado ATTACK
    private void OnAttack()
    {
        bool onAttackRange = OnAttackRange(targetTransform.position);
        bool targetDetected = TargetDetectedOnArea(detectedAreaRadius);

        if (targetDetected && !onAttackRange && !alreadyAttacking) 
        {
            ToChasing();
            return;
        }

        if (!targetDetected && !onAttackRange && !alreadyAttacking) 
        {
            ToSearching();
            return;
        } 

        if (!alreadyAttacking)
        {
            StartCoroutine(AttackProcess());
        }
        else
        {
            if (PlayerBehind() != invertedDirection)
            {
                Flip();
            }
        }

        //rb.linearVelocity = Vector3.zero;
        localMovement = Vector3.zero;

        if (hitboxCreated)
        {
            Vector3 colliderPosition = transform.position + transform.right * attackRange;

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
        timer = Random.Range(minPatrolDelay, maxPatrolDelay);
        //rb.linearVelocity = Vector3.zero;
        localMovement = Vector3.zero;
        detection.EnableVision();

        state = ENEMY_STATE.IDLE;
    }

    // Comportamiento de transición a PATROL
    private void ToPatrol()
    {
        lastTargetPos = Vector3.zero;
        timer = Random.Range(minIdleDelay, maxIdleDelay);
        detection.EnableVision();

        state = ENEMY_STATE.PATROL;
    }

    private void ToPreparingChase()
    {
        timer = Random.Range(minChaseDelay, maxChaseDelay);

        state = ENEMY_STATE.PREPARING_CHASE;
    }

    // Comportamiento de transición a CHASING
    private void ToChasing()
    {
        detection.DisableVision();

        state = ENEMY_STATE.CHASING;
    }

    // Comportamiento de transición a SEARCHING
    private void ToSearching()
    {
        timer = Random.Range(minSearchingDelay, maxSearchingDelay);

        state = ENEMY_STATE.SEARCHING;
    }

    private void ToStunned()
    {
        timer = stunnedTimerDuration;
        stunnedIcon.SetActive(true);
        state = ENEMY_STATE.STUNNED;
    }

    // Comportamiento de transición a ATTACK
    private void ToAttack()
    {
        Debug.Log("Changed to ATTACK");
        //rb.linearVelocity = Vector3.zero;
        localMovement = Vector3.zero;

        state = ENEMY_STATE.ATTACK;
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

        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 20));

        //Debug.Log("PREPARING ATTACK");

        yield return new WaitForSeconds(attackPreparationTime);

        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0));

        hitboxCreated = true;

        // Limpia los objetivos del ataque anterior
        hitTargets.Clear();

        //Debug.Log("ATTACKING");

        yield return new WaitForSeconds(attackDuration);

        //Debug.Log("STOPPED ATTACKING");

        hitboxCreated = false;

        yield return new WaitForSeconds(attackCooldown);

        //Debug.Log("ATTACK ON COOLDOWN");

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
    private bool DetectedEdge()
    {
        Debug.DrawRay(edgeCheck.position, Vector3.down, Color.red);
        return Physics2D.Raycast(edgeCheck.position, Vector3.down, 1f, groundLayer);
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

    // Devuelve si el jugador esta detras del enemigo o no
    private bool PlayerBehind()
    {
        float deltaX = targetTransform.position.x - transform.position.x;

        return deltaX < 0;
    }

    public override void ResetEnemy()
    {
        if (!canRespawn) return;

        alreadyAttacking = false;
        hitboxCreated = false;

        ResetColor();
        //rb.linearVelocity = Vector3.zero;
        localMovement = Vector3.zero;
        isDead = false;
        state = defaultState;
        characterStats.FullHeal();
        transform.position = spawnPosition;
        transform.rotation = Quaternion.Euler(spawnRotation);
        invertedDirection = transform.rotation == Quaternion.Euler(0, 180, 0);
        detection.EnableVision();
    }

    #endregion
}
