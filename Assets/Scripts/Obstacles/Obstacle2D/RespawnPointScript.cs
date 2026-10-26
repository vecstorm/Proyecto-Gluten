using UnityEngine;

public class RespawnPointScript : MonoBehaviour
{
    // Rigidbody2D del jugador: lo necesitamos para frenarlo al reaparecer
    private Rigidbody2D rb;

    // Componente de vida del jugador (de aquí viene el evento OnDeath)
    private CharacterStats characterStats;

    // Posicion donde reaparecera el jugador
    private Vector2 respawnPointPlayer;

    private void Awake()
    {
        // GetComponent busca el componente en ESTE mismo objeto (el Player)
        rb = GetComponent<Rigidbody2D>();
        characterStats = GetComponent<CharacterStats>();

        // Buscamos el objeto con la etiqueta SpawnCube
        GameObject spawnCube = GameObject.FindWithTag("SpawnCube");

        if (spawnCube != null)
        {
            // Guardamos su posicion (x, y)
            respawnPointPlayer = spawnCube.transform.position;
        }
        else
        {
            // Si no existe el cubo, usamos una posicion por defecto
            respawnPointPlayer = new Vector2(0f, 1f);
            Debug.LogWarning("No se encontro ningun objeto con la etiqueta SpawnCube");
        }
    }

    private void OnEnable()
    {
        // NUEVO: si este objeto no tiene CharacterStats (por ejemplo el cubo), no hacemos nada
        if (characterStats == null) return;

        // Nos suscribimos: cuando el jugador muera, se ejecuta Respawn
        characterStats.OnDeath += Respawn;
    }

    private void OnDisable()
    {
        // NUEVO: misma comprobacion para no dar error al desuscribirnos
        if (characterStats == null) return;

        // Nos desuscribimos para no dejar la funcion colgada en el evento
        characterStats.OnDeath -= Respawn;
    }

    private void Respawn()
    {
        if (rb != null)
        {
            // Quitamos la velocidad lineal (movimiento) y angular (giro)
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Movemos al jugador al respawn, conservando su Z actual
        transform.position = new Vector3(
            respawnPointPlayer.x,
            respawnPointPlayer.y,
            transform.position.z
        );

        // Recuperamos toda la vida
        characterStats.FullHeal();

        Debug.Log("El respawn del jugador ha funcionado, ole");
    }
}