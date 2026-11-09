
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
        if (playerApagado != null)
        {
            knockbackTimer -= Time.deltaTime;

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

        Vector2 playerCenter = playerRb.worldCenterOfMass;
        Vector2 closestPoint = bumperCollider.ClosestPoint(playerCenter);
        Vector2 direction = playerCenter - closestPoint;

        if (direction.sqrMagnitude < 0.001f) return;

        direction.Normalize();

        playerRb.linearVelocity = Vector2.zero;
        playerRb.AddForce(direction * bounceForce, ForceMode2D.Impulse);

        player.enabled = false;
        playerApagado = player;
        knockbackTimer = knockbackTime;
    }
}
