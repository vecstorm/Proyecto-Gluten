using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectileController : MonoBehaviour
{
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private float speed = 6f;

    private Rigidbody2D rb;
    private Vector3 targetPosition;
    private Vector3 direction;

    private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
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

        Destroy(gameObject);
    }

    public void SetMovement(float speed, Vector3 targetPosition)
    {
        this.speed = speed;

        direction = (targetPosition - transform.position).normalized;
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
