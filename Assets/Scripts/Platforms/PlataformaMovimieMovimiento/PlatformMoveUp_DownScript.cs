using UnityEngine;

public class PlatformMoveUp_DownScript : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private string limitTag = "PlatformLimit";

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private int direction = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void FixedUpdate()
    {
        Vector2 step = Vector2.up * direction * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + step);
    }

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
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}