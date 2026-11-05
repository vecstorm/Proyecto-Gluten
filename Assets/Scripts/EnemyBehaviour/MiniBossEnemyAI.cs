using System.Collections;
using UnityEngine;
using System;

public class MiniBossEnemyAI : MonoBehaviour
{
    private enum BOSS_STATE { INTRO, IDLE, ATTACK, HEAVY_ATTACK, PREPARING_JUMP, JUMP_ATTACK };

    [Header("Debug:")]
    [SerializeField] private BOSS_STATE state;
    [SerializeField] private bool bossActive = false;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private int health = 30;

    private int maxHealth;
    private BOSS_STATE lastAttackState;

    [Header("Ground Detection:")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Intro / Idle:")]
    [SerializeField] private float introTime = 2f;
    [SerializeField] private float minIdleTime = 1f;
    [SerializeField] private float maxIdleTime = 1.7f;

    [Header("Attack:")]
    [SerializeField] private float attackRadius = 3f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDuration = 0.3f;
    [SerializeField] private float attackMovementSpeed = 30f;
    [SerializeField] private float attackMovementDuration = 0.2f;
    [SerializeField] private float attackRecoverDuration = 0.6f;
    [SerializeField] private float attackDelay = 1f;

    [Header("Heavy Attack:")]
    [SerializeField] private float heavyAttackRadius = 6f;
    [SerializeField] private float heavyAttackDelay = 1f;
    [SerializeField] private float heavyAttackRecoverDuration = 1f;
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private int spikeAmount = 8;
    [SerializeField] private float spikeRange = 6f;
    [SerializeField] private float spikeLifeTime = 3f;
    [SerializeField] private float spikeSizeMultiplier = 0.3f;

    [Header("Jump Attack:")]
    [SerializeField] private float targetMinDistanceToJump = 10f;
    [SerializeField] private float jumpPreparationTime = 1f;
    [SerializeField] private float jumpHeight = 7f;
    [SerializeField] private float jumpDuration = 3f;
    [SerializeField] private AnimationCurve jumpCurve;
    
    [Header("Patterns:")]
    [Range(0f, 100f)] public float baseAttackProbability = 60f;
    [Range(0f, 100f)] public float multiAttackProbability = 70f;
    [SerializeField] private int maxMultiAttackChain = 3;
    [Range(0f, 100f)] public float heavyAfterJumpProbability = 30f;
    [SerializeField] private float multiJumpHealthTrigger = 20f;
    [SerializeField] private int maxMultiJumpChain = 6;

    [Header("Difficulty:")]
    [SerializeField] private int difficultyLevel = 0;
    [Range(0f, 1f)] public float movementDurationMultiplier = 0.2f;
    [Range(-1f, 0f)] public float attackDelayMultiplier = -0.3f;

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

    private bool waitingToAct = false;

    private float healthModule = 0;
    private Rigidbody2D rb;
    private float timer;
    private bool invertedDirection = false;

    private void Start()
    {
        state = BOSS_STATE.INTRO;
        rb = GetComponent<Rigidbody2D>();

        spawnPosition = transform.position;

        maxHealth = health;
        healthModule = health / multiJumpHealthTrigger;
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

    // ========================================================= FUNCIONES DE COMPORTAMIENTO DE ESTADO ========================================================= //

    // Comportamiento en estado INTRO
    private void OnIntro()
    {
        if (!bossActive) return;

        timer += Time.fixedDeltaTime;

        if (timer >= introTime)
        {
            ToAttack();
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
    }

    // Comportamiento en estado HEAVY_ATTACK
    private void OnHeavyAttack()
    {
        if (!alreadyHeavyAttacking)
        {
            StartCoroutine(HeavyAttackProcess());
        }
    }

    // Comportamiento en estado PREPARING_JUMP
    private void OnPreparingJump()
    {
        rb.linearVelocity = Vector3.zero;
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

        Vector3 jumpPosition = Vector3.Lerp(jumpStartPosition, jumpLandingPosition, timer / jumpDuration);

        jumpPosition.y += jumpCurve.Evaluate(timer / jumpDuration) * jumpHeight;

        rb.MovePosition(jumpPosition);

        if (jumpPosition.y < lastJumpVerticalPosition.y && DetectedGround())
        {
            timer = 0f;
            alreadyJumpAttacking = false;

            if (health % healthModule == 0 && health != maxHealth && health != 0)
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

        if (multiJumpCount < maxMultiJumpChain)
        {
            ToPreparingJump();
        }
        else
        {
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

    // ========================================================= FUNCIONES DE CONDICIÓN DE ESTADO ========================================================= //

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
        rb.AddForce(-transform.right * 3f, ForceMode2D.Impulse);

        yield return new WaitForSeconds(StatWithMultiplier(attackDelay, attackDelayMultiplier));

        attackHitboxActive = true;

        rb.linearVelocity = transform.right * attackMovementSpeed;

        Collider2D hit = Physics2D.OverlapCircle(transform.position + transform.right * attackRange, attackRadius);

        yield return new WaitForSeconds(StatWithMultiplier(attackMovementDuration, movementDurationMultiplier));

        rb.linearVelocity = Vector3.zero;

        yield return new WaitForSeconds(attackRecoverDuration);

        attackHitboxActive = false;
        alreadyAttacking = false;

        ProbabilityToAct(multiAttackProbability, ToConsecutiveAttack, ToIdle);
    }

    // Seqüencia de ataque pesado del enemigo
    private IEnumerator HeavyAttackProcess()
    {
        alreadyHeavyAttacking = true;
        transform.localScale = new Vector3(3f, 3.2f, 3f);

        yield return new WaitForSeconds(heavyAttackDelay);

        heavyAttackHitboxActive = true;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, heavyAttackRadius);

        transform.localScale = new Vector3(3f, 2.8f, 3f);

        StartCoroutine(SpawnSpikes());

        yield return new WaitForSeconds(heavyAttackRecoverDuration);

        transform.localScale = new Vector3(3f, 3f, 3f);

        heavyAttackHitboxActive = false;
        alreadyHeavyAttacking = false;

        state = BOSS_STATE.IDLE;
    }

    // Tiempo de espera para pasar al estado JUMP_ATTACK
    private IEnumerator WaitToJump()
    {
        alreadyJumpAttacking = true;

        yield return new WaitForSeconds(jumpPreparationTime);

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
        Vector3 rayPoint = new Vector3(transform.position.x + offset, transform.position.y + 4f);

        RaycastHit2D hit = Physics2D.Raycast(rayPoint, -Vector3.up, 8f, groundLayer);

        if (hit.collider != null)
        {
            Vector3 spikeSpawnPoint = new Vector3(hit.point.x, hit.point.y, transform.position.z);

            GameObject spike = Instantiate(spikePrefab, spikeSpawnPoint, Quaternion.identity);
            spike.transform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, scaleMultiplier);

            spike.GetComponent<AutoDestroy>().SetLifeTime(spikeLifeTime);
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
}
