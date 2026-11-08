using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent (typeof (CharacterStats))]
public abstract class EnemyAI : MonoBehaviour
{
    protected enum ENEMY_STATE { IDLE, PATROL, CHASING, ATTACK, SEARCHING, RAMMING, STUNNED, RETREAT, DEAD }

    [Header("Debug:")]
    [SerializeField] protected ENEMY_STATE state;
    [SerializeField] protected ENEMY_STATE defaultState;
    [SerializeField] protected bool showGizmos = true;
    [SerializeField] protected bool canRespawn = true;
    [SerializeField] protected float flashTimer = 0.2f;

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
        defaultState = state;

        rendererColor = GetComponent<SpriteRenderer>().color;
        invertedDirection = transform.rotation == Quaternion.Euler(0, 180, 0);
        rb = GetComponent<Rigidbody2D>();
        characterStats = GetComponent<CharacterStats>();
    }

    protected void Flip()
    {
        invertedDirection = !invertedDirection;

        transform.Rotate(0, 180, 0);
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

    protected abstract void ResetEnemy();
}
