using UnityEngine;

public class HealthUpgradePickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        CharacterStats stats = other.GetComponentInParent<CharacterStats>();

        if (stats == null)
            return;

        stats.AddHealthUpgradeToken();

        Destroy(gameObject);
    }
}