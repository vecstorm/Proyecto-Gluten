using System.Collections.Generic;
using UnityEngine;

public class SpikeController : MonoBehaviour
{
    [SerializeField] private int contactDamage = 10;
    [SerializeField] Transform center;

    private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Limpia los objetivos del ataque anterior
            hitTargets.Clear();

            CombatDamage damage = new CombatDamage(contactDamage, CombatTeam.Enemy, transform, center.position, CombatAttackType.Melee, false);
            CombatManager.MakeDamage(transform, collision, hitTargets, damage, CombatTeam.Enemy);
        }
    }

    public void SetContactDamage(int contactDamage)
    {
        this.contactDamage = contactDamage;
    }
}
