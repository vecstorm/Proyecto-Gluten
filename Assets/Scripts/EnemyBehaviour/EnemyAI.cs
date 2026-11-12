using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent (typeof (CharacterStats))]
public abstract class EnemyAI : MonoBehaviour
{
    protected enum ENEMY_STATE { IDLE, PATROL, PREPARING_CHASE, CHASING, ATTACK, SEARCHING, RAMMING, STUNNED, RETREAT, DEAD }

    [Header("Debug:")]
    [SerializeField] protected ENEMY_STATE state;
    [SerializeField] protected ENEMY_STATE defaultState;
    [SerializeField] protected bool showGizmos = true;
    [SerializeField] protected bool canRespawn = true;
    [SerializeField] protected bool isAerial = false;
    [SerializeField] protected float flashTimer = 0.2f;
    [SerializeField] protected GameObject stunnedIcon;
    [SerializeField] protected LayerMask playerDetectionLayer;

    [Header("Ground Detection:")]
    [SerializeField] protected Transform groundDetection;
    [SerializeField] protected LayerMask groundLayer;

    protected Vector3 spawnPosition;
    protected Vector3 spawnRotation;
    protected bool invertedDirection = false;
    protected bool isDead = false;
    protected Color rendererColor;

    protected Rigidbody2D rb;
    protected CharacterStats characterStats;
    protected readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

    protected void InitializeBasicComponents()
    {
        EnemyManager.EnemiesOnMap.Add(this);

        defaultState = state;

        rendererColor = GetComponent<SpriteRenderer>().color;
        invertedDirection = transform.rotation == Quaternion.Euler(0, 180, 0);
        rb = GetComponent<Rigidbody2D>();
        characterStats = GetComponent<CharacterStats>();
    }

    protected void Flip()
    {
        if (!GroundBeneath()) return;
            
        invertedDirection = !invertedDirection;

        transform.Rotate(0, 180, 0);
    }

    private bool GroundBeneath()
    {
        if (isAerial) return true;

        Debug.DrawRay(groundDetection.position, Vector3.down, Color.red);
        return Physics2D.Raycast(groundDetection.position, Vector3.down, 1f, groundLayer);
    }

    protected void FlashColor()
    {
        StartCoroutine(FlashSequence());
    }

    private IEnumerator FlashSequence()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.color = Color.white;
        }

        yield return new WaitForSeconds(flashTimer);

        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.color = rendererColor;
        }
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

    protected void ResetColor()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.color = rendererColor;
        }
    }

    public abstract void ResetEnemy();
}
