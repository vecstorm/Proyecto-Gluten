
using UnityEngine;

public class MoveCollision : MonoBehaviour
{
    // Configuracion del movimiento
    [SerializeField] private float speed = 2f;
    [SerializeField] private string pointTag = "PlatformPointMove";

    // Daño que quita la plataforma al jugador
    [SerializeField] private float damage = 2f;

    // Creamos las variables para guardar los componentes de la plataforma y sus puntos de movimiento
    private Rigidbody2D rb;
    private Collider2D platformCol;
    private Vector2 pointA, pointB, target;

    // Jugador cuando esta encima de la plataforma
    private Rigidbody2D currentRider;
    private float originalGravity;

    // Variables para controlar las colisiones y el empuje
    private Collider2D currentRiderCol;
    private bool riderPushed;

    // Tiempo hasta el que la plataforma puede volver a hacer daño al jugador
    private float nextDamageTime;

    // Guardamos los objetos detectados para utilizarlos en las comprobaciones
    private readonly Collider2D[] obstacleHits = new Collider2D[8];
    private readonly Collider2D[] playerHits = new Collider2D[16];

    // Guardamos las distancias y direcciones que utilizaremos para empujar al jugador
    // 4 lados cardinales (arriba, derecha, izquierda, abajo) + 4 diagonales
    private readonly float[] pushDistances = new float[8];
    private readonly bool[] pushTried = new bool[8];
    private readonly float[] pushPriority = new float[8];
    private static readonly Vector2[] pushDirections =
    {
        Vector2.up, Vector2.right, Vector2.left, Vector2.down,
        new Vector2(0.7071068f, 0.7071068f),
        new Vector2(-0.7071068f, 0.7071068f),
        new Vector2(0.7071068f, -0.7071068f),
        new Vector2(-0.7071068f, -0.7071068f)
    };
    private static readonly Vector3 extraSize = new Vector3(0.04f, 0.04f, 0f);

    // Filtramos los objetos que vamos a detectar
    private ContactFilter2D filter;

    private void Awake()
    {
        // Indicamos las capas y los objetos que tendremos en cuenta
        filter = new ContactFilter2D
        {
            useTriggers = Physics2D.queriesHitTriggers,
            useLayerMask = true,
            layerMask = Physics2D.DefaultRaycastLayers
        };
    }

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

        // Comprobamos si la plataforma puede avanzar sin atravesar al jugador
        bool canMove = TryPushPlayers(movementStep);

        // Movemos primero al jugador para evitar que pierda la detección de la plataforma
        HandlePlayerRider(canMove && !riderPushed ? movementStep : Vector2.zero);

        // Movemos la plataforma si no hay ninguna colisión que lo impida
        if (canMove) rb.position = nextPos;

        // Comprobamos si la plataforma ha llegado a su punto de destino
        if (Vector2.Distance(rb.position, target) <= 0.001f)
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
            currentRiderCol = currentRider != null ? currentRider.GetComponentInChildren<Collider2D>() : null;

