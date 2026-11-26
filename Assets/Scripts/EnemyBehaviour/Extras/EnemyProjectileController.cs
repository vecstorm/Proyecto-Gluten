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

    [Header("Particles:")]
    [SerializeField] private ParticleSystem followParticlesPrefab;
    [SerializeField] private ParticleSystem impactParticlesPrefab;
    [SerializeField] private bool onSurface = false;
    [SerializeField] private LayerMask collisionLayer;

    private Rigidbody2D rb;
    private Vector3 targetPosition;
    private Vector3 direction;
    private ParticleSystem followParticles;

    private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (followParticlesPrefab != null)
        {
            followParticles = Instantiate(followParticlesPrefab, transform);
            followParticles.transform.localScale = Vector3.one;
        }
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
            if (followParticles != null)
            {
                followParticles.transform.SetParent(null);
                followParticles.transform.localScale = Vector3.one;
                followParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            if (impactParticlesPrefab != null)
            {
                if (onSurface)
                {
                    Vector3 surfaceDirection = ((Vector3) collision.ClosestPoint(transform.position) - transform.position).normalized;

                    RaycastHit2D hit = Physics2D.Raycast(transform.position, surfaceDirection, 1f, collisionLayer);

                    if (hit.collider)
                    {
                        Debug.Log("DETECTED GROUND");

                        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                        ParticleSystem particle = Instantiate(impactParticlesPrefab, hit.point, rotation);
                    }
                }
                else
                {
                    Instantiate(impactParticlesPrefab, transform.position, Quaternion.identity);
                }
            }

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

    public void SetContactDamage(int contactDamage)
    {
        this.contactDamage = contactDamage;
    }

    private Vector3 GetCollisionDirection(Vector3 normal)
    {
        if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y))
        {
            return normal.x > 0 ? Vector3.right : Vector3.left;
        }
        else
        {
            return normal.y > 0 ? Vector3.up : Vector3.down;
        }
    }
}
