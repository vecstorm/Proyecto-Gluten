using Unity.Netcode;
using UnityEngine;

public class StayBumperVidaScript : NetworkBehaviour
{
    // Creamos una variable para indicar cuanta vida quitamos
    [SerializeField] private int DañoJugador = 1;

    // Creamos una variable para indicar cada cuanto tiempo quitamos vida
    [SerializeField] private float TiempoDaño = 1f;

    // Creamos una variable para saber el tiempo que recibe daño el jugador
    private float TiempoJugadorDaño = 0f;

    private void OnCollisionStay(Collision collision)
    {
        if (!IsServer) return;

        // Aqui comprobamos si el jugador esta tocando el obstaculo
        if (collision.gameObject.TryGetComponent<PlayerController>(out PlayerController player))
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
        if (!IsServer) return;

        // Reiniciamos el tiempo cuando el jugador se aleja del obstaculo
        if (collision.gameObject.GetComponent<PlayerController>() != null)
        {
            TiempoJugadorDaño = 0f;
        }
    }
}
