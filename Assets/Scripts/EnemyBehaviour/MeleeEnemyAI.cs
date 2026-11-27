using System.Collections;
using System.Linq;
using UnityEngine;

public class MeleeEnemyAI : TroopAI
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

    private bool alreadyStopped = false; 

    private Vector3 lastTargetPos;

    private Collider2D hitbox;

    private float timer;

    #region MAIN LOOP

    private void Awake()
    {
        InitializeBasicComponents();

        hitbox = GetComponent<Collider2D>();
    }

    private void Start()
    {
        detection = GetComponent<VisionCone>();
        spawnPosition = transform.position;
        spawnRotation = transform.rotation.eulerAngles;
        material = GetComponent<SpriteRenderer>().material;
    }

    private void OnEnable()
    {
        characterStats.OnDeath += ToDead;
        characterStats.OnStunApplied += ToStunned;
        characterStats.OnDamageTaken += DamageReaction;
        characterStats.OnDamageTaken += AddKnockback;
    }

    private void OnDisable()
    {
        characterStats.OnDeath -= ToDead;
        characterStats.OnStunApplied -= ToStunned;
        characterStats.OnDamageTaken -= DamageReaction;
        characterStats.OnDamageTaken -= AddKnockback;
    }

    private void FixedUpdate()
    {
        if (state == ENEMY_STATE.DEAD || !GroundBeneath())
        {
            rb.linearVelocityX = 0;
            return;
        }

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

        Gizmos.color = Color.purple;
        Gizmos.DrawWireSphere(groundDetection.position, groundDetectionRadius);

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
            if (OnAttackRange(targetTransform.position))
            {
                ToAttack();
                return;
            }

            ToPreparingChase();
            return;
        }

        timer -= Time.fixedDeltaTime;

        if (timer <= 0)
        {
            ToPatrol();
        }
        
        localMovement = new Vector3(0, rb.linearVelocity.y, 0);
    }

    // Comportamiento en estado PATROL
    protected virtual void OnPatrol()
    {
        if (!DetectedEdge())
        {
            Flip();
        }

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

        localMovement = new Vector3((invertedDirection ? -1 : 1) * speed, rb.linearVelocity.y);
    }

    // Comportamiento en estado PREPARING_CHASE
    private void OnPreparingChase()
    {
        timer -= Time.fixedDeltaTime;

        rb.linearVelocity = rb.linearVelocity;

        if (!DetectedEdge())
        {
            localMovement = new Vector3(0, rb.linearVelocity.y, 0);
        }

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
        
        if (TargetBehind(targetTransform.position)) Flip();

        localMovement = new Vector3(GetTargetDirection().x * (speed * 2f), rb.linearVelocity.y);

        if (!DetectedEdge())
        {
            if (!alreadyStopped)
            {
                alreadyStopped = true;
                InvokeAnimationTrigger(ENEMY_STATE.IDLE);
            }

            localMovement = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    // Comportamiento en estado SEARCHING
    private void OnSearching()
    {
        if (TargetDetectedOnArea(detectedAreaRadius)) 
        {
            ToChasing();
            return;
        }
        
        Vector3 movementPosition = new Vector3(lastTargetPos.x, transform.position.y, transform.position.z);

        bool hasDetectedWall = DetectedWall();

        if (!DetectedEdge() || hasDetectedWall)
        {
            if (!alreadyStopped)
            {
                alreadyStopped = true;
                InvokeAnimationTrigger(ENEMY_STATE.IDLE);
            }
            
            movementPosition = transform.position;
        }

        rb.MovePosition(Vector3.MoveTowards(transform.position, movementPosition, (speed * 2f) * Time.fixedDeltaTime));

        if (Vector3.Distance(transform.position, movementPosition) < 0.1f)
        {
            timer -= Time.fixedDeltaTime;

            if (timer <= 0)
            {
                if (hasDetectedWall)
                {
                    Flip();
                }

                ToPatrol();
            }
        }
    }

    // Comportamiento en estado STUNNED
    private void OnStunned()
    {
        timer -= Time.fixedDeltaTime;

        //rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        localMovement = new Vector3(0, rb.linearVelocity.y, 0);

        if (timer <= 0f)
        {
            stunnedIcon.SetActive(false);
            ToIdle();
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
            if (TargetBehind(targetTransform.position))
            {
                Flip();
            }
        }

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

    // Comportamiento de transición a IDLE
    private void ToIdle()
    {
        timer = Random.Range(minPatrolDelay, maxPatrolDelay);
        localMovement = Vector3.zero;
        detection.EnableVision();

        InvokeAnimationTrigger(ENEMY_STATE.IDLE);

        state = ENEMY_STATE.IDLE;
    }

    // Comportamiento de transición a PATROL
    private void ToPatrol()
    {
        lastTargetPos = Vector3.zero;
        timer = Random.Range(minIdleDelay, maxIdleDelay);
        detection.EnableVision();

        InvokeAnimationTrigger(ENEMY_STATE.PATROL);

        state = ENEMY_STATE.PATROL;
    }

    // Comportamiento de transición a PREPARING_CHASE
    private void ToPreparingChase()
    {
        timer = Random.Range(minChaseDelay, maxChaseDelay);

        state = ENEMY_STATE.PREPARING_CHASE;
    }

    // Comportamiento de transición a CHASING
    private void ToChasing()
    {
        alreadyStopped = false;

        detection.DisableVision();

        InvokeAnimationTrigger(ENEMY_STATE.CHASING);

        state = ENEMY_STATE.CHASING;
    }

    // Comportamiento de transición a SEARCHING
    private void ToSearching()
    {
        alreadyStopped = false;

        timer = Random.Range(minSearchingDelay, maxSearchingDelay);

        state = ENEMY_STATE.SEARCHING;
    }

    // Comportamiento de transición a STUNNED
    private void ToStunned()
    {
        timer = stunnedTimerDuration;
        stunnedIcon.SetActive(true);

        // SONIDO ---------> Stun //
        InvokeAnimationTrigger(ENEMY_STATE.STUNNED);

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
        ParticleManager.SpawnParticles("DeathImpact", transform.position, Quaternion.identity);

        hitbox.enabled = false;
        isDead = true;
        state = ENEMY_STATE.DEAD;
        localMovement = Vector3.zero;

        InvokeAnimationTrigger(ENEMY_STATE.DEAD);
        // SONIDO ---------> Death //
    }

    #endregion

    #region STATE CONDITION

    // ========================================================= FUNCIONES DE CONDICIÓN DE ESTADO ========================================================= //

    // Bucle de ataque del enemigo
    private IEnumerator AttackProcess()
    {
        alreadyAttacking = true;

        // SONIDO ---------> Attack Prepare //
        InvokeAnimationTrigger(ENEMY_STATE.ATTACK);

        yield return new WaitForSeconds(attackPreparationTime);

        // SONIDO ---------> Attack //

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

    #endregion

    #region UTILITIES

    // ========================================================= FUNCIONES DE UTILIDAD ========================================================= //

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

    // Gira al enemigo si colisiona con un elemento del escenario
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Ground"))
        {
            ContactPoint2D contact = collision.GetContact(0);

            if (tryiedFlipAirborne && GroundBeneath())
            {
                tryiedFlipAirborne = false;
                Flip();
                return;
            }

            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                Flip();
            }
        }
    }

    private bool DetectedWall()
    {
        return Physics2D.Raycast(groundDetection.position, Vector3.right, 1f, groundLayer);
    }

    private void DamageReaction()
    {
        // SONIDO ---------> Hurt //
        FlashColor();
        ToChasing();
    }

    // Logica de reseteo del enemigo
    public override void ResetEnemy()
    {
        if (!canRespawn) return;

        gameObject.SetActive(true);

        alreadyAttacking = false;
        hitboxCreated = false;

        ResetColor();
        localMovement = Vector3.zero;
        externalMovement = Vector3.zero;
        isDead = false;
        hitbox.enabled = true;
        state = defaultState;
        InvokeAnimationTrigger(state);
        characterStats.FullHeal();
        transform.position = spawnPosition;
        transform.rotation = Quaternion.Euler(spawnRotation);
        invertedDirection = transform.rotation == Quaternion.Euler(0, 180, 0);
        detection.EnableVision();
    }

    #endregion
}
