using UnityEngine;

public class PlatformMoveUp_DownScript : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private string limitTag = "PlatformPoint";
    [SerializeField] private bool useVelocityMovement = true;

    // Rigidbody2D de la plataforma para controlar su movimiento
    private Rigidbody2D rb;

    // Collider de la plataforma para comprobar si el jugador esta encima
    private Collider2D platformCollider;

    // 1 = sube, -1 = baja
    private int direction = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        platformCollider = GetComponent<Collider2D>();

        // La plataforma se mueve por codigo y no por fisicas
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Suaviza el movimiento de la plataforma
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void FixedUpdate()
    {
        // Comprueba si usamos la velocidad para mover la plataforma
        if (useVelocityMovement)
        {
            rb.linearVelocity = Vector2.up * direction * speed;
        }
        else
        {
            // Calcula cuanto debe moverse la plataforma en cada frame
            Vector2 step = Vector2.up * direction * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + step);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si toca un punto limite, cambia la direccion del movimiento
        if (other.CompareTag(limitTag))
            direction = -direction;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Comprueba que el objeto que toca la plataforma sea el jugador
        if (collision.rigidbody == null || !collision.gameObject.CompareTag("Player"))
            return;

        // Una vez que el jugador este abajo de la plataforma del collider
        bool PlayerDownPlatform = collision.collider.bounds.max.y <= platformCollider.bounds.min.y + 0.1f;

        // Si el juagador esta abajo DEATH()
        if (PlayerDownPlatform)
        {
            CharacterStats charactStats = collision.gameObject.GetComponent<CharacterStats>();

            if(charactStats != null)
            {
                charactStats.Death();
            }
            return;
        }

        // Comprueba que el jugador este encima de la plataforma
        bool isOnTop = collision.collider.bounds.min.y >= platformCollider.bounds.max.y - 0.1f;
        if (!isOnTop)
            return;

        // Comprueba la velocidad vertical del jugador
        float verticalSpeed = collision.rigidbody.linearVelocity.y;

        // Si el jugador esta subiendo a una velocidad parecida a la plataforma,
        // se detiene su movimiento vertical para evitar que atraviese la plataforma
        if (verticalSpeed > 0f && verticalSpeed <= speed + 0.5f)
        {
            collision.rigidbody.linearVelocity = new Vector2(collision.rigidbody.linearVelocity.x, 0f);
        }
    }
}