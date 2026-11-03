using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Estados posibles del sistema de gancho.
/// Controlan todo el ciclo de vida del gancho desde que se dispara hasta que vuelve a estar disponible.
/// </summary>
public enum HookState { Ready, Shooting, Retracting, Pulling }

/// <summary>
/// Gestiona todo el control del gancho del jugador.
/// </summary>
public class PlayerHook : MonoBehaviour
{
    #region Variables

    #region References

    [Header("References")]

    [Tooltip("RigidBody del jugador que será desplazado por el gancho")]
    [SerializeField] private Rigidbody2D playerRb;

    [Tooltip("Punto desde el cual se dispara el gancho")]
    [SerializeField] private Transform hookOrigin;

    [Tooltip("Representación visual del gancho")]
    [SerializeField] private Transform hookVisual;

    #endregion

    #region Hook Settings

    [Header("Hook Settings")]

    [Tooltip("Velocidad a la que viaja el gancho")]
    [SerializeField] private float hookSpeed = 20f;

    [Tooltip("Distancia máxima que puede recorrer el gancho")]
    [SerializeField] private float maxDistance = 10f;

    [Tooltip("Velocidad a la que el jugador es atraído al punto de enganche")]
    [SerializeField] private float pullSpeed = 20f;

    [Tooltip("Tiempo de reutilización del gancho")]
    [SerializeField] private float cooldown = 1f;

    [Tooltip("Tiempo máximo que el gancho puede permanecer activo antes de volver")]
    [SerializeField] private float maxHookTravelTime = 2f;

    #endregion

    #region Homing

    [Header("Homing")]

    [Tooltip("Radio dentro del cual se buscarán automáticamente puntos de enganche")]
    [SerializeField] private float homingRadius = 6f;

    #endregion

    #region Layers

    [Header("Layers")]

    [Tooltip("Capas con las que el gancho puede colisionar")]
    [SerializeField] private LayerMask collisionLayer;

    [Tooltip("Capas válidas para engancharse")]
    [SerializeField] private LayerMask hookLayer;

    #endregion

    #region Runtime

    /// <summary>
    /// Estado actual del gancho.
    /// </summary>
    private HookState currentState = HookState.Ready;

    /// <summary>
    /// Dirección actual de desplazamiento del gancho.
    /// </summary>
    private Vector2 hookDirection;

    /// <summary>
    /// Posición actual del gancho.
    /// </summary>
    private Vector2 hookPosition;

    /// <summary>
    /// Posición del punto de enganche.
    /// </summary>
    private Vector2 targetPosition;

    /// <summary>
    /// Distancia recorrida por el gancho.
    /// </summary>
    private float travelledDistance;

    /// <summary>
    /// Tiempo restante hasta que el gancho vuelva a estar disponible.
    /// </summary>
    private float cooldownTimer;

    /// <summary>
    /// Tiempo que lleva activo el lanzamiento actual.
    /// </summary>
    private float hookTravelTimer;

    /// <summary>
    /// Padre original del gancho visual.
    /// </summary>
    private Transform hookOriginalParent;

    #endregion

    #region Hook Point Tracking

    /// <summary>
    /// Puntos actualmente resaltados.
    /// </summary>
    private readonly HashSet<HookPoint> highlightedPoints = new HashSet<HookPoint>();

    /// <summary>
    /// Puntos detectados este frame.
    /// </summary>
    private readonly HashSet<HookPoint> detectedPoints = new HashSet<HookPoint>();

    #endregion

    #region Properties

    /// <summary>
    /// Indica si el jugador está siendo atraído hacia un punto de enganche.
    /// </summary>
    public bool IsPulling => currentState == HookState.Pulling;

    /// <summary>
    /// Indica si el gancho está realizando alguna acción.
    /// </summary>
    public bool IsBusy => currentState != HookState.Ready;

    /// <summary>
    /// Indica si el gancho puede dispararse.
    /// </summary>
    public bool CanFire => !IsBusy && cooldownTimer <= 0f;

    /// <summary>
    /// Indica si el frame actual corresponde al estado Pulling.
    /// </summary>
    public bool WasPullingThisStep { get; private set; }

    #endregion

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        // Guardamos el padre original del visual del gancho
        hookOriginalParent = hookVisual.parent;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Dispara el gancho en una dirección determinada.
    /// </summary>
    /// <param name="direction">Dirección inicial del disparo</param>
    /// <returns>
    /// True si el lanzamiento ha comenzado correctamente.
    /// </returns>
    public bool FireHook(Vector2 direction)
    {
        if (!CanFire)
            return false;

        if (direction.sqrMagnitude < 0.01f)
            return false;

        hookPosition = hookOrigin.position;
        travelledDistance = 0f;

        // Buscamos el punto de enganche más cercano
        HookPoint target = FindNearestHookPoint();

        if (target != null)
        {
            // Disparamos directamente hacia el HookPoint
            hookDirection = ((Vector2)target.transform.position - hookPosition).normalized;
        }
        else
        {
            // Si no hay ningún HookPoint cerca, usamos la dirección recibida
            hookDirection = direction.normalized;
        }

        currentState = HookState.Shooting;
        hookTravelTimer = 0f;

        UpdateHookVisual();
        hookVisual.SetParent(null, true);

        return true;
    }

    /// <summary>
    /// Actualiza la lógica del gancho durante FixedUpdate.
    /// </summary>
    public void TickFixed()
    {
        UpdateHookPointHighlights();

        WasPullingThisStep = currentState == HookState.Pulling;

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.fixedDeltaTime;

        switch (currentState)
        {
            case HookState.Shooting:
                Shoot();
                break;

            case HookState.Retracting:
                Retract();
                break;

            case HookState.Pulling:
                PullPlayer();
                break;
        }
    }

