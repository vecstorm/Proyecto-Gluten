
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona las mecánicas de combate del jugador.
/// Incluye el ataque básico, el parry, la invulnerabilidad y las herramientas
/// de depuración visual. No depende de las clases concretas de los enemigos.
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    #region Variables

    #region Components

    [Header("References")]

    [Tooltip("Estadísticas del jugador, se utiliza para consultar su vida y aplicar daño")]
    [SerializeField] private CharacterStats characterStats;

    [Tooltip("Punto desde el que se calcula la posición del ataque básico")]
    [SerializeField] private Transform attackOrigin;

    [Tooltip("Capas de colisión que pueden recibir el ataque básico")]
    [SerializeField] private LayerMask hittableLayers = ~0;

    private PlayerController playerController;

    #endregion

    #region Configuration

    [Header("Basic Attack")]

    [Tooltip("Cantidad de daño que inflige cada ataque básico")]
    [SerializeField, Min(0f)] private float basicAttackDamage = 2f;

    [Tooltip("Radio de la esfera de detección del ataque básico")]
    [SerializeField, Min(0f)] private float basicAttackRange = 1f;

    [Tooltip("Tiempo que debe pasar entre dos ataques básicos")]
    [SerializeField, Min(0f)] private float basicAttackCooldown = 0.3f;

    [Header("Invulnerability")]

    [Tooltip("Tiempo durante el que el jugador no puede recibir otro ataque después de recibir daño")]
    [SerializeField, Min(0f)] private float invulnerabilityDuration = 0.3f;

    #endregion

    #region State

    /// <summary>
    /// Evita que un mismo objetivo reciba varias veces el daño si tiene diferentes colliders dentro del área del ataque.
    /// </summary>
    private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

    /// <summary>
    /// Temporizador para el tiempo entre ataques.
    /// </summary>
    private float attackCooldownTimer;

    /// <summary>
    /// Temporizador para la invulnerabilidad.
    /// </summary>
    [SerializeField] private float invulnerabilityTimer;

    /// <summary>
    /// Dirección hacia la que mira el jugador, 1 = derecha, -1 = izquierda.
    /// </summary>
    private int facingDirection = 1;

    #endregion

    #region Properties


    /// <summary>
    /// Indica si el jugador se encuentra en un periodo de invulnerabilidad.
    /// </summary>
    public bool IsInvulnerable => invulnerabilityTimer > 0f;

    /// <summary>
    /// Indica si el jugador puede ejecutar otro ataque básico.
    /// </summary>
    public bool CanAttack => attackCooldownTimer <= 0f;

    #endregion

    #region Debug Configuration

    [Header("Debug Gizmos")]

    [Tooltip("Activa o desactiva la visualización del área de ataque en la Scene View")]
    [SerializeField] private bool showCombatGizmos = true;

    [Tooltip("Tiempo durante el que permanece visible el gizmo después de ejecutar un ataque")]
    [SerializeField, Min(0f)] private float gizmoDisplayDuration = 0.2f;

    [Tooltip("Color del gizmo que representa el área real del ataque básico")]
    [SerializeField] private Color attackGizmoColor = Color.red;

    /// <summary>
    /// Temporizador independiente para mantener visible el gizmo durante un breve periodo después de ejecutar el ataque.
    /// </summary>
    private float attackGizmoTimer;

    #endregion

    #endregion

    #region Unity Callbacks

    /// <summary>
    /// Inicializa las referencias necesarias para el combate.
    /// </summary>
    private void Awake()
    {
        // Busca automáticamente CharacterStats si no se ha asignado
        // desde el Inspector.
        if (characterStats == null)
            characterStats = GetComponent<CharacterStats>();

        // Si no existe un origen de ataque asignado,
        // utiliza la posición del propio jugador.
        if (attackOrigin == null)
            attackOrigin = transform;

        playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        characterStats.OnDamageTaken += ReceiveAttack;
    }

    private void OnDisable()
    {
        characterStats.OnDamageTaken -= ReceiveAttack;
    }

    /// <summary>
    /// Actualiza los temporizadores de las mecánicas de combate.
    /// </summary>
    private void Update()
    {
        // Cooldown del ataque básico
        attackCooldownTimer = Mathf.Max(0f, attackCooldownTimer - Time.deltaTime);

        // Duración de la invulnerabilidad
        invulnerabilityTimer = Mathf.Max(0f, invulnerabilityTimer - Time.deltaTime);

        if (!characterStats.GetCanTakeDamage())
        {
            if (invulnerabilityTimer <= 0f)
            {
                characterStats.SetCanTakeDamage(true);
            }
        }

        if (attackGizmoTimer >= 0f)
        {
            // Calcula el centro de la esfera desplazándola hacia delante
            // Se utiliza la mitad del alcance para situar el ataque entre
            // el jugador y su distancia máxima de impacto
            Vector2 center = (Vector2)attackOrigin.position
                + Vector2.right * facingDirection * (basicAttackRange * 0.5f);

            // Detecta todos los colliders que se encuentren dentro de la esfera.
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                center,
                basicAttackRange,
                hittableLayers
            );

            // Construye la información del daño que recibirán los objetivos.
            CombatDamage damage = new CombatDamage(
                basicAttackDamage,
                CombatTeam.Player,
                transform,
                center,
                CombatAttackType.Melee,
                false
            );

            CombatManager.MakeDamage(transform, hits, hitTargets, damage, CombatTeam.Player);
        }

        // Controla cuánto tiempo permanece visible el gizmo del ataque
        if (attackGizmoTimer > 0f)
            attackGizmoTimer -= Time.deltaTime;
    }

    #endregion

    #region Direction

    /// <summary>
    /// Actualiza la dirección hacia la que mira el jugador.
    /// Si cambia de dirección durante un parry, lo cancela inmediatamente.
    /// </summary>
    /// <param name="direction">Dirección del jugador: 1 para derecha y -1 para izquierda.</param>
    public void SetFacingDirection(int direction)
    {
        if (direction == 0)
            return;

        // Normaliza cualquier valor recibido a 1 o -1
        int newDirection = direction > 0 ? 1 : -1;

        // No es necesario hacer nada si la dirección no ha cambiado
        if (newDirection == facingDirection)
            return;

        facingDirection = newDirection;
    }

    #endregion

    #region Basic Attack

    /// <summary>
    /// Ejecuta el ataque básico del jugador.
    /// Detecta todos los objetivos golpeables dentro de una esfera situada delante
    /// del jugador y les aplica daño una única vez.
    /// </summary>
    /// <returns>True si el ataque se ha ejecutado correctamente.</returns>
    public bool PerformBasicAttack()
    {
        // Comprueba que el jugador puede atacar y sigue vivo
        if (!CanAttack || characterStats == null || characterStats.CurrentHealth <= 0f)
            return false;

        // Activa el gizmo y el cooldown del ataque
        attackGizmoTimer = gizmoDisplayDuration;
        attackCooldownTimer = basicAttackCooldown;

        // Limpia los objetivos del ataque anterior
        hitTargets.Clear();

        return true;
    }

    #endregion

    #region Parry

    /// <summary>
    /// Recibe un ataque y determina si debe ser neutralizado mediante parry
    /// o si debe aplicar daño al jugador.
    /// </summary>
    /// <param name="incomingDamage">Información del ataque recibido.</param>
    /// <returns>True si el jugador ha recibido daño.</returns>
    public void ReceiveAttack()
    {
        // Ignora ataques si el jugador no tiene estadísticas, está muerto
        // o se encuentra en un periodo de invulnerabilidad.
        if (characterStats == null || characterStats.CurrentHealth <= 0f || IsInvulnerable)
            return;

        playerController.ChangeState(PlayerState.Hurt);

        characterStats.SetCanTakeDamage(false);

        // Activa la invulnerabilidad para evitar recibir daño consecutivo.
        invulnerabilityTimer = invulnerabilityDuration;
        Debug.Log("Invulnerabilidad activada.");
    }

    #endregion

    #region Debug

    /// <summary>
    /// Dibuja en la Scene View el área real del ataque básico
    /// mientras su temporizador de visualización permanece activo.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showCombatGizmos || attackGizmoTimer <= 0f)
            return;

        Gizmos.color = attackGizmoColor;

        // Utiliza el origen configurado o el propio jugador como alternativa.
        Transform origin = attackOrigin != null ? attackOrigin : transform;

        // Mantiene exactamente el mismo centro y radio que OverlapCircleAll.
        Vector3 center = origin.position
            + Vector3.right * facingDirection * (basicAttackRange * 0.5f);

        Gizmos.DrawWireSphere(center, basicAttackRange);
    }

    #endregion
}