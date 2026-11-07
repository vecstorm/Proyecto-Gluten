using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent (typeof (CharacterStats))]
public abstract class EnemyAI : MonoBehaviour
{
    protected enum ENEMY_STATE { IDLE, PATROL, CHASING, ATTACK, SEARCHING, RAMMING, STUNNED, DEAD }

    [Header("Debug:")]
    [SerializeField] protected ENEMY_STATE state;
    [SerializeField] protected ENEMY_STATE defaultState;
    [SerializeField] protected bool showGizmos = true;
    [SerializeField] protected bool canRespawn = true;

    protected Vector3 spawnPosition;
    protected Vector3 spawnRotation;
    protected bool invertedDirection = false;
    protected bool isDead = false;

    protected Rigidbody2D rb;
    protected CharacterStats characterStats;
    protected readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

    protected void InitializeBasicComponents()
    {
        defaultState = state;

        invertedDirection = transform.rotation == Quaternion.Euler(0, 180, 0);
        rb = GetComponent<Rigidbody2D>();
        characterStats = GetComponent<CharacterStats>();
    }

    protected void Flip()
    {
        invertedDirection = !invertedDirection;

        transform.Rotate(0, 180, 0);
    }

    protected abstract void ResetEnemy();
}
