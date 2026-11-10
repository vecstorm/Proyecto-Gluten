
using UnityEngine;

public class MoveCollision : MonoBehaviour
{
    // Configuracion del movimiento
    [SerializeField] private float speed = 2f;
    [SerializeField] private string pointTag = "PlatformPointMove";

    // Creamos las variables para guardar los componentes de la plataforma y sus puntos de movimiento
    private Rigidbody2D rb;
    private Collider2D platformCol;
    private Vector2 pointA, pointB, target;

    // Jugador cuando esta encima de la plataforma
    private Rigidbody2D currentRider;
    private float originalGravity;

    private void Start()
    {
        // Obtenemos los componentes de la plataforma
        rb = GetComponent<Rigidbody2D>();
        platformCol = GetComponent<Collider2D>();

        // Configuramos las físicas para controlar el movimiento de la plataforma mediante código
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Buscamos los dos puntos de movimiento más cercanos a la plataforma
        if (!InitializePoints())
        {
            enabled = false;
            return;
        }

        // Establecemos como objetivo el punto más cercano a la plataforma
        target = Vector2.Distance(transform.position, pointA) <= Vector2.Distance(transform.position, pointB) ? pointA : pointB;
    }

    private bool InitializePoints()
    {
        // Buscamos los puntos dentro de este prefab y no en toda la escena
        Transform root = transform.parent != null ? transform.parent : transform;

        // Creamos las variables para guardar los dos puntos más cercanos y sus distancias
        Transform p1 = null, p2 = null;
        float minDist = float.MaxValue, secondDist = float.MaxValue;

        // Recorremos los hijos del prefab para encontrar los dos puntos más cercanos a la plataforma
        foreach (var p in root.GetComponentsInChildren<Transform>(true))
        {
            // Ignoramos la propia plataforma y los objetos que no tengan el tag de punto
            if (p == transform || !p.CompareTag(pointTag)) continue;

            // Calculamos la distancia entre la plataforma y cada punto
            float dist = Vector2.Distance(transform.position, p.position);

            if (dist < minDist)
            {
                // Guardamos el punto anterior como segundo y el actual como primero
                secondDist = minDist; p2 = p1;
                minDist = dist; p1 = p;
            }
            else if (dist < secondDist)
            {
                // Guardamos el punto actual como segundo más cercano
                secondDist = dist; p2 = p;
            }
        }

        // Comprobamos que se hayan encontrado los dos puntos necesarios
        if (!p1 || !p2)
        {
            Debug.Log($"Movimiento Colisión la plataforma necesita 2 hijos con el tag '{pointTag}' (PointA y PointB).", this);
            return false;
        }

        // Guardamos las posiciones de los dos puntos encontrados
        pointA = p1.position;
        pointB = p2.position;
        return true;
    }

    private void FixedUpdate()
    {
        // Guardamos la posición actual de la plataforma
        Vector2 currentPos = rb.position;

        // Calculamos la siguiente posición teniendo en cuenta la velocidad y el tiempo de físicas
        Vector2 nextPos = Vector2.MoveTowards(currentPos, target, speed * Time.fixedDeltaTime);

        // Calculamos la distancia que se desplazará la plataforma en este paso
        Vector2 movementStep = nextPos - currentPos;

        // Movemos primero al jugador para evitar que pierda la detección de la plataforma
        HandlePlayerRider(movementStep);

        // Movemos la plataforma a la siguiente posición
        rb.position = nextPos;

        // Comprobamos si la plataforma ha llegado a su punto de destino
        if (Vector2.Distance(nextPos, target) <= 0.001f)
        {
            // Cambiamos el objetivo al otro punto para continuar el movimiento
            target = (target == pointA) ? pointB : pointA;
        }
    }

    private void HandlePlayerRider(Vector2 movementStep)
    {
        // Detectamos si hay un jugador encima de la plataforma
        Rigidbody2D playerOnTop = DetectPlayerOnTop();

        // Comprobamos si ha cambiado el jugador que está encima de la plataforma
        if (playerOnTop != currentRider)
        {
            // Restauramos el jugador anterior con su gravedad original
            if (currentRider != null) currentRider.gravityScale = originalGravity;

            // Guardamos el nuevo jugador que está encima de la plataforma
            currentRider = playerOnTop;

            // Guardamos la gravedad original del nuevo jugador
            if (currentRider != null) originalGravity = currentRider.gravityScale;
        }

        // Comprobamos si hay un jugador encima para moverlo junto a la plataforma
        if (currentRider != null)
        {
            // Aplicamos al jugador el mismo desplazamiento que realiza la plataforma
            if (movementStep != Vector2.zero)
                currentRider.position += movementStep;

            // Comprobamos si el jugador está saltando mediante su velocidad vertical
            if (currentRider.linearVelocity.y > 0.5f)
            {
                // Restauramos la gravedad original del jugador mientras salta
                currentRider.gravityScale = originalGravity;
            }
            else
            {
                // Anulamos la gravedad del jugador para evitar vibraciones sobre la plataforma
                currentRider.gravityScale = 0f;

                // Eliminamos su velocidad vertical para evitar movimientos no deseados
                currentRider.linearVelocity = new Vector2(currentRider.linearVelocity.x, 0f);
            }
        }
    }

    private Rigidbody2D DetectPlayerOnTop()
    {
        // Obtenemos los límites del Collider de la plataforma
        Bounds b = platformCol.bounds;

        // Calculamos el centro y el tamaño de la zona donde detectaremos al jugador
        Vector2 boxCenter = new Vector2(b.center.x, b.max.y - 0.085f);
        Vector2 boxSize = new Vector2(b.size.x, 0.23f);

        // Detectamos todos los objetos que se encuentren dentro de la zona indicada
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f);

        // Recorremos los objetos detectados para buscar al jugador
        foreach (var hit in hits)
        {
            // Comprobamos si el objeto tiene el tag Player y un Rigidbody asociado
            if (hit.CompareTag("Player") && hit.attachedRigidbody != null)
                return hit.attachedRigidbody;
        }

        // Devolvemos null si no encontramos ningún jugador
        return null;
    }

    private void OnDisable()
    {
        // Restauramos el jugador con su gravedad original si se desactiva el script
        if (currentRider != null) currentRider.gravityScale = originalGravity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Comprobamos si el objeto con el que chocamos es el jugador
        if (!collision.gameObject.CompareTag("Player")) return;

        // Comprobamos si el jugador está debajo de la plataforma
        bool isBelow = collision.collider.bounds.max.y <= platformCol.bounds.min.y + 0.1f;

        // Comprobamos si la plataforma se desplaza principalmente en vertical
        bool isVerticalMovement = Mathf.Abs(pointB.y - pointA.y) >= Mathf.Abs(pointB.x - pointA.x);

        // Comprobamos si el jugador está debajo de una plataforma que se mueve verticalmente
        if (isBelow && isVerticalMovement)
        {
            // Hacemos que el personaje muera al quedar aplastado por la plataforma
            collision.gameObject.GetComponentInParent<CharacterStats>()?.Death();
        }
    }
}
