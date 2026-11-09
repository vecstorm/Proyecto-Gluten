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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Solo reaccionamos si lo que toca el cubo tiene vida (el jugador)
        CharacterStats characterStats = collision.gameObject.GetComponentInParent<CharacterStats>();

        if (characterStats != null)
        {
            // Si queremos daño inmediato, dejamos el temporizador ya "lleno"
            damageTimer = damageOnContact ? damageInterval : 0f;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Buscamos CharacterStats
        CharacterStats characterStats = collision.gameObject.GetComponentInParent<CharacterStats>();

        if (characterStats != null)
        {
            // Sumamos el tiempo que dura cada paso de fisica
            damageTimer += Time.fixedDeltaTime;

            // Comprobamos si ya toca hacer daño
            if (damageTimer >= damageInterval)
            {
                // Quitamos vida al jugador
                characterStats.TakeDamage(damageAmount);

                // Reiniciamos el temporizador para el siguiente golpe
                damageTimer = 0f;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        CharacterStats characterStats = collision.gameObject.GetComponentInParent<CharacterStats>();

        if (characterStats != null)
        {
            // El jugador se ha ido: reiniciamos el temporizador
            damageTimer = 0f;
        }
    }
}