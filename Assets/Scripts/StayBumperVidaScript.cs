
using UnityEngine;

public class StayBumperVidaScript : MonoBehaviour
{
    // Creamos una variable para indicar cuanta vida quitamos
    [SerializeField] private int DañoJugador = 1;

    // Creamos una variable para indicar cada cuanto tiempo quitamos vida
    [SerializeField] private float TiempoDaño = 1.5f;

    // Creamos una variable para saber el tiempo que recibe daño el jugador
    private float TiempoJugadorDaño = 0f;

    private void OnCollisionStay(Collision collision)
    {
        // Aqui comprobamos si el jugador esta tocando el obstaculo
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();

        if (player != null)
        {
            // Sumamos el tiempo que ha pasado
            TiempoJugadorDaño += Time.fixedDeltaTime;

            // Comprobamos que cuando pase el tiempo quite vida
            if (TiempoJugadorDaño >= TiempoDaño)
            {
                // Quitamos vida al jugador
                player.RecibirDaño(DañoJugador);

                // Mostramos el daño en el mensaje
                Debug.Log("El jugador recibe " + DañoJugador + " de daño");

                // Reiniciamos el tiempo
                TiempoJugadorDaño = 0f;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Reiniciamos el tiempo cuando el jugador se aleja del obstaculo
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();

        if (player != null)
        {
            TiempoJugadorDaño = 0f;
        }
    }
}
