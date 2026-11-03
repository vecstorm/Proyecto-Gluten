using UnityEngine;

public class PlatformMoveUp_DownScript : MonoBehaviour
{

    [SerializeField] private float speed = 3f; // velocidad de la plataforma
    [SerializeField] private string limitTag = "PlatformPoint"; // tag de PointA y PointB

    private Rigidbody2D rb;

    private int direction = 1; // 1 = arriba, -1 = abajo

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void FixedUpdate()
    {
        // Aquí tenemos para cuando avanza la plataforma
        Vector2 step = Vector2.up * direction * speed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + step);
    }

    // Al tocar algún punto A o B da la vuelta
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(limitTag))
        {
            direction = -direction;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Comprobamos que el jugador en la plataforma para evitar que se pegue por los lados
            if (collision.transform.position.y > transform.position.y)
            {
                collision.transform.SetParent(transform);
            }
        }
    }

    // Cuando el jugador sale de la plataforma
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.transform.parent == transform)
            {
                collision.transform.SetParent(null);
            }
        }
    }
}