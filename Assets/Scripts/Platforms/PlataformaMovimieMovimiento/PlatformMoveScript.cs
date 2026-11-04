using UnityEngine;

public class PlatformMoveScript : MonoBehaviour
{
    [SerializeField] private float speed = 3f; // Velocidad de la plataforma
    [SerializeField] private string limitTag = "PlatformLimit"; // Tag de PointA y PointB

    private Rigidbody2D rb;

    // Collider de la plataforma para comprobar si el jugador esta encima
    private BoxCollider2D boxCollider;

    // Rigidbody del jugador que esta encima de la plataforma
    private Rigidbody2D playerUp;

    private int direction = 1; // 1 = derecha, -1 = izquierda

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Quitamos el rozamiento para evitar que el jugador tiemble al moverse con la plataforma
        PhysicsMaterial2D sinRozamiento = new PhysicsMaterial2D("PlatformMove_SinRozamiento");
        sinRozamiento.friction = 0f;
        sinRozamiento.bounciness = 0f;

        if (boxCollider != null)
            boxCollider.sharedMaterial = sinRozamiento;
    }

    void FixedUpdate()
    {
        // Calculamos el movimiento de la plataforma
        Vector2 step = Vector2.right * direction * speed * Time.fixedDeltaTime;

        // Movemos la plataforma
        rb.position += step;

        // Si el jugador esta encima, tambien lo movemos con la plataforma
        if (playerUp != null)
        {
            playerUp.position += step;
        }
    }

    // Cuando toca un punto limite, cambia la direccion
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(limitTag))
        {
            direction = -direction;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Comprobamos si el objeto que toca la plataforma es el jugador
        if (!collision.gameObject.CompareTag("Player") || collision.rigidbody == null) return;

        // Comprobamos si el jugador esta encima de la plataforma
        bool isOnTop = collision.collider.bounds.min.y >= boxCollider.bounds.max.y - 0.1f;

        // Si esta encima, guardamos su Rigidbody para moverlo con la plataforma
        playerUp = isOnTop ? collision.rigidbody : null;
    }

    // Cuando el jugador deja de tocar la plataforma, deja de moverse con ella
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerUp = null;
        }
    }
}