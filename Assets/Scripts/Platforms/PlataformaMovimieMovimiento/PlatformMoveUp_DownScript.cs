using UnityEngine;
using System.Collections.Generic;

public class PlatformMoveUp_DownScript : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private string limitTag = "PlatformLimit";

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private Rigidbody2D playerUp;
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

        if (playerUp != null)
        {
            playerUp.position += step;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(limitTag))
        {
            direction = -direction;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || collision.rigidbody == null) return;

        float playerBottom = collision.collider.bounds.min.y;
        float platformTop = boxCollider.bounds.max.y;

        if (playerBottom < platformTop - 0.1f)
        {
            Physics2D.IgnoreCollision(collision.collider, boxCollider, true);
            playerUp = null;
            return;
        }

        bool isOnTop = playerBottom >= platformTop - 0.1f;
        playerUp = isOnTop ? collision.rigidbody : null;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerUp = null;
            Collider2D playerCol = collision.collider;
            if (playerCol != null && boxCollider != null)
            {
                Physics2D.IgnoreCollision(playerCol, boxCollider, false);
            }
        }
    }
}