    /// <summary>
    /// Corta el gancho manualmente.
    /// </summary>
    public void CancelHook()
    {
        if (currentState == HookState.Ready)
            return;

        FinishHook();
    }

    #endregion

    #region Hook Logic

    /// <summary>
    /// Gestiona el viaje del gancho.
    /// </summary>
    private void Shoot()
    {
        hookTravelTimer += Time.fixedDeltaTime;

        // Si está mucho rato viajando, retraemos
        if (hookTravelTimer >= maxHookTravelTime)
        {
            currentState = HookState.Retracting;
            return;
        }

        // Calculamos cuanto debe avanzar
        float distance = hookSpeed * Time.fixedDeltaTime;

        distance = Mathf.Min(distance, maxDistance - travelledDistance);

        // Lanzamos un raycast, si golpea pared retraemos, si golpea enganche nos desplazamos hacia el
        RaycastHit2D hit = Physics2D.Raycast(hookPosition, hookDirection, distance, collisionLayer);

        if (hit.collider != null)
        {
            hookPosition = hit.point;
            travelledDistance += hit.distance;

            HookPoint point = hit.collider.GetComponentInParent<HookPoint>();

            bool validHook = point != null &&
                (hookLayer.value & (1 << hit.collider.gameObject.layer)) != 0;

            if (validHook)
            {
                targetPosition = point.transform.position;
                currentState = HookState.Pulling;
            }
            else
            {
                currentState = HookState.Retracting;
            }

            UpdateHookVisual();
            return;
        }

        hookPosition += hookDirection * distance;
        travelledDistance += distance;

        UpdateHookVisual();

        if (travelledDistance >= maxDistance)
            currentState = HookState.Retracting;
    }

    /// <summary>
    /// Hace volver al gancho.
    /// </summary>
    private void Retract()
    {
        hookPosition = Vector2.MoveTowards( hookPosition, hookOrigin.position, hookSpeed * Time.fixedDeltaTime);

        UpdateHookVisual();

        if (Vector2.Distance(hookPosition, hookOrigin.position) < 0.05f)
            FinishHook();
    }

    /// <summary>
    /// Desplaza al jugador hacia el punto de enganche.
    /// </summary>
    private void PullPlayer()
    {
        hookTravelTimer += Time.fixedDeltaTime;

        // Si está mucho rato intentando atraer, retraemos
        if (hookTravelTimer >= maxHookTravelTime)
        {
            currentState = HookState.Retracting;
            return;
        }

        // Hacemos que el jugador viaje hacia el enganche
        Vector2 playerPosition = playerRb.position;
        Vector2 direction = (targetPosition - playerPosition).normalized;

        float distance = Vector2.Distance(playerPosition, targetPosition);
        float movement = pullSpeed * Time.fixedDeltaTime;

        playerRb.linearVelocity = direction * pullSpeed;

        if (distance <= movement)
        {
            // Conservamos la velocidad de llegada.
            playerRb.linearVelocity = direction * pullSpeed;

            FinishHook();
            return;
        }
    }

    /// <summary>
    /// Hacemos un reset al gancho.
    /// </summary>
    private void FinishHook()
    {
        currentState = HookState.Ready;
        cooldownTimer = cooldown;

        hookPosition = hookOrigin.position;

        if (hookVisual != null)
        {
            hookVisual.SetParent(hookOriginalParent, true);
            hookVisual.localPosition = Vector3.zero;
        }

        hookPosition = hookOrigin.position;
        UpdateHookVisual();
    }

    #endregion

    #region Hook Point Search

    /// <summary>
    /// Busca el punto de enganche más cercano dentro del radio de homing.
    /// </summary>
    private HookPoint FindNearestHookPoint()
    {
        // Buscamos todos los enganches dentro del radio
        Collider2D[] colliders = Physics2D.OverlapCircleAll(hookOrigin.position, homingRadius, hookLayer);

        HookPoint closestPoint = null;
        float closestDistance = Mathf.Infinity;

        // Revisamos todos los enganches dentro del radio y devolvemos el más cercano
        foreach (Collider2D collider in colliders)
        {
            HookPoint point = collider.GetComponentInParent<HookPoint>();

            if (point == null)
                continue;

            float distance = Vector2.Distance(hookOrigin.position, point.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = point;
            }
        }

        return closestPoint;
    }

    /// <summary>
    /// Controla el resaltado de los puntos de enganche.
    /// </summary>
    private void UpdateHookPointHighlights()
    {
        detectedPoints.Clear();

        // Buscamos todos los enganches dentro del radio
        Collider2D[] colliders = Physics2D.OverlapCircleAll(hookOrigin.position, homingRadius, hookLayer);

        // Guardamos todos los enganches detectados
        foreach (Collider2D collider in colliders)
        {
            HookPoint point = collider.GetComponentInParent<HookPoint>();

            if (point != null)
                detectedPoints.Add(point);
        }

        // Desactivamos el color de los enganches que han salido del rango
        foreach (HookPoint point in highlightedPoints)
        {
            if (point != null && !detectedPoints.Contains(point))
                point.SetInRange(false);
        }

        // Activamos el color de los enganches que acaban de entrar en rango
        foreach (HookPoint point in detectedPoints)
        {
            if (point != null && !highlightedPoints.Contains(point))
                point.SetInRange(true);
        }

        // Reiniciamos y guardamos los que hemos marcado
        highlightedPoints.Clear();

        foreach (HookPoint point in detectedPoints)
            highlightedPoints.Add(point);
    }

    #endregion

    #region Visuals

    /// <summary>
    /// Actualiza la posición visual del gancho.
    /// </summary>
    private void UpdateHookVisual()
    {
        if (hookVisual != null)
            hookVisual.position = hookPosition;
    }

    #endregion
}