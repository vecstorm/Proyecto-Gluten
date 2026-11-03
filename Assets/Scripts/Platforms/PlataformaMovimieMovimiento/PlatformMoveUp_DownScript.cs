using UnityEngine;
using System.Collections.Generic;

public class PlatformMoveUp_DownScript : MonoBehaviour
{

    [SerializeField] private float speed = 3f; // velocidad de la plataforma
    [SerializeField] private string limitTag = "PlatformPoint"; // tag de PointA y PointB

    private Rigidbody2D rb;

    private int direction = 1; // 1 = arriba, -1 = abajo

    private HashSet<Rigidbody2D> passengerRbs = new HashSet<Rigidbody2D>();

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

        // Movemos la plataforma con MovePosition
        rb.MovePosition(rb.position + step);

        // Mover al jugador, el mismo desplazamiento para que suban y bajen con ella sin romper el salto
        foreach (Rigidbody2D passengerRb in passengerRbs)
        {
            if (passengerRb != null)
            {
                passengerRb.position += step;
            }
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Comprobamos que el jugador esta encima de la plataforma para evitar
            if (collision.transform.position.y > transform.position.y)
            {
                Rigidbody2D pRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (pRb != null && !passengerRbs.Contains(pRb))
                {
                    passengerRbs.Add(pRb);
                }
            }
        }
    }

    // Cuando el juagdor sale de la plataforma y se separa
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D pRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (pRb != null && passengerRbs.Contains(pRb))
            {
                passengerRbs.Remove(pRb);
            }
        }
    }
}