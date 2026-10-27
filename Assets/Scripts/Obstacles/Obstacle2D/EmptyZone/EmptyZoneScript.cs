using UnityEngine;

public class EmptyZoneScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Comprobamos si el jugador es el que ha caido
        CharacterStats playerStats = other.GetComponentInParent<CharacterStats>();

        if (playerStats != null)
        {
            // Matamos al jugador
            playerStats.Death();

            Debug.Log("JAJAJAAJ HAS MUERTO");
        }
    }
}