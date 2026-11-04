
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

    #endregion

    #region Configuration

    [Header("Basic Attack")]

    [Tooltip("Cantidad de daño que inflige cada ataque básico")]
    [SerializeField, Min(0f)] private float basicAttackDamage = 2f;

    [Tooltip("Radio de la esfera de detección del ataque básico")]
    [SerializeField, Min(0f)] private float basicAttackRange = 1f;

    [Tooltip("Tiempo que debe pasar entre dos ataques básicos")]
    [SerializeField, Min(0f)] private float basicAttackCooldown = 0.3f;

    [Header("Parry")]

    [Tooltip("Duración de la ventana durante la que el jugador puede realizar un parry")]
    [SerializeField, Min(0f)] private float parryWindow = 0.15f;

    [Tooltip("Tiempo que debe pasar antes de poder iniciar otro parry")]
    [SerializeField, Min(0f)] private float parryCooldown = 0.5f;

    [Tooltip("Duración del aturdimiento aplicado a un enemigo cuerpo a cuerpo tras un parry exitoso")]
    [SerializeField, Min(0f)] private float meleeParryStunDuration = 0.5f;

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
    private float invulnerabilityTimer;

    /// <summary>
    /// Temporizador para el parry.
    /// </summary>
    private float parryTimer;

    /// <summary>
    /// Temporizador para el tiempo entre "parrys".
    /// </summary>
    private float parryCooldownTimer;

    /// <summary>
    /// Indica si el jugador se encuentra actualmente dentro de la ventana temporal de parry.
    /// </summary>
    private bool isParrying;

    /// <summary>
    /// Dirección hacia la que mira el jugador, 1 = derecha, -1 = izquierda.
    /// </summary>
    private int facingDirection = 1;

    /// <summary>
    /// Dirección en la que se inició el parry, si el jugador cambia de dirección, el parry se cancela.
    /// </summary>
    private int parryFacingDirection;

    #endregion

    #region Properties

    /// <summary>
    /// Indica si el jugador está realizando un parry.
    /// </summary>
    public bool IsParrying => isParrying;

    /// <summary>
    /// Indica si el jugador se encuentra en un periodo de invulnerabilidad.
    /// </summary>
    public bool IsInvulnerable => invulnerabilityTimer > 0f;

    /// <summary>
    /// Indica si el jugador puede ejecutar otro ataque básico.
    /// </summary>
    public bool CanAttack => attackCooldownTimer <= 0f;

    /// <summary>
    /// Indica si el jugador puede iniciar otro parry.
    /// </summary>
    public bool CanParry => parryCooldownTimer <= 0f;

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
    }

    /// <summary>
    /// Actualiza los temporizadores de las mecánicas de combate.
    /// </summary>
    private void Update()
    {
        // Cooldown del ataque básico.
        attackCooldownTimer = Mathf.Max(0f, attackCooldownTimer - Time.deltaTime);

        // Duración de la invulnerabilidad.
        invulnerabilityTimer = Mathf.Max(0f, invulnerabilityTimer - Time.deltaTime);

        // Duración de la ventana de parry.
        parryTimer = Mathf.Max(0f, parryTimer - Time.deltaTime);

        // Cooldown entre intentos de parry.
        parryCooldownTimer = Mathf.Max(0f, parryCooldownTimer - Time.deltaTime);

        // Finaliza automáticamente el parry cuando termina su ventana temporal.
        if (isParrying && parryTimer <= 0f)
            isParrying = false;

        // Controla cuánto tiempo permanece visible el gizmo del ataque.
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

        // Normaliza cualquier valor recibido a 1 o -1.
        int newDirection = direction > 0 ? 1 : -1;

        // No es necesario hacer nada si la dirección no ha cambiado.
        if (newDirection == facingDirection)
            return;

        facingDirection = newDirection;

        // El parry solo protege hacia la dirección en la que comenzó.
        // Cambiar de dirección cancela la ventana para evitar cubrir ambos lados.
        if (isParrying && facingDirection != parryFacingDirection)
            EndParry();
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
        // Comprueba que el jugador puede atacar y sigue vivo.
        if (!CanAttack || characterStats == null || characterStats.CurrentHealth <= 0f)
            return false;

        // Activa el gizmo y el cooldown del ataque.
        attackGizmoTimer = gizmoDisplayDuration;
        attackCooldownTimer = basicAttackCooldown;

        // Limpia los objetivos del ataque anterior.
        hitTargets.Clear();

        // Calcula el centro de la esfera desplazándola hacia delante.
        // Se utiliza la mitad del alcance para situar el ataque entre
        // el jugador y su distancia máxima de impacto.
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

        // Procesa todos los colliders detectados.
        foreach (Collider2D hit in hits)
        {
            // Ignora colliders nulos y cualquier collider perteneciente
            // al propio jugador.
            if (hit == null || hit.transform.root == transform.root)
                continue;

            // Busca el componente que permite recibir daño en el objeto
            // o en alguno de sus padres.
            IDamageable damageable = FindDamageable(hit);

            // Ignora objetos que no reciben daño y objetivos del equipo jugador.
            if (damageable == null || damageable.Team == CombatTeam.Player)
                continue;

            // HashSet.Add devuelve false si el objetivo ya estaba registrado.
            // Así evitamos aplicar daño varias veces a un mismo personaje
            // que tenga varios colliders dentro del área de ataque.
            if (hitTargets.Add(damageable))
                damageable.ReceiveDamage(damage);
        }

        return true;
    }

    #endregion

    #region Parry

    /// <summary>
    /// Intenta iniciar una ventana de parry.
    /// El parry solo puede bloquear ataques parryables que lleguen desde delante.
    /// </summary>
    /// <returns>True si el parry se ha iniciado correctamente.</returns>
    public bool BeginParry()
    {
        // Comprueba que el cooldown ha terminado y que el jugador sigue vivo.
        if (!CanParry || characterStats == null || characterStats.CurrentHealth <= 0f)
            return false;

        isParrying = true;

        // Guarda la dirección inicial para impedir que el jugador
        // pueda girar y cubrir ambos lados con el mismo parry.
        parryFacingDirection = facingDirection;

        // Inicia la ventana temporal y su cooldown.
        parryTimer = parryWindow;
        parryCooldownTimer = parryCooldown;

        return true;
    }

    /// <summary>
    /// Cancela la ventana de parry sin modificar su cooldown.
    /// </summary>
    public void EndParry()
    {
        isParrying = false;
        parryTimer = 0f;
    }

    /// <summary>
    /// Recibe un ataque y determina si debe ser neutralizado mediante parry
    /// o si debe aplicar daño al jugador.
    /// </summary>
    /// <param name="incomingDamage">Información del ataque recibido.</param>
    /// <returns>True si el jugador ha recibido daño.</returns>
    public bool ReceiveAttack(CombatDamage incomingDamage)
    {
        // Ignora ataques si el jugador no tiene estadísticas, está muerto
        // o se encuentra en un periodo de invulnerabilidad.
        if (characterStats == null || characterStats.CurrentHealth <= 0f || IsInvulnerable)
            return false;

        // Comprueba que el parry sigue activo, que el ataque es bloqueable
        // y que procede de la dirección hacia la que mira el jugador.
        if (isParrying &&
            parryTimer > 0f &&
            incomingDamage.IsParryable &&
            IsAttackInFront(incomingDamage))
        {
            ResolveParry(incomingDamage);
            return false;
        }

        // Si no se ha podido realizar el parry, el jugador recibe el daño completo.
        characterStats.TakeDamage(incomingDamage.Amount);

        // Activa la invulnerabilidad para evitar recibir daño consecutivo.
        invulnerabilityTimer = invulnerabilityDuration;

        return true;
    }

    /// <summary>
    /// Comprueba si el ataque recibido procede del lado hacia el que mira el jugador.
    /// No tiene límite de distancia: únicamente comprueba la dirección.
    /// </summary>
    /// <param name="incomingDamage">Información del ataque recibido.</param>
    /// <returns>True si el ataque procede de delante del jugador.</returns>
    private bool IsAttackInFront(CombatDamage incomingDamage)
    {
        // Utiliza el punto de impacto del ataque para determinar su procedencia.
        Vector2 attackPosition = incomingDamage.HitPoint;

        Vector2 directionToAttack = attackPosition - (Vector2)transform.position;

        // Si el punto de impacto coincide prácticamente con la posición
        // del jugador, utiliza la posición del atacante como referencia.
        if (directionToAttack.sqrMagnitude < 0.001f &&
            incomingDamage.Source != null)
        {
            directionToAttack =
                (Vector2)incomingDamage.Source.position - (Vector2)transform.position;
        }

        // Si no se puede determinar la procedencia del ataque,
        // no se considera válido para realizar un parry.
        if (directionToAttack.sqrMagnitude < 0.001f)
            return false;

        // El producto escalar permite comprobar si el ataque procede
        // del mismo semiplano hacia el que mira el jugador.
        // Un resultado positivo indica que está delante.
        Vector2 facing = Vector2.right * facingDirection;

        return Vector2.Dot(facing, directionToAttack.normalized) > 0f;
    }

    /// <summary>
    /// Resuelve las consecuencias de un parry exitoso según el tipo de ataque.
    /// </summary>
    /// <param name="incomingDamage">Información del ataque neutralizado.</param>
    private void ResolveParry(CombatDamage incomingDamage)
    {
        // Consume inmediatamente la ventana para impedir que un mismo parry
        // neutralice varios ataques.
        EndParry();

        // Parry contra proyectiles:
        // intenta localizar un componente que implemente IReflectableProjectile
        // y le solicita cambiar de propietario y equipo.
        if (incomingDamage.AttackType == CombatAttackType.Projectile &&
            incomingDamage.Source != null)
        {
            IReflectableProjectile projectile =
                FindInterface<IReflectableProjectile>(incomingDamage.Source);

            if (projectile != null)
            {
                // Calcula la dirección de reflexión utilizando la posición
                // del proyectil respecto al jugador.
                Vector2 direction = incomingDamage.Source.position - transform.position;

                // Si ambas posiciones coinciden, utiliza una dirección alternativa
                // para evitar enviar un vector nulo al proyectil.
                direction = direction.sqrMagnitude > 0.001f
                    ? direction.normalized
                    : Vector2.right * -facingDirection;

                projectile.Reflect(transform, CombatTeam.Player, direction);
            }
        }
        // Parry contra ataques cuerpo a cuerpo:
        // intenta localizar un componente que implemente IStunnable
        // y le aplica la duración de aturdimiento configurada.
        else if (incomingDamage.AttackType == CombatAttackType.Melee &&
                 incomingDamage.Source != null)
        {
            IStunnable stunnable = FindInterface<IStunnable>(incomingDamage.Source);

            if (stunnable != null)
                stunnable.ApplyStun(meleeParryStunDuration);
        }
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Busca un componente que implemente IDamageable en el collider recibido
    /// o en alguno de sus objetos padre.
    /// </summary>
    /// <param name="collider">Collider del objetivo detectado.</param>
    /// <returns>El componente IDamageable encontrado o null si no existe.</returns>
    private static IDamageable FindDamageable(Collider2D collider)
    {
        MonoBehaviour[] behaviours = collider.GetComponentsInParent<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IDamageable damageable)
                return damageable;
        }

        return null;
    }

    /// <summary>
    /// Busca un componente que implemente una interfaz concreta
    /// en el objeto atacante recibido o en alguno de sus padres.
    /// </summary>
    /// <typeparam name="T">Interfaz que se desea encontrar.</typeparam>
    /// <param name="source">Transform del objeto atacante.</param>
    /// <returns>El componente encontrado o null si no existe.</returns>
    private static T FindInterface<T>(Transform source)
        where T : class
    {
        MonoBehaviour[] behaviours = source.GetComponentsInParent<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is T component)
                return component;
        }

        return null;
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