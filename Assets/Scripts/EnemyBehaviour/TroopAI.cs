using UnityEngine;

public abstract class TroopAI : EnemyAI
{
    protected enum ENEMY_STATE { IDLE, PATROL, PREPARING_CHASE, CHASING, ATTACK, SEARCHING, RAMMING, STUNNED, RETREAT, DEAD }

    [Header("States:")]
    [SerializeField] protected ENEMY_STATE state;
    [SerializeField] protected ENEMY_STATE defaultState;

    [Header("Ground Detection:")]
    [SerializeField] protected Transform groundDetection;
    [SerializeField] protected LayerMask groundLayer;

    [Header("Knockback:")]
    [SerializeField] protected bool horizontalKnockback = true;
    [SerializeField] protected bool verticalKnockback = false;
    [SerializeField] protected float knockbackForce = 5f;
    [SerializeField] protected float externalMovementDecay = 5f;

    protected override void InitializeBasicComponents()
    {
        EnemyManager.EnemiesOnMap.Add(this);

        defaultState = state;

        rendererColor = GetComponent<SpriteRenderer>().color;
        invertedDirection = transform.rotation == Quaternion.Euler(0, 180, 0);
        rb = GetComponent<Rigidbody2D>();
        characterStats = GetComponent<CharacterStats>();
    }

    protected override void Flip()
    {
        if (!GroundBeneath())
        {
            //Debug.Log(gameObject.name + " tryied to flip on air");
            tryiedFlipAirborne = true;
            return;
        }

        invertedDirection = !invertedDirection;

        transform.Rotate(0, 180, 0);
    }

    protected bool GroundBeneath()
    {
        if (isAerial) return true;

        Debug.DrawRay(groundDetection.position, Vector3.down, Color.red);
        return Physics2D.Raycast(groundDetection.position, Vector3.down, 1f, groundLayer);
    }

    protected bool ObstacleBlocksTarget(Vector3 targetPosition, float distance)
    {
        Vector3 targetDirection = targetPosition - transform.position;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, targetDirection, distance, playerDetectionLayer);

        if (hit)
        {
            Debug.DrawRay(transform.position, targetDirection, Color.blue);

            if (hit.collider.CompareTag("Player"))
            {
                return false;
            }
        }

        return true;
    }

    protected void AddKnockback()
    {
        Vector3 knockbackDirection = (transform.position - characterStats.LastDamagePosition).normalized;

        ParticleManager.SpawnParticles("AttackImpact", transform.position, new Vector3(knockbackDirection.x, 0, 0));

        externalMovement = new Vector3(horizontalKnockback ? knockbackDirection.x : 0, verticalKnockback ? knockbackDirection.y : 0, 0) * knockbackForce;
    }
}
