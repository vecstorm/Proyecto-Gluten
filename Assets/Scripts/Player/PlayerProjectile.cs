
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona el comportamiento de un proyectil mágico del jugador, atraviesa enemigos, pero desaparece al alcanzar un obstáculo.
/// </summary>
public class PlayerProjectile : MonoBehaviour
{
    #region Configuration

    [Header("Projectile")]

    [Tooltip("Daño que inflige el proyectil")]
    [SerializeField, Min(0f)] private float damage = 2f;

    [Tooltip("Velocidad de desplazamiento")]
    [SerializeField, Min(0f)] private float speed = 18f;

    [Tooltip("Distancia máxima que puede recorrer")]
    [SerializeField, Min(0f)] private float maxDistance = 12f;

    [Tooltip("Radio utilizado para detectar colisiones")]
    [SerializeField, Min(0f)] private float collisionRadius = 0.25f;

    [Header("Collision")]

    [Tooltip("Capas que contienen objetivos que pueden recibir daño")]
    [SerializeField] private LayerMask hittableLayers;

    [Tooltip("Capas que detienen el proyectil, como paredes y obstáculos")]
    [SerializeField] private LayerMask obstacleLayers;

    #endregion

    #region State

    /// <summary>
    /// Propietario del proyectil.
    /// </summary>
    private Transform owner;

    /// <summary>
    /// Dirección de movimiento del proyectil.
    /// </summary>
    private Vector2 direction;

    /// <summary>
    /// Distancia recorrida por el proyectil.
    /// </summary>
    private float distanceTravelled;

    /// <summary>
    /// Evita que un mismo objetivo reciba daño varias veces.
    /// </summary>
    private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

    #endregion

    #region Initialization

    /// <summary>
    /// Inicializa el proyectil con su propietario y dirección de movimiento.
    /// </summary>
    public void Initialize(Transform projectileOwner, Vector2 projectileDirection)
    {
        owner = projectileOwner;

        direction = projectileDirection.normalized;

        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.right;

        // Orienta el proyectil hacia su dirección de desplazamiento
        transform.right = direction;
    }

    #endregion

    #region Unity Callbacks

    private void FixedUpdate()
    {
        MoveProjectile();
    }

    #endregion

    #region Movement

    /// <summary>
    /// Desplaza el proyectil y comprueba los objetivos y obstáculos de su trayectoria.
    /// </summary>
    private void MoveProjectile()
    {
        float remainingDistance = maxDistance - distanceTravelled;

        if (remainingDistance <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        float stepDistance = Mathf.Min(speed * Time.fixedDeltaTime, remainingDistance);

        Vector2 currentPosition = transform.position;

        // Comprueba si existe un obstáculo en la trayectoria
        RaycastHit2D obstacleHit = Physics2D.CircleCast(currentPosition, collisionRadius, direction, stepDistance, obstacleLayers);

        float allowedDistance = obstacleHit.collider != null ? obstacleHit.distance : stepDistance;

        // Detecta todos los objetivos que atraviesa durante este desplazamiento
        RaycastHit2D[] hits = Physics2D.CircleCastAll(currentPosition, collisionRadius, direction, allowedDistance, hittableLayers);

        // Ordena los impactos según su distancia al proyectil
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // Aplica daño a todos los objetivos alcanzados
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            CombatDamage projectileDamage = new CombatDamage( damage, CombatTeam.Player, owner, hit.point, CombatAttackType.Projectile, false);

            CombatManager.MakeDamage(owner, hit.collider, hitTargets, projectileDamage, CombatTeam.Player);
        }

        // Si encuentra un obstáculo, termina su recorrido
        if (obstacleHit.collider != null)
        {
            transform.position = currentPosition + direction * obstacleHit.distance;
            Destroy(gameObject);
            return;
        }

        // Continúa avanzando si no ha encontrado obstáculos
        transform.position = currentPosition + direction * stepDistance;

        distanceTravelled += stepDistance;

        if (distanceTravelled >= maxDistance)
            Destroy(gameObject);
    }

    #endregion
}