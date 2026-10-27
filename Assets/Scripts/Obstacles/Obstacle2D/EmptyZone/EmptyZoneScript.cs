using UnityEngine;

public class EmptyZoneScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Comprobamos si el jugador es el que ha caido
        CharacterStats statsJugador = other.GetComponentInParent<CharacterStats>();

        if (statsJugador != null)
        {
            // Matamos al jugador
            statsJugador.Death();

            Debug.Log("JAJAJAAJ HAS MUERTO");
        }
    }
}