using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectileController : MonoBehaviour
{
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private float speed = 6f;
    [SerializeField] private string[] contactTags;
    [SerializeField] private bool usesGravity = false;

    private Rigidbody2D rb;
    private Vector3 targetPosition;
    private Vector3 direction;

    private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (usesGravity) return;

        rb.linearVelocity = direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            hitTargets.Clear();

            CombatDamage damage = new CombatDamage(contactDamage, CombatTeam.Enemy, transform, transform.position, CombatAttackType.Projectile, false);
            CombatManager.MakeDamage(transform, collision, hitTargets, damage, CombatTeam.Enemy);
        }

        if (contactTags.Contains(collision.tag))
        {
            Destroy(gameObject);
        }
    }

    public void SetMovement(float speed, Vector3 targetPosition)
    {
        this.speed = speed;

        direction = (targetPosition - transform.position).normalized;
    }

    public void SetLinearDirection(float speed, Vector3 targetDirection)
    {
        if (rb != null)
        {
            rb.gravityScale = 0;
        }
        
        this.speed = speed;
        direction = targetDirection;
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    public void SetTargetPosition(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }
}
