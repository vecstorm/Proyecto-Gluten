using UnityEngine;

public class CubeHealtDamageScript : MonoBehaviour
{
    // Cantidad de vida que quitamos al jugador en cada golpe
    [SerializeField] private float damageAmount = 1f;

    // Segundos que pasan entre un golpe y el siguiente
    [SerializeField] private float damageInterval = 1.5f;

    // Si esta activado, el primer golpe ocurre al tocar el cubo
    // Si esta desactivado, el primer golpe llega tras damageInterval segundos
    [SerializeField] private bool damageOnContact = false;

    // Tiempo acumulado desde el ultimo golpe
    private float damageTimer = 0f;

    // Jugador que esta tocando el cubo en este momento
    private CharacterStats playerInContact = null;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Solo reaccionamos si lo que toca el cubo tiene vida (el jugador)
        CharacterStats characterStats = collision.gameObject.GetComponentInParent<CharacterStats>();

        if (characterStats != null)
        {
            // Guardamos quien esta en contacto para contar el tiempo en Update
            playerInContact = characterStats;

            // Si queremos daño inmediato, dejamos el temporizador ya "lleno"
            damageTimer = damageOnContact ? damageInterval : 0f;
        }
    }

    private void Update()
    {
        if (playerInContact == null) return;

        // Contamos el timepo en update para que el contacto funcione
        damageTimer += Time.deltaTime;

        // Comprobamos si ya toca hacer daño
        if (damageTimer >= damageInterval)
        {
            // Quitamos vida al jugador
            playerInContact.TakeDamage(damageAmount);

            // Reiniciamos el temporizador para el siguiente golpe
            damageTimer = 0f;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        CharacterStats characterStats = collision.gameObject.GetComponentInParent<CharacterStats>();

        if (characterStats != null && characterStats == playerInContact)
        {
            // El jugador noe sta en el cubo, dejamos de contar y reiniciamos el temporizador
            playerInContact = null;
            damageTimer = 0f;
        }
    }
}
