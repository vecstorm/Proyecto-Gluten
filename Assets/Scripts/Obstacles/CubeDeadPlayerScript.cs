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

        if (playerRb != null)
        {
            // Posición donde reaparece el jugador al morir
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            playerRb.position = new Vector3(0f, 1f, 0f);
        }
        else
        {
            jugador.transform.position = new Vector3(0f, 1f, 0f);
        }
    }
}