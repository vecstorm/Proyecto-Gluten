using UnityEngine;

#region Enums

/// <summary>
/// Equipo al que pertenece un personaje u objeto de combate, se utiliza para identificar quién puede dañar a quién.
/// </summary>
public enum CombatTeam
{
    Neutral,
    Player,
    Enemy
}

/// <summary>
/// Tipo de ataque que ha generado un impacto, permite distinguir entre ataques cuerpo a cuerpo, proyectiles y habilidades especiales.
/// </summary>
public enum CombatAttackType
{
    Melee,
    Projectile,
    Special
}

#endregion

#region Combat Damage

/// <summary>
/// Contiene toda la información necesaria para representar un impacto dentro del sistema de combate.
/// </summary>
public struct CombatDamage
{
    /// <summary>
    /// Cantidad de daño que produce el impacto.
    /// </summary>
    public float Amount;

    /// <summary>
    /// Equipo al que pertenece quien genera el daño.
    /// </summary>
    public CombatTeam SourceTeam;

    /// <summary>
    /// Transform del objeto que genera el impacto, puede utilizarse para identificar al atacante o proyectil.
    /// </summary>
    public Transform Source;

    /// <summary>
    /// Posición del impacto en el mundo, se utiliza para determinar desde qué dirección llega un ataque.
    /// </summary>
    public Vector2 HitPoint;

    /// <summary>
    /// Tipo de ataque que ha producido el impacto.
    /// </summary>
    public CombatAttackType AttackType;

    /// <summary>
    /// Indica si el ataque puede ser neutralizado mediante parry.
    /// </summary>
    public bool IsParryable;

    /// <summary>
    /// Constructor principal, permite especificar directamente el equipo y el objeto atacante.
    /// </summary>
    public CombatDamage(
        float amount,
        CombatTeam sourceTeam,
        Transform source,
        Vector2 hitPoint,
        CombatAttackType attackType = CombatAttackType.Melee,
        bool isParryable = true)
    {
        Amount = amount;
        SourceTeam = sourceTeam;
        Source = source;
        HitPoint = hitPoint;
        AttackType = attackType;
        IsParryable = isParryable;
    }

    /// <summary>
    /// Constructor que obtiene automáticamente el equipo y el Transform del atacante a partir de CharacterStats.
    /// </summary>
    public CombatDamage(
        float amount,
        CharacterStats stats,
        Vector2 hitPoint,
        CombatAttackType attackType = CombatAttackType.Melee,
        bool isParryable = true)
    {
        Amount = amount;
        SourceTeam = stats.Team;
        Source = stats.transform;
        HitPoint = hitPoint;
        AttackType = attackType;
        IsParryable = isParryable;
    }

    /// <summary>
    /// Variante del constructor que permite indicar primero si el ataque puede ser neutralizado mediante parry.
    /// </summary>
    public CombatDamage(
        float amount,
        CharacterStats stats,
        Vector2 hitPoint,
        bool isParryable,
        CombatAttackType attackType = CombatAttackType.Melee)
    {
        Amount = amount;
        SourceTeam = stats.Team;
        Source = stats.transform;
        HitPoint = hitPoint;
        AttackType = attackType;
        IsParryable = isParryable;
    }

    /// <summary>
    /// Variante que utiliza la posición del atacante como punto de impacto.
    /// </summary>
    public CombatDamage(
        float amount,
        CharacterStats stats,
        bool isParryable,
        CombatAttackType attackType = CombatAttackType.Melee)
    {
        Amount = amount;
        SourceTeam = stats.Team;
        Source = stats.transform;
        HitPoint = stats.transform.position;
        AttackType = attackType;
        IsParryable = isParryable;
    }
}

#endregion

#region Combat Interfaces

/// <summary>
/// Interfaz que debe implementar cualquier objeto que pueda recibir daño dentro del sistema de combate.
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Equipo al que pertenece el objetivo.
    /// </summary>
    CombatTeam Team { get; }

    /// <summary>
    /// Recibe y procesa un impacto.
    /// </summary>
    /// <param name="damage">Información del impacto recibido</param>
    /// <returns>True o false según el resultado de la operación de daño.</returns>
    bool ReceiveDamage(CombatDamage damage);
}

/// <summary>
/// Interfaz opcional para personajes u objetos que puedan recibir efectos de aturdimiento.
/// </summary>
public interface IStunnable
{
    /// <summary>
    /// Aplica el efecto de aturdimiento.
    /// </summary>
    void ApplyStun();
}

/// <summary>
/// Interfaz opcional para proyectiles que puedan ser reflejados mediante un parry.
/// </summary>
public interface IReflectableProjectile
{
    /// <summary>
    /// Refleja el proyectil y actualiza sus datos de propiedad.
    /// </summary>
    /// <param name="newOwner">Nuevo propietario del proyectil.</param>
    /// <param name="newTeam">Nuevo equipo al que pertenece.</param>
    /// <param name="direction">Nueva dirección de movimiento.</param>
    void Reflect(Transform newOwner, CombatTeam newTeam, Vector2 direction);
}

#endregion