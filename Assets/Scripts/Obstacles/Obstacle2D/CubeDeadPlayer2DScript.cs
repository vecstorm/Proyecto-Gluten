using UnityEngine;

public class CubeDeadPlayer2DScript : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Comprobamos si ha habido colisión con el jugador
        PlayerController jugador = collision.collider.GetComponentInParent<PlayerController>();
        if (jugador == null) return;

        // Buscamos el Rigidbody del jugador
        Rigidbody2D playerRb = jugador.GetComponent<Rigidbody2D>();

        // Buscamos CharacterStats a partir del jugador encontrado
        CharacterStats characterStats = jugador.GetComponent<CharacterStats>();

        // Comprobamos que existan ambos para evitar errores
        if (playerRb != null && characterStats != null)
        {
            characterStats.Death(); 
        }
        else
        {
            jugador.transform.position = new Vector2(0f, 1f);
        }
    }
}