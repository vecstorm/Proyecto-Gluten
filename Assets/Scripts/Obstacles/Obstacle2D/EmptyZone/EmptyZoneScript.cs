using UnityEngine;

public class EmptyZoneScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        CharacterStats playerStats = other.GetComponentInParent<CharacterStats>();

        if (playerStats != null)
        {
            RespawnPointScript respawn = playerStats.GetComponent<RespawnPointScript>();

            if (respawn != null)
            {
                // Si el jugador tiene más de 10 de vida, le restamos vida y usamos el respawn sin curar
                if (playerStats.CurrentHealth > 10)
                {
                    // Aplicamos el daño manualmente o mediante un método que NO dispare la muerte automática
                    // (O si usas TakeDamage, asegúrate de que no lance el respawn por evento en este caso)
                    playerStats.TakeDamage(10);
                    respawn.Respawn_Without_Health();
                }
                else
                {
                    // Si los 10 de daño lo van a matar, dejamos que muera oficialmente (su evento OnDeath hará el Respawn completo)
                    playerStats.TakeDamage(10);
                    // Aquí NO llamamos a respawn.Respawn() manualmente, dejamos que el evento de muerte lo haga solo una vez.
                }
            }
        }
    }
}