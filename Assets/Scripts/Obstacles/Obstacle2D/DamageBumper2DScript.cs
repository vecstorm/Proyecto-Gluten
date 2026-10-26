using UnityEngine;

public class DamageBumper2DScript : MonoBehaviour
{
    [SerializeField] private float damageAmount = 1f;
    [SerializeField] private float bounceForce = 15f;
    [SerializeField] private float knockbackTime = 0.3f;

    private Collider2D bumperCollider;

    // Guardamos el PlayerController que hemos apagado para poder encenderlo luego
    private PlayerController playerApagado;

    // Tiempo que queda hasta devolverle el control al jugador
    private float knockbackTimer = 0f;

    private void Awake()
    {
        bumperCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        // Solo hacemos algo si hay un jugador con el control apagado
        if (playerApagado != null)
        {
            // Restamos el tiempo que ha pasado desde el frame anterior
            knockbackTimer -= Time.deltaTime;

            // Si el tiempo ha llegado a 0, devolvemos el control
            if (knockbackTimer <= 0f)
            {
                playerApagado.enabled = true;
                playerApagado = null;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Lo que hacemos es comprobar si hemos colisionado con el jugador
        if (!collision.gameObject.TryGetComponent<PlayerController>(out PlayerController player))
            return;

        // Si es correto entonces aplicaremos el daño
        CharacterStats characterStats = collision.gameObject.GetComponent<CharacterStats>();
        if (characterStats != null)
        {
            characterStats.TakeDamage(damageAmount);
        }

        Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();

        if (playerRb == null) return;

        // Obtenemos el centro del player y el punto mas cercano del bumper
        Vector2 playerCenter = playerRb.worldCenterOfMass;
        Vector2 closestPoint = bumperCollider.ClosestPoint(playerCenter);

        // Calculamos la dirección desde el bumper hacia el jugador
        Vector2 direction = playerCenter - closestPoint;

        // Comprobamos queeste kla direccion valida
        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        // Eliminamos la velocidad para que se vea el impulso
        playerRb.linearVelocity = Vector2.zero;

        // Aplicamos el impulso
        playerRb.AddForce(direction * bounceForce, ForceMode2D.Impulse);

        // Apagamos el script de movimiento para que no borre el impulso
        player.enabled = false;

        // Guardamos al jugador y ponemos el temporizador en marcha
        playerApagado = player;
        knockbackTimer = knockbackTime;
    }
}