using UnityEngine;

public class CrusherBlockScript : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float topPosition = 5f;    // Altura máxima a la que sube
    [SerializeField] private float bottomPosition = 0f; // Altura mínima a la que baja
    [SerializeField] private float moveSpeed = 3f;      // Velocidad del bloque

    private Rigidbody rb;
    private bool isMovingDown = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Nos aseguramos de que sea kinematic para que la gravedad no lo afecte
        rb.isKinematic = true;
    }

    void FixedUpdate()
    {
        // Aqui lo que hacemos es decidir la posision
        float targetY;

        if (isMovingDown)
        {
            targetY = bottomPosition;
        }
        else
        {
            targetY = topPosition;
        }

        // Moveremos el bloque hacia la posicion que esta calculada usando físicas
        Vector3 motionBlock = new Vector3(transform.position.x, targetY, transform.position.z);
        Vector3 newPosition = Vector3.MoveTowards(transform.position, motionBlock, moveSpeed * Time.fixedDeltaTime);

        rb.MovePosition(newPosition);

        // Cambiaremos la direccion al llegar al destino
        if (Mathf.Abs(transform.position.y - targetY) < 0.05f)
        {
            isMovingDown = !isMovingDown;
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