using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona las mecánicas de combate del jugador.
/// Incluye el ataque básico, la recepción de daño, la invulnerabilidad y las herramientas de depuración visual.
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    #region Variables

    #region Components

    [Header("References")]

    [Tooltip("Punto desde el que se calcula la posición del ataque básico")]
    [SerializeField] private Transform attackOrigin;

    [Tooltip("Capas de colisión que pueden recibir el ataque básico")]
    [SerializeField] private LayerMask hittableLayers = ~0;

    /// <summary>
    /// Estadísticas del jugador, se utilizan para consultar su vida, comprobar si puede recibir daño y gestionar los impactos.
    /// </summary>
    private CharacterStats characterStats;

    /// <summary>
    /// Referencia al controlador del jugador, se utiliza para cambiar su estado cuando recibe daño.
    /// </summary>
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

    [Tooltip("Tiempo de duración de la detección del ataque")]
    [SerializeField, Min(0f)] private float basicAttackDuration = 0.2f;

    /// <summary>
    /// Temporizador que controla cuánto tiempo permanece activo el ataque y visible su representación para debug.
    /// </summary>
    private float basicAttackTimer;

    [Header("Ranged Attack")]

    [Tooltip("Prefab del proyectil mágico")]
    [SerializeField] private PlayerProjectile projectilePrefab;

    [Tooltip("Punto desde el que aparecerá el proyectil")]
    [SerializeField] private Transform projectileSpawnPoint;

    [Tooltip("Tiempo de espera entre ataques a distancia")]
    [SerializeField, Min(0f)] private float rangedAttackCooldown = 0.5f;

    /// <summary>
    /// Tiempo restante hasta poder realizar otro ataque a distancia.
    /// </summary>
    private float rangedAttackCooldownTimer;

    [Header("Invulnerability")]

    [Tooltip("Tiempo durante el que el jugador no puede recibir otro ataque después de recibir daño")]
    [SerializeField, Min(0f)] private float invulnerabilityDuration = 0.3f;

    #endregion

    #region State

    /// <summary>
    /// Evita que un mismo objetivo reciba varias veces el daño si tiene diferentes colliders dentro del área del ataque.
    /// Se limpia al comenzar cada ataque.
    /// </summary>
    private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

    /// <summary>
    /// Temporizador que controla el tiempo restante hasta poder ejecutar otro ataque básico.
    /// </summary>
    private float attackCooldownTimer;

    /// <summary>
    /// Temporizador de invulnerabilidad.
    /// </summary>
    [SerializeField] private float invulnerabilityTimer;

    /// <summary>
    /// Dirección hacia la que mira el jugador: 1 = derecha, -1 = izquierda.
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

    /// <summary>
    /// Indica si el jugador puede ejecutar otro ataque a distancia.
    /// </summary>
    public bool CanRangedAttack => rangedAttackCooldownTimer <= 0f;

    #endregion

    #region Debug Configuration

    [Header("Debug Gizmos")]

    [Tooltip("Activa o desactiva la visualización del área de ataque para debug")]
    [SerializeField] private bool showCombatGizmos = true;

    [Tooltip("Color del gizmo que representa el área real del ataque básico")]
    [SerializeField] private Color attackGizmoColor = Color.red;

    #endregion

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        characterStats = GetComponent<CharacterStats>();

        playerController = GetComponent<PlayerController>();

        if (attackOrigin == null)
            attackOrigin = transform;
    }

    private void OnEnable()
    {
        characterStats.OnDamageTaken += ReceiveAttack;
    }

    private void OnDisable()
    {
        characterStats.OnDamageTaken -= ReceiveAttack;
    }

    private void Update()
    {
        // Reduce el tiempo restante del cooldown del ataque básico y del ataque a distancia
        attackCooldownTimer = Mathf.Max(0f, attackCooldownTimer - Time.deltaTime);
        rangedAttackCooldownTimer = Mathf.Max(0f, rangedAttackCooldownTimer - Time.deltaTime);

        // Reduce el tiempo restante de invulnerabilidad
        invulnerabilityTimer = Mathf.Max(0f, invulnerabilityTimer - Time.deltaTime);

        // Vuelve a habilitar el daño cuando termina el periodo de invulnerabilidad
        if (!characterStats.GetCanTakeDamage())
        {
            if (invulnerabilityTimer <= 0f)
            {
                characterStats.SetCanTakeDamage(true);
            }
        }

        // Mientras el temporizador del ataque esté activo, continúa detectando los objetivos dentro del área de impacto
        if (basicAttackTimer > 0f)
        {
            // Calcula el centro de la esfera desplazándola hacia delante
            Vector2 center = (Vector2)attackOrigin.position + Vector2.right * facingDirection * (basicAttackRange * 0.5f);

            // Detecta todos los colliders que se encuentren dentro de la esfera
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, basicAttackRange, hittableLayers);

            // Construye la información del daño que recibirán los objetivos
            CombatDamage damage = new CombatDamage(basicAttackDamage, CombatTeam.Player, transform, center, CombatAttackType.Melee, false);

            // Aplica el daño a los objetivos detectados
            CombatManager.MakeDamage(transform, hits, hitTargets, damage, CombatTeam.Player);
        }

        if (basicAttackTimer > 0f)
            basicAttackTimer -= Time.deltaTime;
    }

    #endregion

    #region Direction

    /// <summary>
    /// Actualiza la dirección hacia la que mira el jugador.
    /// </summary>
    /// <param name="direction">Dirección del jugador: 1 para derecha y -1 para izquierda</param>
    public void SetFacingDirection(int direction)
    {
        if (direction == 0)
            return;

        int newDirection = direction > 0 ? 1 : -1;

        if (newDirection == facingDirection)
            return;

        facingDirection = newDirection;
    }

    #endregion

    #region Attacks

    /// <summary>
    /// Inicia el ataque básico del jugador. Activa su ventana de detección, inicia el cooldown y limpia los objetivos registrados del ataque anterior.
    /// </summary>
    /// <returns>True si el ataque se ha iniciado correctamente; false si el jugador no puede atacar.</returns>
    public bool PerformBasicAttack()
    {
        if (!CanAttack || characterStats == null || characterStats.CurrentHealth <= 0f)
            return false;

        // Activa el temporizador que controla la detección del ataque y la visualización de su gizmo
        basicAttackTimer = basicAttackDuration;

        // Inicia el cooldown para impedir que se ejecute otro ataque antes de que transcurra el tiempo configurado
        attackCooldownTimer = basicAttackCooldown;

        // Limpia los objetivos registrados durante el ataque anterior
        hitTargets.Clear();

        return true;
    }

    /// <summary>
    /// Lanza un proyectil mágico en la dirección hacia la que mira el jugador.
    /// </summary>
    /// <returns>True si el proyectil se ha creado correctamente.</returns>
    public bool PerformRangedAttack()
    {
        if (!CanRangedAttack || projectilePrefab == null || characterStats == null || characterStats.CurrentHealth <= 0f)
            return false;

        // Utiliza el punto configurado o la posición del jugador como alternativa
        Vector3 spawnPosition = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;

        // Crea el proyectil
        PlayerProjectile projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        // Lo inicializa con la dirección actual del jugador
        projectile.Initialize(transform, Vector2.right * facingDirection);

        // Inicia el cooldown independiente
        rangedAttackCooldownTimer = rangedAttackCooldown;

        return true;
    }

    #endregion

    #region Damage Reception

    /// <summary>
    /// Gestiona la recepción de daño del jugador. Comprueba si puede recibir el impacto, cambia su estado y activa el periodo de invulnerabilidad.
    /// </summary>
    private void ReceiveAttack()
    {
        if (characterStats == null || characterStats.CurrentHealth <= 0f || IsInvulnerable)
            return;

        // Cambia el estado del jugador para reproducir el comportamiento correspondiente al recibir daño
        playerController.ChangeState(PlayerState.Hurt);

        // Impide que CharacterStats acepte nuevos impactos durante el periodo de invulnerabilidad
        characterStats.SetCanTakeDamage(false);

        // Activa el temporizador de invulnerabilidad
        invulnerabilityTimer = invulnerabilityDuration;
    }

    #endregion

    #region Debug

    /// <summary>
    /// Dibuja en la escena el área real del ataque básico mientras su temporizador de visualización permanece activo.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showCombatGizmos || basicAttackTimer <= 0f)
            return;

        Gizmos.color = attackGizmoColor;

        // Utiliza el origen configurado o el propio jugador como alternativa
        Transform origin = attackOrigin != null ? attackOrigin : transform;

        // Calcula el centro utilizando la misma fórmula que se emplea en OverlapCircleAll
        Vector3 center = origin.position + Vector3.right * facingDirection * (basicAttackRange * 0.5f);

        // Dibuja el contorno de la esfera que representa el área de ataque
        Gizmos.DrawWireSphere(center, basicAttackRange);
    }

    #endregion
}