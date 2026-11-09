using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Permite localizar objetivos que reciben daño y aplicar impactos sin depender de las clases de los personajes.
/// </summary>
public static class CombatManager
{
    #region Damage Functions

    /// <summary>
    /// Aplica daño a varios colliders detectados, evitando dañar al propio atacante, a objetivos del mismo equipo y aplicar daño repetido a un mismo objetivo.
    /// </summary>
    /// <param name="attacker">Transform del atacante</param>
    /// <param name="hits">Colliders detectados por el ataque</param>
    /// <param name="hitTargets">Conjunto de objetivos que ya han recibido daño durante el ataque</param>
    /// <param name="damage">Información del impacto</param>
    /// <param name="team">Equipo que debe excluirse del daño</param>
    public static void MakeDamage(Transform attacker, Collider2D[] hits, HashSet<IDamageable> hitTargets, CombatDamage damage, CombatTeam team = CombatTeam.Enemy)
    {
        // Recorre todos los colliders detectados por el ataque
        foreach (Collider2D hit in hits)
        {
            MakeDamage(attacker, hit, hitTargets, damage, team);
        }
    }

    /// <summary>
    /// Aplica daño a un único collider detectado, comprueba que el objetivo sea válido y que no haya recibido daño previamente durante el mismo ataque.
    /// </summary>
    /// <param name="attacker">Transform del atacante</param>
    /// <param name="hit">Collider del objetivo detectado</param>
    /// <param name="hitTargets">Conjunto de objetivos que ya han recibido daño durante el ataque</param>
    /// <param name="damage">Información del impacto</param>
    /// <param name="team">Equipo que debe excluirse del daño</param>
    public static void MakeDamage(Transform attacker, Collider2D hit, HashSet<IDamageable> hitTargets, CombatDamage damage, CombatTeam team = CombatTeam.Enemy)
    {
        // Ignora colliders nulos y cualquier collider perteneciente al mismo objeto raíz que el atacante
        if (attacker == null || hit == null || hitTargets == null || hit.transform.root == attacker.root)
            return;

        // Busca un componente que permita recibir daño en el objeto detectado o en alguno de sus padres
        IDamageable damageable = FindDamageable(hit);

        // Ignora objetos que no puedan recibir daño y objetivos pertenecientes al equipo excluido
        if (damageable == null || damageable.Team == team)
            return;

        // Evita aplicar daño varias veces al mismo objetivo
        if (hitTargets.Add(damageable))
            damageable.ReceiveDamage(damage);
    }

    #endregion

    #region Damageable Search

    /// <summary>
    /// Busca un componente que implemente IDamageable en el collider recibido o en alguno de sus objetos padre.
    /// </summary>
    /// <param name="collider">Collider del objetivo detectado</param>
    /// <returns>El componente IDamageable encontrado o null si no existe.</returns>
    public static IDamageable FindDamageable(Collider2D collider)
    {
        // Obtiene todos los MonoBehaviour del objeto detectado y de sus padres
        MonoBehaviour[] behaviours = collider.GetComponentsInParent<MonoBehaviour>();

        // Comprueba qué componentes implementan IDamageable
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IDamageable damageable)
                return damageable;
        }

        return null;
    }

    #endregion
}