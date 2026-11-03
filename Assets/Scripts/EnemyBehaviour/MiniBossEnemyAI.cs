using System.Collections;
using UnityEditor;
using UnityEngine;

public class MiniBossEnemyAI : MonoBehaviour
{
    private enum BOSS_STATE { IDLE, ATTACK, HEAVY_ATTACK, JUMP_ATTACK };

    [Header("Debug:")]
    [SerializeField] private BOSS_STATE state;

    [Header("Attack:")]
    [SerializeField] private float attackRadius = 3f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDuration = 0.3f;
    [SerializeField] private float attackMovementDuration = 0.2f;
    [SerializeField] private float attackRecoverDuration = 0.6f;
    [SerializeField] private float attackDelay = 1f;

    [Header("Heavy Attack:")]
    [SerializeField] private float heavyAttackRadius = 6f;
    [SerializeField] private float heavyAttackDelay = 1f;
    [SerializeField] private float heavyAttackRecoverDuration = 1f;

    private bool alreadyAttacking = false;
    private bool attackHitboxActive = false;

    private bool alreadyHeavyAttacking = false;
    private bool heavyAttackHitboxActive = false;

    private Rigidbody2D rb;

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
            Gizmos.DrawSphere(transform.position + Vector3.right * attackRange, attackRadius);

            Gizmos.color = Color.orange;
            Gizmos.DrawWireSphere(transform.position + Vector3.right * attackRange, attackRadius);
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

    private void OnIdle()
    {
        rb.linearVelocity = Vector3.zero;
    }

    private void OnAttack()
    {
        if (!alreadyAttacking)
        {
            StartCoroutine(AttackProcess());
        }
    }

    private void OnHeavyAttack()
    {
        if (!alreadyHeavyAttacking)
        {
            StartCoroutine(HeavyAttackProcess());
        }
    }

    private void OnJumpAttack()
    {

    }

    private IEnumerator AttackProcess()
    {
        alreadyAttacking = true;
        rb.AddForce(-Vector3.right * 3f, ForceMode2D.Impulse);

        yield return new WaitForSeconds(attackDelay);

        attackHitboxActive = true;

        rb.linearVelocity = Vector3.right * 30f;

        Collider2D hit = Physics2D.OverlapCircle(transform.position + Vector3.right * attackRange, attackRadius);

        yield return new WaitForSeconds(attackMovementDuration);

        rb.linearVelocity = Vector3.zero;

        yield return new WaitForSeconds(attackRecoverDuration);

        attackHitboxActive = false;
        alreadyAttacking = false;

        state = BOSS_STATE.IDLE;
    }

    private IEnumerator HeavyAttackProcess()
    {
        alreadyHeavyAttacking = true;
        transform.localScale = new Vector3(3f, 3.2f, 3f);

        yield return new WaitForSeconds(heavyAttackDelay);

        heavyAttackHitboxActive = true;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, heavyAttackRadius);

        transform.localScale = new Vector3(3f, 2.8f, 3f);

        yield return new WaitForSeconds(heavyAttackRecoverDuration);

        transform.localScale = new Vector3(3f, 3f, 3f);

        heavyAttackHitboxActive = false;
        alreadyHeavyAttacking = false;

        state = BOSS_STATE.IDLE;
    }
}