            // Guardamos la gravedad original del nuevo jugador
            if (currentRider != null) originalGravity = currentRider.gravityScale;
        }

        // Comprobamos si hay un jugador encima para moverlo junto a la plataforma
        if (currentRider != null)
        {
            // Aplicamos al jugador el mismo desplazamiento que realiza la plataforma
            if (movementStep != Vector2.zero)
                MoveRider(currentRider, currentRiderCol, movementStep);

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
        int count = Physics2D.OverlapBox(boxCenter, boxSize, 0f, filter, playerHits);

        // Recorremos los objetos detectados para buscar al jugador
        for (int i = 0; i < count; i++)
        {
            Collider2D hit = playerHits[i];

            // Comprobamos si el objeto tiene el tag Player y un Rigidbody asociado
            if (hit == null || !hit.CompareTag("Player") || hit.attachedRigidbody == null) continue;

            // Su centro tiene que estar sobre la plataforma, si esta al lado no cuenta
            Bounds p = hit.bounds;
            if (p.center.x < b.min.x || p.center.x > b.max.x) continue;

            return hit.attachedRigidbody;
        }

        // Devolvemos null si no encontramos ningún jugador
        return null;
    }

    // Comprobamos si la plataforma puede avanzar sin atravesar al jugador
    private bool TryPushPlayers(Vector2 movementStep)
    {
        riderPushed = false;
        if (movementStep == Vector2.zero) return true;

        Bounds future = platformCol.bounds;
        future.center += (Vector3)movementStep;

        // Centro y tamaño de la caja de búsqueda
        Vector3 boxCenter = future.center;
        Vector3 boxSize = future.size + extraSize;

        // Si la plataforma se mueve en diagonal la caja tiene que cubrir también la franja
        // que barre al moverse, si no el jugador no se detecta y no se le empuja ni se le daña
        if (movementStep.x != 0f && movementStep.y != 0f)
        {
            boxCenter.x += movementStep.x * 0.5f;
            boxCenter.y += movementStep.y * 0.5f;
            boxSize.x += Mathf.Abs(movementStep.x);
            boxSize.y += Mathf.Abs(movementStep.y);
        }

        // Buscamos los jugadores que coincidirán con la siguiente posición
        int count = Physics2D.OverlapBox(boxCenter, boxSize, 0f, filter, playerHits);

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = playerHits[i];

            if (hit == null || !hit.CompareTag("Player") || hit.attachedRigidbody == null) continue;

            Rigidbody2D body = hit.attachedRigidbody;

            // Comprobamos si el jugador puede acompañar a la plataforma sin ser empujado
            if (body == currentRider && CanRide(body, hit, movementStep)) continue;

            if (body == currentRider) riderPushed = true;

            // Comprobamos si el jugador está atrapado y no puede moverse con la plataforma
            if (!CanRide(body, hit, movementStep) && Time.time >= nextDamageTime)
            {
                nextDamageTime = Time.time + 1f;
                body.GetComponentInParent<CharacterStats>()?.TakeDamage(damage);
            }

            // Intentamos sacar al jugador de la zona de la plataforma
            if (!PushPlayer(body, hit, future)) return false;
        }

        return true;
    }

    // Comprobamos si el jugador puede desplazarse junto a la plataforma
    private bool CanRide(Rigidbody2D body, Collider2D col, Vector2 step)
    {
        // Comprobamos si hay espacio para mover al jugador a la siguiente posición
        return IsFree(col, body, body.position + step);
    }


    // Buscamos una posición libre para sacar al jugador de la plataforma
    private bool PushPlayer(Rigidbody2D body, Collider2D col, Bounds future)
    {
        Bounds player = col.bounds;
        if (!player.Intersects(future)) return true;

        // Calculamos cuánto tiene que moverse el jugador para salir por cada lado
        pushDistances[0] = future.max.y - player.min.y + 0.02f; // arriba
        pushDistances[1] = future.max.x - player.min.x + 0.02f; // derecha
        pushDistances[2] = player.max.x - future.min.x + 0.02f; // izquierda
        pushDistances[3] = player.max.y - future.min.y + 0.02f; // abajo
        
        // Indicamos la distancia que necesita el jugador para salir por las diagonales                                                       
        pushDistances[4] = Mathf.Max(pushDistances[0], pushDistances[1]) * 1.41421356f; // arriba-derecha
        pushDistances[5] = Mathf.Max(pushDistances[0], pushDistances[2]) * 1.41421356f; // arriba-izquierda
        pushDistances[6] = Mathf.Max(pushDistances[3], pushDistances[1]) * 1.41421356f; // abajo-derecha
        pushDistances[7] = Mathf.Max(pushDistances[3], pushDistances[2]) * 1.41421356f; // abajo-izquierda

        // Reiniciamos los lados que ya hemos probado
        for (int i = 0; i < 8; i++) pushTried[i] = false;

        // Comprobamos hacia donde se mueve la plataforma
        Vector2 move = (Vector2)(future.center - platformCol.bounds.center);
        bool diagonal = move.x != 0f && move.y != 0f;

        // Indicamos la dirección del movimiento
        if (move.sqrMagnitude > 0f) move.Normalize();

        // Indicamos la prioridad de cada dirección que vamos a probar
        for (int i = 0; i < 8; i++)
        {
            pushPriority[i] = diagonal ? Vector2.Dot(pushDirections[i], move) : -pushDistances[i];
        }

        // Guardamos la posición y los límites iniciales del jugador
        Vector2 startPosition = body.position;
        Bounds startBounds = col.bounds;

        // Probamos primero la dirección con mayor prioridad
        for (int attempt = 0; attempt < 8; attempt++)
        {
            int best = -1;

            // Comprobamos qué dirección tiene la mayor prioridad entre las que faltan por probar
            for (int i = 0; i < 8; i++)
            {
                if (pushTried[i]) continue;

                if (best == -1 ||
                    pushPriority[i] > pushPriority[best] ||
                    (pushPriority[i] == pushPriority[best] && pushDistances[i] < pushDistances[best]))
                    best = i;
            }

            pushTried[best] = true;

            Vector2 direction = pushDirections[best];
            float distance = pushDistances[best];

            // Volvemos a la posición inicial para probar otra dirección
            body.position = startPosition;

            for (float moved = 0; moved < distance; moved += 0.02f)
            {
                float nextDistance = Mathf.Min(moved + 0.02f, distance);
                Vector2 next = startPosition + direction * nextDistance;

                // Calculamos los límites que tendría el jugador en la siguiente posición
                Bounds nextBounds = startBounds;
                nextBounds.center += (Vector3)(next - startPosition);

                // Comprobamos que no haya paredes u otros obstáculos en el camino
                if (!IsFree(col, body, next)) break;

                body.position = next;

                // Comprobamos si el jugador ya ha salido de la plataforma
                if (!nextBounds.Intersects(future)) return true;
            }
        }

        return !col.bounds.Intersects(future);
    }

    // Comprobamos si una posición está libre de obstáculos
    private bool IsFree(Collider2D col, Rigidbody2D body, Vector2 position)
    {
        // Calculamos los límites del jugador en la posición de destino
        Bounds b = col.bounds;
        b.center += (Vector3)(position - body.position);

        // Detectamos los objetos que hay en la posición indicada
        int count = Physics2D.OverlapBox(b.center, b.size * 0.96f, 0f, filter, obstacleHits);

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = obstacleHits[i];

            // Ignoramos triggers, el jugador y la plataforma
            if (hit == null || hit.isTrigger || hit == col || hit == platformCol) continue;

            // Obtenemos el Rigidbody del objeto detectado
            Rigidbody2D hitRb = hit.attachedRigidbody;

            // Ignoramos el propio jugador y la plataforma
            if (hitRb == body || hitRb == rb) continue;

            // Ignoramos otros objetos con Rigidbody dinámico
            if (hitRb != null && hitRb.bodyType == RigidbodyType2D.Dynamic) continue;

            return false;
        }

        return true;
    }


    // Movemos al jugador sin permitir que atraviese obstáculos
    private void MoveRider(Rigidbody2D body, Collider2D col, Vector2 step)
    {
        // Comprobamos si el jugador tiene un Collider para detectar los obstáculos
        if (col == null)
        {
            body.position += step;
            return;
        }

        // Comprobamos si hay movimiento para desplazar al jugador
        if (step == Vector2.zero) return;

        // Calculamos la distancia y la dirección del movimiento
        float distance = step.magnitude;
        Vector2 direction = step.normalized;

        // Guardamos la posición actual del jugador
        Vector2 current = body.position;

        // Comprobamos el recorrido en pequeños pasos
        for (float moved = 0f; moved < distance; moved += 0.02f)
        {
            // Calculamos la siguiente posición sin superar el desplazamiento total
            float distanceStep = Mathf.Min(0.02f, distance - moved);
            Vector2 next = current + direction * distanceStep;

            // Comprobamos si hay espacio libre para avanzar
            if (!IsFree(col, body, next)) break;

            // Movemos al jugador a la siguiente posición
            body.position = next;
            current = next;
        }
    }


    private void OnDisable()
    {
        // Restauramos el jugador con su gravedad original si se desactiva el script
        if (currentRider != null) currentRider.gravityScale = originalGravity;
    }
}
