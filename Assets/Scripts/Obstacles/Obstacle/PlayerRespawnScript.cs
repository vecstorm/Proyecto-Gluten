
using UnityEngine;

public class PlayerRespawnScript : MonoBehaviour
{
    private Rigidbody rb;

    private CharacterStats characterStats;
    // Esto es la posicion del jugador
    private Vector3 RespawnPaintPlayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Lo que hacemos es obtener el componente de vida del jugador
        characterStats = GetComponent<CharacterStats>();

        // Buscaremos el objeto de la etiqueta SpawnCube
        GameObject spawnCube = GameObject.FindWithTag("SpawnCube");

        // Si encontramos el cubo, guardamos su posicion
        if (spawnCube != null)
        {
            RespawnPaintPlayer = spawnCube.transform.position;
        }
        else
        {
            // Si no encontramos el cubo, usamos esta posicion por defecto
            RespawnPaintPlayer = new Vector3(0f, 1f, 0f);
        }
    }

    private void OnEnable()
    {
        // Cuando el jugador muera, ejecutamos la funcion Respawn
        characterStats.OnDeath += Respawn;
    }

    private void OnDisable()
    {
        // Dejamos de escuchar el evento de muerte del jugador
        characterStats.OnDeath -= Respawn;
    }

    private void Respawn()
    {
        if (rb != null && !rb.isKinematic)
        {
            // Detenemos el movimiento y la rotacion del jugador
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Colocamos al jugador en la posicion de respawn
        transform.position = RespawnPaintPlayer;

        // Recuperamos toda la vida del jugador
        characterStats.FullHeal();

        // Mensaje del respawn funciona
        Debug.Log("El respawn del jugador ha funcionado, ole");
    }
}
