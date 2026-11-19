using System.Collections.Generic;
using UnityEngine;

public class HitboxWithWarning : MonoBehaviour
{
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private Collider2D hitbox;
    [SerializeField] private GameObject warning;
    [SerializeField] private SpriteRenderer warningArea;
    [SerializeField] private Color warningColor;

    private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();
    private Color spawnColor;

    private void Awake()
    {
        hitbox.enabled = false;
        spawnColor = warningArea.color;
    }

    public void SetHitboxActive(bool value)
    {
        hitbox.enabled = value;
        warning.SetActive(!value);

        if (value)
        {
            warningArea.color = warningColor;
        }
        else
        {
            warningArea.color = spawnColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            hitTargets.Clear();

            CombatDamage damage = new CombatDamage(contactDamage, CombatTeam.Enemy, transform, transform.position);
            CombatManager.MakeDamage(transform, collision, hitTargets, damage, CombatTeam.Enemy);
        }
    }

    public void SetContactDamage(int contactDamage)
    {
        this.contactDamage = contactDamage;
    }
}
