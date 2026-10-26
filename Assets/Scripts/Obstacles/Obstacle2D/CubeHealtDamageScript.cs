using UnityEngine;

public class CubeHealtDamageScript : MonoBehaviour
{
    // Cantidad de vida que quitamos al jugador
    [SerializeField] private float damageAmount = 1f;

    // Tiempo entre cada daño
    [SerializeField] private float damageInterval = 1.5f;

    // Tiempo que el jugador permanece en contacto
    private float damageTimer = 0f;

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Comprobamos si el objeto tiene el sistema de vida
        CharacterStats characterStats = collision.gameObject.GetComponent<CharacterStats>();

        if (characterStats != null)
        {
            // Sumamos el tiempo transcurrido
            damageTimer += Time.fixedDeltaTime;

            // Comprobamos si ha pasado el intervalo de daño
            if (damageTimer >= damageInterval)
            {
                // Quitamos vida al jugador
                characterStats.TakeDamage(damageAmount);

                // Mostramos el daño en la consola
                Debug.Log("El jugador recibe " + damageAmount + " de daño");

                // Comprobamos la vida actual del jugador
                Debug.Log("Vida actual: " + characterStats.CurrentHealth);

                // Reiniciamos el temporizador
                damageTimer = 0f;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Reiniciamos el tiempo cuando el jugador deja de tocar el obstáculo
        CharacterStats characterStats = collision.gameObject.GetComponent<CharacterStats>();

        if (characterStats != null)
        {
            damageTimer = 0f;
        }
    }
}