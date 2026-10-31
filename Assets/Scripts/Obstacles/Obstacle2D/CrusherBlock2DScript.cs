using UnityEngine;

public class CrusherBlock2DScript : MonoBehaviour
{
    [SerializeField] private float topPosition = 5f;    // Altura máxima a la que sube
    [SerializeField] private float bottomPosition = 0f; // Altura mínima a la que baja
    [SerializeField] private float moveSpeed = 3f;      // Velocidad de subida del bloque

    private Rigidbody2D rb;
    private bool isMovingUp = false; // Empieza esperando, luego sube

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Empezamos en modo de saber su gravedad para subirlo
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Lo que hafo es que no se mueva de su posición
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }

    void FixedUpdate()
    {
        if (isMovingUp)
        {
            float targetY = topPosition;

            Vector2 motionBlock = new Vector2(transform.position.x, targetY);
            Vector2 newPosition = Vector2.MoveTowards(transform.position, motionBlock, moveSpeed * Time.fixedDeltaTime);

            rb.MovePosition(newPosition);

            // Al llegar arriba del todo, cambiamos a modo caída libre
            if (Mathf.Abs(transform.position.y - targetY) < 0.05f)
            {
                isMovingUp = false;
                // Soltamos el bloque
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }
        else
        {
            // Si el bloque ya bajo lo detectara para volverlo a subir
            if (transform.position.y <= bottomPosition + 0.2f || (rb.linearVelocity.y >= -0.05f && rb.linearVelocity.y <= 0.05f && transform.position.y < topPosition))
            {
                // Haremos que no rebote raro
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.linearVelocity = Vector2.zero;
                isMovingUp = true; // Aqui sube de nuevo el cubo
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // En caso que colisione con el jugador, lo mata directo
        if (collision.gameObject.CompareTag("Player"))
        {
            CharacterStats stats = collision.gameObject.GetComponent<CharacterStats>();
            if (stats != null)
            {
                // Lo matamos directamente al jugador
                stats.Death();
                Debug.Log("Ohh, you're dead :(");
            }
        }
    }
}