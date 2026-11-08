using UnityEngine;

public class CubeDeadPlayerScript : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Comprobamos si ha habido colisión con el jugador
        PlayerController jugador = collision.collider.GetComponentInParent<PlayerController>();
        if (jugador == null) return;

        // Buscamos el Rigidbody del jugador
        Rigidbody playerRb = jugador.GetComponent<Rigidbody>();

        CharacterStats characterStats = collision.gameObject.GetComponent<CharacterStats>();

        if (playerRb != null)
        {
            characterStats.Death();
        }
        else
        {
            jugador.transform.position = new Vector3(0f, 1f, 0f);
        }
    }
}