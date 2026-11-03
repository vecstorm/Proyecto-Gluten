using UnityEngine;

public class PlatformMoveScript : MonoBehaviour
{
    [SerializeField] private float speed = 3f; // velocidad de la plataforma
    [SerializeField] private string limitTag = "PlatformLimit"; // tag de PointA y PointB

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider; // Para saber donde esta el borde superior
    private Rigidbody2D playerUp; // el jugador que va encima de la plataforma
    private int direction = 1; // 1 = derecha, -1 = izquierda

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void FixedUpdate()
    {
        // Aquí tenemos para cuando avanza la plataforma
        Vector2 step = Vector2.right * direction * speed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + step); // Esto mueve la plataforma

        // En este casi si le jugador esta encima entonces lo mueve a la misma velocidad de la plataforma
        if (playerUp != null)
        {
            playerUp.position += step;
        }
    }

    // Al tocar algún punto A o B da la vuelta
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(limitTag))
        {
            direction = -direction;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Si la plataforma tiene al jugador encima entonces el juagdor va con él
        if (!collision.gameObject.CompareTag("Player") || collision.rigidbody == null) return;

        // Si los pies del jugador están en lo alto de la plataforma, va montado
        bool isOnTop = collision.collider.bounds.min.y >= boxCollider.bounds.max.y - 0.1f;

        playerUp = isOnTop ? collision.rigidbody : null;
    }

    // Cuando el jugador deja de tocar la plataforma ya no va montado
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerUp = null;
        }
    }
}