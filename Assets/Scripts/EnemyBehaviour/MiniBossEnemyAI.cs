using System.Collections;
using UnityEngine;
using System;

public class MiniBossEnemyAI : MonoBehaviour
{
    private enum BOSS_STATE { IDLE, ATTACK, HEAVY_ATTACK, PREPARING_JUMP, JUMP_ATTACK };

    [Header("Debug:")]
    [SerializeField] private BOSS_STATE state;
    [SerializeField] private Transform targetTransform;

    [Header("Ground Detection:")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

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
    [SerializeField] private float spikeSizeMultiplier = 0.3f;

    [Header("Jump Attack:")]
    [SerializeField] private float jumpPreparationTime = 1f;
    [SerializeField] private float jumpHeight = 7f;
    [SerializeField] private float jumpDuration = 3f;
    [SerializeField] private AnimationCurve jumpCurve;

    [Header("Patterns:")]
    [Range(0f, 100f)] public float multiAttackProbability = 70f;
    [SerializeField] private int maxMultiAttackChain = 3;
    [Range(0f, 100f)] public float heavyAfterJumpProbability = 30f;

    private int multiAttackCount = 0;

    private Vector3 jumpStartPosition;
    private Vector3 jumpLandingPosition;
    private Vector3 lastJumpVerticalPosition;

    private bool alreadyAttacking = false;
    private bool attackHitboxActive = false;

    private bool alreadyHeavyAttacking = false;
    private bool heavyAttackHitboxActive = false;

    private bool alreadyJumpAttacking = false;

    private Rigidbody2D rb;
    private float timer;
    private bool invertedDirection = false;

    private void Start()
    {
        state = BOSS_STATE.IDLE;
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        switch (state)
        {
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

    // Comportamiento en estado IDLE
    private void OnIdle()
    {
        rb.linearVelocity = Vector3.zero;

        if (PlayerBehind() != invertedDirection) Flip();
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

            ProbabilityToAct(heavyAfterJumpProbability, ToHeavyAttack, ToIdle);
        }

        lastJumpVerticalPosition = jumpPosition;
    }
    private void ToIdle()
    {
        state = BOSS_STATE.IDLE;
    }

    private void ToAttack()
    {
        if (PlayerBehind() != invertedDirection) Flip();
        state = BOSS_STATE.ATTACK;
    }

    private void ToConsecutiveAttack()
    {
        multiAttackCount++;
        if (PlayerBehind() != invertedDirection) Flip();

        if (multiAttackCount < maxMultiAttackChain)
        {
            state = BOSS_STATE.ATTACK;
        }
        else 
        {
            state = BOSS_STATE.IDLE;
        }
    }

    private void ToHeavyAttack()
    {
        if (PlayerBehind() != invertedDirection) Flip();
        state = BOSS_STATE.HEAVY_ATTACK;
    }

    // ========================================================= FUNCIONES DE CONDICIÓN DE ESTADO ========================================================= //

    // Seqüencia de ataque del enemigo
    private IEnumerator AttackProcess()
    {
        alreadyAttacking = true;
        rb.AddForce(-transform.right * 3f, ForceMode2D.Impulse);

        yield return new WaitForSeconds(attackDelay);

        attackHitboxActive = true;

        rb.linearVelocity = transform.right * attackMovementSpeed;

        Collider2D hit = Physics2D.OverlapCircle(transform.position + transform.right * attackRange, attackRadius);

        yield return new WaitForSeconds(attackMovementDuration);

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

        jumpStartPosition = transform.position;
        jumpLandingPosition = new Vector3(targetTransform.position.x, transform.position.y, transform.position.z);

        state = BOSS_STATE.JUMP_ATTACK;
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
}
