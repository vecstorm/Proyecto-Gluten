using UnityEngine;

public class CheckpointZone2D : MonoBehaviour
{
    public static Vector2 ultimaPosicionSegura;
    public static bool hayPuntoSeguro = false;

    private static float maxPlayerXPosition = float.MinValue;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController jugador = other.GetComponentInParent<PlayerController>();

        if (jugador != null)
        {
            if (transform.position.x > maxPlayerXPosition)
            {
                maxPlayerXPosition = transform.position.x;
                ultimaPosicionSegura = transform.position;
                hayPuntoSeguro = true;

                Debug.Log("¡Checkpoint abanzado guardado en: " + ultimaPosicionSegura + "!");
            }
            else
            {
                ultimaPosicionSegura = transform.position;
                hayPuntoSeguro = true;
                Debug.Log("¡Checkpoint actualizado en: " + ultimaPosicionSegura + "!");
            }
        }
    }
}