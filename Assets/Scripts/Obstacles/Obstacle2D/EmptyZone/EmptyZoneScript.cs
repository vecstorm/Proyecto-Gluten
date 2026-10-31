using UnityEngine;

public class EmptyZoneScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Comprobamos si el jugador es el que ha caido
        CharacterStats playerStats = other.GetComponentInParent<CharacterStats>();

        if (playerStats != null)
        {
            // Hacemos daño al jugador
            playerStats.TakeDamage(10);

            // NUEVOOO PARA APLICAR
            // Buscamos el script de respawn del jugador
            RespawnPointScript respawn = playerStats.GetComponent<RespawnPointScript>();

            if (respawn != null)
            {
                // Devolvemos al player a su punto de respawn
                respawn.Respawn();
            }
        }
    }
}