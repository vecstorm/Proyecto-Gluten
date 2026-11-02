using UnityEngine;

public class PlatformMoveScript : MonoBehaviour
{
    [Header("Límites del Recorrido (Arrastra aquí los puntos)")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Configuración")]
    [SerializeField] private float speed = 3f;

    private float minX, maxX;
    private int direction = 1;

    void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError("¡Falta asignar Point A o Point B en el inspector!", this);
            enabled = false;
            return;
        }

        // Calcula los límites basándose en la posición de los puntos
        minX = Mathf.Min(pointA.position.x, pointB.position.x);
        maxX = Mathf.Max(pointA.position.x, pointB.position.x);
    }

    void Update()
    {
        // Mueve la plataforma horizontalmente
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime);

        // Si llega al límite derecho, rebota a la izquierda
        if (transform.position.x >= maxX)
        {
            transform.position = new Vector3(maxX, transform.position.y, transform.position.z);
            direction = -1;
        }
        // Si llega al límite izquierdo, rebota a la derecha
        else if (transform.position.x <= minX)
        {
            transform.position = new Vector3(minX, transform.position.y, transform.position.z);
            direction = 1;
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