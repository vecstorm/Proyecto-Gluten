using UnityEngine;

public class RevivePlayer : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Comprobamos si el jugador tiene CharacterStats
        CharacterStats characterStats = other.GetComponentInParent<CharacterStats>();

        if (characterStats != null)
        {
            // Recuperamos toda la vida y estamina del jugador
            characterStats.FullHeal();

            Debug.Log("El player tiene la vida completa");
        }
    }
}