using System.Collections.Generic;
using UnityEngine;

public static class CombatManager
{
    /// <summary>
    /// Busca un componente que implemente IDamageable en el collider recibido
    /// o en alguno de sus objetos padre.
    /// </summary>
    /// <param name="collider">Collider del objetivo detectado.</param>
    /// <returns>El componente IDamageable encontrado o null si no existe.</returns>
    public static IDamageable FindDamageable(Collider2D collider)
    {
        MonoBehaviour[] behaviours = collider.GetComponentsInParent<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IDamageable damageable)
                return damageable;
        }

        return null;
    }

    public static void MakeDamage(Transform transform, Collider2D[] hits, HashSet<IDamageable> hitTargets, CombatDamage damage, CombatTeam team = CombatTeam.Enemy)
    {
        foreach (Collider2D hit in hits)
        {
            // Ignora colliders nulos y cualquier collider perteneciente
            // al propio jugador.
            if (hit == null || hit.transform.root == transform.root)
                continue;

            // Busca el componente que permite recibir daño en el objeto
            // o en alguno de sus padres.
            IDamageable damageable = CombatManager.FindDamageable(hit);

            // Ignora objetos que no reciben daño y objetivos del equipo jugador.
            if (damageable == null || damageable.Team == team)
                continue;

            // HashSet.Add devuelve false si el objetivo ya estaba registrado.
            // Así evitamos aplicar daño varias veces a un mismo personaje
            // que tenga varios colliders dentro del área de ataque.
            if (hitTargets.Add(damageable))
                damageable.ReceiveDamage(damage);
        }
    }

    public static void MakeDamage(Transform transform, Collider2D hit, HashSet<IDamageable> hitTargets, CombatDamage damage, CombatTeam team = CombatTeam.Enemy)
    {
        // Ignora colliders nulos y cualquier collider perteneciente
        // al propio jugador.
        if (hit == null || hit.transform.root == transform.root) return;

        // Busca el componente que permite recibir daño en el objeto
        // o en alguno de sus padres.
        IDamageable damageable = CombatManager.FindDamageable(hit);

        // Ignora objetos que no reciben daño y objetivos del equipo jugador.
        if (damageable == null || damageable.Team == team) return;

        // HashSet.Add devuelve false si el objetivo ya estaba registrado.
        // Así evitamos aplicar daño varias veces a un mismo personaje
        // que tenga varios colliders dentro del área de ataque.
        if (hitTargets.Add(damageable))
            damageable.ReceiveDamage(damage);
    }
}
