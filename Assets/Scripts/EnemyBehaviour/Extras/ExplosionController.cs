using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    [SerializeField] private int contactDamage = 2;
    [SerializeField] private float timer = 1f;

    private Collider2D hitbox;

    private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

    private void Awake()
    {
        hitbox = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        timer -= Time.fixedDeltaTime;

        if (timer <= 0f)
        {
            if (hitbox == null) return;

            hitbox.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Limpia los objetivos del ataque anterior
            hitTargets.Clear();

            CombatDamage damage = new CombatDamage(contactDamage, CombatTeam.Enemy, transform, transform.position, CombatAttackType.Melee, false);
            CombatManager.MakeDamage(transform, collision, hitTargets, damage, CombatTeam.Enemy);
        }
    }

    public void SetContactDamage(int contactDamage)
    {
        this.contactDamage = contactDamage;
    }
}
