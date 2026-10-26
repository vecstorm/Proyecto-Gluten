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

        CharacterStats characterStats = collision.gameObject.GetComponent<CharacterStats>();

        if (playerRb != null)
        {
            characterStats.Death();
        }
        else
        {
            jugador.transform.position = new Vector2(0f, 1f);
        }
    }
}