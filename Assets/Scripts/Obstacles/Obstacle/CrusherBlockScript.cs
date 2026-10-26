using UnityEngine;

public class CrusherBlockScript : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float topPosition = 5f;    // Altura máxima a la que sube
    [SerializeField] private float bottomPosition = 0f; // Altura mínima a la que baja
    [SerializeField] private float moveSpeed = 3f;      // Velocidad de subida del bloque

    private Rigidbody rb;
    private bool isMovingUp = false; // Empieza cayendo o esperando, luego sube

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Empezamos en modo cinemático para poder subirlo nosotros controlando la posición
        rb.isKinematic = true;
    }

    void FixedUpdate()
    {
        if (isMovingUp)
        {
            float targetY = topPosition;

            Vector3 motionBlock = new Vector3(transform.position.x, targetY, transform.position.z);
            Vector3 newPosition = Vector3.MoveTowards(transform.position, motionBlock, moveSpeed * Time.fixedDeltaTime);

            rb.MovePosition(newPosition);

            // Al llegar arriba del todo, cambiamos a modo caída libre
            if (Mathf.Abs(transform.position.y - targetY) < 0.05f)
            {
                isMovingUp = false;
                // Soltamos el bloque
                rb.isKinematic = false;
            }
        }
        else
        {
            // Si el bloque ya bajo lo detectara para volverlo a subir
            if (transform.position.y <= bottomPosition + 0.2f || (rb.linearVelocity.y >= -0.05f && rb.linearVelocity.y <= 0.05f && transform.position.y < topPosition))
            {
                // Haremos que no rebote raro
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                isMovingUp = true; // Aqui sube de nuevo el cubo
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // En caso que colisione con el jugador, lo mata directo
        if (collision.gameObject.CompareTag("Player"))
        {
            CharacterStats stats = collision.gameObject.GetComponent<CharacterStats>();
            if (stats != null)
            {
                // Lo matamos directamente al jugador
                stats.Death();
                Debug.Log("Vaya, fuiste aplastado JAJAJAJAAJAJ");
            }
        }
    }
}