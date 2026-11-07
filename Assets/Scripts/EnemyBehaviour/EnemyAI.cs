using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent (typeof (CharacterStats))]
public abstract class EnemyAI : MonoBehaviour
{
    protected enum ENEMY_STATE { IDLE, PATROL, CHASING, ATTACK, SEARCHING, STUNNED, DEAD }

    [Header("Debug:")]
    [SerializeField] protected ENEMY_STATE state;
    [SerializeField] protected ENEMY_STATE defaultState;
    [SerializeField] protected bool showGizmos = true;
    [SerializeField] protected bool canRespawn = true;

    protected Rigidbody2D rb;
    protected CharacterStats characterStats;

    protected void InitializeBasicComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        characterStats = GetComponent<CharacterStats>();
    }
}
