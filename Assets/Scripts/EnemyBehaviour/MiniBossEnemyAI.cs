using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniBossEnemyAI : MonoBehaviour
{
    private enum BOSS_STATE { INTRO, IDLE, ATTACK, HEAVY_ATTACK, PREPARING_JUMP, JUMP_ATTACK };

    [Header("Debug:")]
    [SerializeField] private BOSS_STATE state;
    [SerializeField] private bool bossActive = false;
    [SerializeField] private Transform targetTransform;
    [SerializeField, Min(2)] private int healthToDivideDifficulty = 2;

    [Header("Ground Detection:")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Intro / Idle:")]
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private float introTime = 2f;
    [SerializeField] private float minIdleTime = 1f;
    [SerializeField] private float maxIdleTime = 1.7f;

    [Header("Attack:")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackRadius = 3f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDuration = 0.3f;
    [SerializeField] private float attackMovementSpeed = 30f;
    [SerializeField] private float attackMovementDuration = 0.2f;
    [SerializeField] private float attackRecoverDuration = 0.6f;
    [SerializeField] private float attackDelay = 1f;
    [SerializeField] private float attackAnticipationForce = 3f;

    [Header("Heavy Attack:")]
    [SerializeField] private int heavyAttackDamage = 10;
    [SerializeField] private float heavyAttackRadius = 6f;
    [SerializeField] private float heavyAttackDelay = 1f;
    [SerializeField] private float heavyAttackRecoverDuration = 1f;
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private int spikeDamage = 10;
    [SerializeField] private int spikeAmount = 8;
    [SerializeField] private float spikeRange = 6f;
    [SerializeField] private float spikeLifeTime = 3f;
    [SerializeField] private float spikeSizeMultiplier = 0.3f;

    [Header("Jump Attack:")]
    [SerializeField] private int jumpAttackDamage = 20;
    [SerializeField] private float targetMinDistanceToJump = 10f;
    [SerializeField] private float jumpPreparationTime = 1f;
    [SerializeField] private float jumpHeight = 7f;
    [SerializeField] private float jumpDuration = 3f;
    [SerializeField] private AnimationCurve jumpCurve;
    
    [Header("Patterns:")]
    [SerializeField][Range(0f, 100f)] private float baseAttackProbability = 60f;
    [SerializeField][Range(0f, 100f)] private float multiAttackProbability = 70f;
    [SerializeField] private int maxMultiAttackChain = 3;
    [SerializeField][Range(0f, 100f)] private float heavyAfterJumpProbability = 30f;
    [SerializeField] private float multiJumpHealthTrigger = 20f;
    [SerializeField] private int maxMultiJumpChain = 6;

    [Header("Difficulty:")]
    [SerializeField] private int difficultyLevel = 0;
    [SerializeField][Range(-1f, 0f)] private float delayMultiplier = -0.3f;
    [SerializeField][Range(-1f, 0f)] private float recoverDurationMultiplier = -0.4f;
    [SerializeField][Range(0f, 1f)] private float attackMovementDurationMultiplier = 0.2f;
    [SerializeField][Range(-1f, 0f)] private float jumpDurationMultiplier = -0.4f;

    private BOSS_STATE lastAttackState;

    private int multiAttackCount = 0;
    private int multiJumpCount = 0;

    private Vector3 jumpStartPosition;
    private Vector3 jumpLandingPosition;
    private Vector3 lastJumpVerticalPosition;
    private Vector3 spawnPosition;

    private bool alreadyAttacking = false;
    private bool attackHitboxActive = false;

    private bool alreadyHeavyAttacking = false;
    private bool heavyAttackHitboxActive = false;

    private bool alreadyJumpAttacking = false;
    private bool alreadyMultiJumpAttacking = false;

    private bool waitingToAct = false;

    private float healthModule = 0;
    private Rigidbody2D rb;
    private float timer;
    private bool invertedDirection = false;

    private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

    CharacterStats characterStats;

    #region MAIN LOOP

    private void Awake()
    {
        characterStats = GetComponent<CharacterStats>();
    }

    private void Start()
    {
        state = BOSS_STATE.INTRO;
        rb = GetComponent<Rigidbody2D>();

        spawnPosition = transform.position;

        healthModule = characterStats.MaxHealth / (float)healthToDivideDifficulty;
    }

    private void OnEnable()
    {
        characterStats.OnDeath += Death;
        characterStats.OnDamageTaken += CheckHealthLeft;
    }

    private void OnDisable()
    {
        characterStats.OnDeath -= Death;
        characterStats.OnDamageTaken -= CheckHealthLeft;
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case BOSS_STATE.INTRO:
                OnIntro();
                break;

            case BOSS_STATE.IDLE:
                OnIdle();
                break;

            case BOSS_STATE.ATTACK:
                OnAttack();
                break;

            case BOSS_STATE.HEAVY_ATTACK:
                OnHeavyAttack();
                break;

            case BOSS_STATE.PREPARING_JUMP:
                OnPreparingJump();
                break;

            case BOSS_STATE.JUMP_ATTACK:
                OnJumpAttack();
                break;
        }
    }

    private void OnDrawGizmos()
    {
        if (attackHitboxActive)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
            Gizmos.DrawSphere(transform.position + transform.right * attackRange, attackRadius);

            Gizmos.color = Color.orange;
            Gizmos.DrawWireSphere(transform.position + transform.right * attackRange, attackRadius);
        }

        if (heavyAttackHitboxActive)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
            Gizmos.DrawSphere(transform.position, heavyAttackRadius);

            Gizmos.color = Color.orange;
            Gizmos.DrawWireSphere(transform.position, heavyAttackRadius);
        }
    }

    #endregion

    #region STATE BEHAVIOUR

    // ========================================================= FUNCIONES DE COMPORTAMIENTO DE ESTADO ========================================================= //

    // Comportamiento en estado INTRO
    private void OnIntro()
    {
        if (!bossActive) return;

        timer += Time.fixedDeltaTime;

        if (timer >= introTime)
        {
            ProbabilityToAct(baseAttackProbability, ToAttack, ToPreparingJump);
            timer = 0;
        }
    }

    // Comportamiento en estado IDLE
    private void OnIdle()
    {
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

        if (PlayerBehind() != invertedDirection) Flip();

        if (!waitingToAct)
        {
            StartCoroutine(WaitToAct());
        }
    }

    // Comportamiento en estado ATTACK
    private void OnAttack()
    {
        if (!alreadyAttacking)
        {
            StartCoroutine(AttackProcess());
        }

        if (attackHitboxActive)
        {
            Vector2 colliderPosition = transform.position + transform.right * attackRange;

            Collider2D[] hits = Physics2D.OverlapCircleAll(colliderPosition, attackRadius);

            CombatDamage damage = new CombatDamage(attackDamage, characterStats, colliderPosition, false);

            CombatManager.MakeDamage(transform, hits, hitTargets, damage);
        }
    }

    // Comportamiento en estado HEAVY_ATTACK
    private void OnHeavyAttack()
    {
        if (!alreadyHeavyAttacking)
        {
            StartCoroutine(HeavyAttackProcess());
        }

        if (heavyAttackHitboxActive)
        {
            Vector3 colliderPosition = transform.position;

            Collider2D[] hits = Physics2D.OverlapCircleAll(colliderPosition, heavyAttackRadius);

            CombatDamage damage = new CombatDamage(heavyAttackDamage, characterStats, colliderPosition, false);

            CombatManager.MakeDamage(transform, hits, hitTargets, damage);
        }
    }

    // Comportamiento en estado PREPARING_JUMP
    private void OnPreparingJump()
    {
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        if (PlayerBehind() != invertedDirection) Flip();

        if (!alreadyJumpAttacking)
        {
            StartCoroutine(WaitToJump());
        }
    }

    // Comportamiento en estado JUMP_ATTACK
    private void OnJumpAttack()
    {
        timer += Time.fixedDeltaTime;

        float finalJumpDuration = StatWithMultiplier(jumpDuration, jumpDurationMultiplier);

        Vector3 jumpPosition = Vector3.Lerp(jumpStartPosition, jumpLandingPosition, timer / finalJumpDuration);

        jumpPosition.y += jumpCurve.Evaluate(timer / finalJumpDuration) * jumpHeight;

        rb.MovePosition(jumpPosition);

        if (jumpPosition.y < lastJumpVerticalPosition.y && DetectedGround())
        {
            timer = 0f;
            alreadyJumpAttacking = false;

            if (alreadyMultiJumpAttacking)
            {
                ToConsecutiveJump();
            }
            else
            {
                ProbabilityToAct(heavyAfterJumpProbability, ToHeavyAttack, ToIdle);
            }
        }

        lastJumpVerticalPosition = jumpPosition;
    }

    #endregion

    #region STATE TRANSITION

    // ========================================================= FUNCIONES DE TRANSICIÓN DE ESTADO ========================================================= //

    // Comportamiento de transición a IDLE
    private void ToIdle()
    {
        ResetMultiActionCount();
        state = BOSS_STATE.IDLE;
    }

    // Comportamiento de transición a ATTACK
    private void ToAttack()
    {
        if (PlayerBehind() != invertedDirection) Flip();
        state = BOSS_STATE.ATTACK;
        lastAttackState = state;
    }

    // Comportamiento de transición a PREPARING_JUMP
    private void ToPreparingJump()
    {
        state = BOSS_STATE.PREPARING_JUMP;
    }

    // Comportamiento de transición a JUMP_ATTACK
    private void ToJump()
    {
        transform.localScale = new Vector3(3f, 3f, 3f);

        jumpStartPosition = transform.position;
        jumpLandingPosition = new Vector3(targetTransform.position.x, transform.position.y, transform.position.z);

        state = BOSS_STATE.JUMP_ATTACK;
        lastAttackState = state;
    }

    // Comportamiento de transición a ATTACK consecutivamente
    private void ToConsecutiveAttack()
    {
        multiAttackCount++;

        if (multiAttackCount < maxMultiAttackChain)
        {
            ToAttack();
        }
        else 
        {
            ToIdle();
        }
    }

    // Comportamiento de transición a JUMP_ATTACK consecutivamente
    private void ToConsecutiveJump()
    {
        multiJumpCount++;

        Debug.Log(maxMultiJumpChain + " | " + multiJumpCount);

        if (multiJumpCount <= maxMultiJumpChain)
        {
            ToPreparingJump();
        }
        else
        {
            alreadyMultiJumpAttacking = false;
            ToIdle();
        }
    }

    // Comportamiento de transición a HEAVY_ATTACK
    private void ToHeavyAttack()
    {
        if (PlayerBehind() != invertedDirection) Flip();
        state = BOSS_STATE.HEAVY_ATTACK;
        lastAttackState = state;
    }

    #endregion

    #region STATE CONDITION

    // ========================================================= FUNCIONES DE CONDICIÓN DE ESTADO ========================================================= //

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            hitTargets.Clear();
            CombatDamage damage = new CombatDamage(state != BOSS_STATE.JUMP_ATTACK ? contactDamage : jumpAttackDamage, characterStats, false);
            CombatManager.MakeDamage(transform, collision, hitTargets, damage, characterStats.Team);
        }
    }

    // Espera un tiempo definido para decidir que accion realizar
    private IEnumerator WaitToAct()
    {
        waitingToAct = true;

        yield return new WaitForSeconds(UnityEngine.Random.Range(minIdleTime, maxIdleTime));

        DecideAction();

        waitingToAct = false;
    }

    // Decide a que estado cambiar dependiendo de cual ha sido su ultimo ataque realizado
    private void DecideAction()
    {
        switch (lastAttackState)
        {
            case BOSS_STATE.ATTACK:

                if (TargetDistanceExceeds(targetMinDistanceToJump))
                {
                    ToPreparingJump();
                }
                else
                {
                    ProbabilityToAct(baseAttackProbability, ToHeavyAttack, ToAttack);
                }

                break;

            case BOSS_STATE.HEAVY_ATTACK:

                if (TargetDistanceExceeds(targetMinDistanceToJump))
                {
                    ProbabilityToAct(baseAttackProbability, ToPreparingJump, ToAttack);
                }
                else
                {
                    ToAttack();
                }

                break;

            case BOSS_STATE.JUMP_ATTACK:

                if (TargetDistanceExceeds(targetMinDistanceToJump))
                {
                    ToPreparingJump();
                }
                else
                {
                    ProbabilityToAct(baseAttackProbability, ToAttack, ToHeavyAttack);
                }

                break;
        }
    }

    // Devuelve si el objetivo esta a mas distancia que un valor
    private bool TargetDistanceExceeds(float distance)
    {
        return Vector3.Distance(transform.position, targetTransform.position) > distance;
    }

    // Seqüencia de ataque del enemigo
    private IEnumerator AttackProcess()
    {
        alreadyAttacking = true;
        rb.AddForce(-transform.right * attackAnticipationForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(StatWithMultiplier(attackDelay, delayMultiplier));

        attackHitboxActive = true;

        rb.linearVelocity = transform.right * attackMovementSpeed;

        // Limpia los objetivos del ataque anterior
        hitTargets.Clear();

        yield return new WaitForSeconds(StatWithMultiplier(attackMovementDuration, attackMovementDurationMultiplier));

        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

        yield return new WaitForSeconds(StatWithMultiplier(attackRecoverDuration, recoverDurationMultiplier));

        attackHitboxActive = false;
        alreadyAttacking = false;

        ProbabilityToAct(multiAttackProbability, ToConsecutiveAttack, ToIdle);
    }

    // Seqüencia de ataque pesado del enemigo
    private IEnumerator HeavyAttackProcess()
    {
        alreadyHeavyAttacking = true;
        transform.localScale = new Vector3(3f, 3.2f, 3f);

        yield return new WaitForSeconds(StatWithMultiplier(heavyAttackDelay, delayMultiplier));

        heavyAttackHitboxActive = true;

        // Limpia los objetivos del ataque anterior
        hitTargets.Clear();

        transform.localScale = new Vector3(3f, 3f, 3f);

        StartCoroutine(SpawnSpikes());

        yield return new WaitForSeconds(StatWithMultiplier(heavyAttackRecoverDuration, recoverDurationMultiplier));

        heavyAttackHitboxActive = false;
        alreadyHeavyAttacking = false;

        state = BOSS_STATE.IDLE;
    }

    // Tiempo de espera para pasar al estado JUMP_ATTACK
    private IEnumerator WaitToJump()
    {
        alreadyJumpAttacking = true;

        transform.localScale = new Vector3(3f, 2.8f, 3f);

        yield return new WaitForSeconds(StatWithMultiplier(jumpPreparationTime, delayMultiplier));

        ToJump();
    }

    // Crea dos hileras de pinchos alrededor del enemigo
    private IEnumerator SpawnSpikes()
    {
        float spacing = spikeRange / (spikeAmount / 2);

        for (int i = 0; i < spikeAmount / 2; i++)
        {
            float spikeScale = 1 + (i * spikeSizeMultiplier);

            SpawnSpikeAtOffset((i + 1) * spacing, spikeScale);
            SpawnSpikeAtOffset((-i - 1) * spacing, spikeScale);

            yield return new WaitForSeconds(0.2f);
        }
    }

    // Crea un pincho en el suelo a partir de un raycast
    private void SpawnSpikeAtOffset(float offset, float scaleMultiplier)
    {
        Vector3 rayPoint = new Vector3(transform.position.x + offset, transform.position.y);

        RaycastHit2D hit = Physics2D.Raycast(rayPoint, -Vector3.up, 8f, groundLayer);

        if (hit.collider != null)
        {
            Vector3 spikeSpawnPoint = new Vector3(hit.point.x, hit.point.y, transform.position.z);

            GameObject spike = Instantiate(spikePrefab, spikeSpawnPoint, Quaternion.identity);
            spike.transform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, scaleMultiplier);

            spike.GetComponent<AutoDestroy>().SetLifeTime(spikeLifeTime);
            spike.GetComponent<SpikeController>().SetContactDamage(spikeDamage);
        }
    }

    // Gira al enemigo en direccion contraria
    private void Flip()
    {
        invertedDirection = !invertedDirection;

        transform.Rotate(0, 180, 0);
    }

    // Devuelve si el jugador esta detras del enemigo o no
    private bool PlayerBehind()
    {
        float deltaX = targetTransform.position.x - transform.position.x;

        return deltaX < 0;
    }

    // Devuelve si esta detectando suelo delante suyo
    private bool DetectedGround()
    {
        Debug.DrawRay(groundCheck.position, Vector3.down, Color.blue);
        return Physics2D.Raycast(groundCheck.position, Vector3.down, 1f, groundLayer);
    }

    // Ejecuta una funcion depend
    private void ProbabilityToAct(float probability, Action behaviour, Action alternateBehaviour = null)
    {
        if (UnityEngine.Random.Range(0f, 100f) <= probability)
        {
            behaviour();
        }
        else
        {
            if (alternateBehaviour == null) return;

            alternateBehaviour();
        }
    }

    // Reinicia a 0 los contadores de ataques consecutivos
    private void ResetMultiActionCount()
    {
        multiAttackCount = 0;
        multiJumpCount = 0;
    }

    // Calcula el valor de un valor aplicandole un multiplicador que escala con el nivel de dificultad
    private float StatWithMultiplier(float baseStat, float statMultiplier)
    {
        return baseStat + (difficultyLevel * statMultiplier);
    }

    private void Death()
    {
        gameObject.SetActive(false);
    }

    private void CheckHealthLeft()
    {
        //float currenthealth = characterStats.CurrentHealth;

        if (HealthCheck())
        {
            //Debug.Log("Ha subido la dificultad | Current Health: " + currenthealth + " | Health Module: " + healthModule + " | Resultado: " + currenthealth % healthModule + " | Max Health: " + characterStats.MaxHealth);
            alreadyMultiJumpAttacking = true;
            difficultyLevel++;
            ToConsecutiveJump();
        }
    }

    private bool HealthCheck()
    {
        float currenthealth = characterStats.CurrentHealth;
        return currenthealth % healthModule == 0 && currenthealth != characterStats.MaxHealth && currenthealth != 0;
    }

    public void BossActive(bool value)
    {
        bossActive = value;
    }

    #endregion
}
