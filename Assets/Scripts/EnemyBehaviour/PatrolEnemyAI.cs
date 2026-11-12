using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PatrolEnemyAI : EnemyAI
{
    [Header("Movement:")]
    [SerializeField] private float speed = 2f;

    [Header("Edge Detection:")]
    [SerializeField] private Transform edgeCheck;

    [Header("Attack:")]
    [SerializeField] private int contactDamage = 10;

    #region MAIN LOOP

    private void Awake()
    {
        InitializeBasicComponents();
    }

    private void Start()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation.eulerAngles;
    }

    private void OnEnable()
    {
        characterStats.OnDeath += ToDead;
        characterStats.OnDamageTaken += FlashColor;
        characterStats.OnDamageTaken += AddKnockback;

        if (isDead)
        {
            ResetEnemy();
            invertedDirection = transform.rotation == Quaternion.Euler(0, 180, 0);
        }
    }

    private void OnDisable()
    {
        characterStats.OnDeath -= ToDead;
        characterStats.OnDamageTaken -= FlashColor;
        characterStats.OnDamageTaken -= AddKnockback;
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case ENEMY_STATE.PATROL:
                OnPatrol();
                break;

            case ENEMY_STATE.DEAD:
                break;
        }

        externalMovement = Vector3.Lerp(externalMovement, Vector3.zero, externalMovementDecay * Time.fixedDeltaTime);

        rb.linearVelocity = localMovement + externalMovement;
    }

    #endregion

    #region STATE BEHAVIOUR

    // ========================================================= FUNCIONES DE COMPORTAMIENTO DE ESTADO ========================================================= //

    // Comportamiento en estado PATROL
    private void OnPatrol()
    {
        //rb.linearVelocity = new Vector3((invertedDirection ? -1 : 1) * speed, rb.linearVelocity.y);
        localMovement = new Vector3((invertedDirection ? -1 : 1) * speed, rb.linearVelocity.y);

        if (!DetectedEdge())
        {
            Flip();
        }
    }

    #endregion

    #region STATE TRANSITION

    // ========================================================= FUNCIONES DE TRANSICIÓN DE ESTADO ========================================================= //

    // Comportamiento de transición a PATROL
    private void ToPatrol()
    {
        state = ENEMY_STATE.PATROL;
    }

    // Comportamiento de transición a DEAD
    protected void ToDead()
    {
        isDead = true;
        state = ENEMY_STATE.DEAD;
        gameObject.SetActive(false);
    }

    #endregion

    #region STATE CONDITION

    // ========================================================= FUNCIONES DE CONDICIÓN DE ESTADO ========================================================= //

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

    // Devuelve si esta detectando suelo delante suyo
    private bool DetectedEdge()
    {
        Debug.DrawRay(edgeCheck.position, Vector3.down, Color.red);
        return Physics2D.Raycast(edgeCheck.position, Vector3.down, 1f, groundLayer);
    }

    // Gira al enemigo si colisiona con un elemento del escenario
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Ground"))
        {
            ContactPoint2D contact = collision.GetContact(0);

            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                Flip();
            }
        }
    }

    public override void ResetEnemy()
    {
        if (!canRespawn) return;

        ResetColor();
        rb.linearVelocity = Vector3.zero;
        isDead = false;
        state = defaultState;
        characterStats.FullHeal();
        transform.position = spawnPosition;
        transform.rotation = Quaternion.Euler(spawnRotation);
        invertedDirection = transform.rotation == Quaternion.Euler(0, 180, 0);
    }

    #endregion
}
