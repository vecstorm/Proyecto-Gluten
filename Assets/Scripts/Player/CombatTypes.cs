using UnityEngine;

/// <summary>
/// Equipo al que pertenece un personaje u objeto de combate.
/// </summary>
public enum CombatTeam
{
    Neutral,
    Player,
    Enemy
}

public enum CombatAttackType
{
    Melee,
    Projectile,
    Special
}

/// <summary>
/// Datos comunes de un impacto, independientemente de quién lo genere.
/// </summary>
public struct CombatDamage
{
    public float Amount;
    public CombatTeam SourceTeam;
    public Transform Source;
    public Vector2 HitPoint;
    public CombatAttackType AttackType;
    public bool IsParryable;

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

/// <summary>
/// Contrato que debe implementar cualquier objetivo que pueda recibir daño.
/// </summary>
public interface IDamageable
{
    CombatTeam Team { get; }
    bool ReceiveDamage(CombatDamage damage);
}

/// <summary>
/// Contrato opcional para personajes que puedan quedar aturdidos.
/// </summary>
public interface IStunnable
{
    void ApplyStun();
}

/// <summary>
/// Contrato opcional para proyectiles que puedan reflejarse con un parry.
/// </summary>
public interface IReflectableProjectile
{
    void Reflect(
        Transform newOwner,
        CombatTeam newTeam,
        Vector2 direction);
}