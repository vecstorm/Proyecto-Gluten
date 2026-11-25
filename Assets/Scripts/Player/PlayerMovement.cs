using UnityEngine;

/// <summary>
/// Gestiona la física y las mecánicas de movimiento del jugador.
/// No controla la máquina de estados: recibe instrucciones de PlayerController.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    #region Variables

    #region Components

    /// <summary>
    /// Referencia al Rigidbody2D utilizado para mover al personaje.
    /// </summary>
    private Rigidbody2D rb;

    /// <summary>
    /// Velocidad actual del jugador.
    /// </summary>
    public Vector2 Velocity => rb.linearVelocity;

    /// <summary>
    /// Gravedad original del Rigidbody2D.
    /// </summary>
    private float originalGravity;

    #endregion

    #region Movement

    [Header("Movement")]

    [Tooltip("Velocidad de movimiento caminando")]
    [SerializeField] private float walkSpeed = 8f;

    [Tooltip("Velocidad de movimiento corriendo")]
    [SerializeField] private float runSpeed = 10f;

    [Tooltip("Control horizontal en el aire")]
    [SerializeField] private float airborneSpeed = 8f;

    [Tooltip("Velocidad horizontal actual permitida en el aire")]
    private float currentAirborneSpeed;

    [Tooltip("Aceleración horizontal del jugador en el aire")]
    [SerializeField] private float airborneAcceleration = 100f;

    [Tooltip("Multiplicador de gravedad durante la caída")]
    [SerializeField, Min(1f)] private float fallGravityMultiplier = 2f;

    [Tooltip("Velocidad vertical máxima durante la caída")]
    [SerializeField, Min(0f)] private float maxFallSpeed = 15f;

    [Tooltip("Fuerza aplicada al salto")]
    [SerializeField] private float jumpForce = 16.5f;

    [Tooltip("Fuerza aplicada al doble salto")]
    [SerializeField] private float doubleJumpForce = 14f;

    [Tooltip("Indica si el jugador puede realizar un doble salto")]
    [SerializeField] private bool canDoubleJump;

    public float WalkSpeed => walkSpeed;

    public float RunSpeed => runSpeed;

    public bool CanDoubleJump => canDoubleJump;

    #endregion

    #region Coyote Time

    [Header("Coyote Time")]

    [Tooltip("Tiempo durante el que el jugador puede saltar después de abandonar el suelo")]
    [SerializeField, Min(0f)] private float coyoteTimeDuration = 0.10f;

    /// <summary>
    /// Tiempo restante para poder realizar un salto después de abandonar el suelo.
    /// </summary>
    private float coyoteTimeTimer;

    /// <summary>
    /// Indica si el jugador aun tiene tiempo para realizar el salto después de abandonar el suelo.
    /// </summary>
    public bool HasCoyoteTime => coyoteTimeTimer > 0f;

    #endregion

    #region Ground Check

    [Header("Ground Check")]

    [Tooltip("Indica si el personaje está tocando el suelo")]
    [SerializeField] private bool isGrounded;

    [Tooltip("Punto desde el cual se comprueba si el jugador está tocando el suelo")]
    [SerializeField] private Transform groundCheck;

    [Tooltip("Capa considerada como suelo")]
    [SerializeField] private LayerMask groundLayer;

    [Tooltip("Radio de detección del Ground Check")]
    [SerializeField] private float groundCheckRadius = 0.2f;

    public bool IsGrounded => isGrounded;

    #endregion

    #region Wall Check

    [Header("Wall Check")]

    [Tooltip("Indica si el personaje está tocando la pared")]
    [SerializeField] private bool isTouchingWall;

    [Tooltip("Punto desde el cual se comprueba si el jugador está tocando una pared")]
    [SerializeField] private Transform wallCheck;

    [Tooltip("Capa considerada como pared")]
    [SerializeField] private LayerMask wallLayer;

    [Tooltip("Distancia de detección del Wall Check")]
    [SerializeField] private float wallCheckDistance = 0.6f;

    public bool IsTouchingWall => isTouchingWall;

    #endregion

    #region Dash

    [Header("Dash")]

    [Tooltip("Velocidad del dash")]
    [SerializeField] private float dashSpeed = 20f;

    [Tooltip("Duración del dash en segundos")]
    [SerializeField] private float dashDuration = 0.15f;

    [Tooltip("Tiempo de espera entre dashes")]
    [SerializeField] private float dashCooldown = 0.4f;

    /// <summary>
    /// Tiempo restante hasta que el dash vuelva a estar disponible.
    /// </summary>
    private float dashCooldownTimer;

    /// <summary>
    /// Tiempo restante de duración del dash actual.
    /// </summary>
    private float dashTimer;

    /// <summary>
    /// Dirección en la que se realizará el dash.
    /// </summary>
    private float dashDirection;

    [Tooltip("Indica si el jugador puede realizar un dash")]
    [SerializeField] private bool canDash = true;

    /// <summary>
    /// Determina si el jugador puede utilizar el dash.
    /// </summary>
    public bool CanDash => canDash && dashCooldownTimer <= 0f;

    #endregion

    #region Wall Movement

    [Header("Wall")]

    [Tooltip("Duración del deslizamiento en la pared")]
    [SerializeField] private float wallGrabDuration = 0.5f;

    [Tooltip("Velocidad máxima de deslizamiento en la pared")]
    [SerializeField] private float wallSlideMaxSpeed = 3f;

    [Tooltip("Aceleración del deslizamiento en la pared")]
    [SerializeField] private float wallSlideAcceleration = 1.5f;

    [Tooltip("Fuerza aplicada al salto en la pared en X")]
    [SerializeField] private float wallJumpForceX = 15f;

    [Tooltip("Fuerza aplicada al salto en la pared en Y")]
    [SerializeField] private float wallJumpForceY = 12f;

    /// <summary>
    /// Tiempo restante de duración del wall grab actual.
    /// </summary>
    private float wallGrabTimer;

    /// <summary>
    /// Velocidad del wall slide actual.
    /// </summary>
    private float wallSlideSpeed;

    /// <summary>
    /// Indica si el timer de wall grab ha finalizado.
    /// </summary>
    public bool WallGrabFinished => wallGrabTimer <= 0f;

    #endregion

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalGravity = rb.gravityScale;

        currentAirborneSpeed = airborneSpeed;
    }

    #endregion

    #region Physics Checks

    /// <summary>
    /// Comprueba si el jugador está tocando el suelo y actualiza el Coyote Time.
    /// </summary>
    /// <param name="isInGroundState">Indica si el estado actual es Idle, Walk o Run</param>
    public void CheckGround(bool isInGroundState)
    {
        // Guardamos si el jugador estaba tocando el suelo en el paso anterior
        bool wasGrounded = isGrounded;

        // Comprobamos si sigue tocando el suelo
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            // Recuperamos las habilidades al tocar el suelo
            canDash = true;
            canDoubleJump = true;

            // Mientras estamos en el suelo, mantenemos el Coyote Time preparado
            coyoteTimeTimer = coyoteTimeDuration;
        }
        else
        {
            // Si acabamos de abandonar el suelo caminando o estando quietos, activamos la ventana de Coyote Time
            if (wasGrounded && isInGroundState)
            {
                coyoteTimeTimer = coyoteTimeDuration;
            }
            else if (coyoteTimeTimer > 0f)
            {
                // Reducimos el tiempo disponible mientras estamos en el aire
                coyoteTimeTimer = Mathf.Max(0f,coyoteTimeTimer - Time.fixedDeltaTime);
            }
        }
    }

    /// <summary>
    /// Comprueba si el jugador está tocando una pared.
    /// </summary>
    /// <param name="facingDirection">Dirección del jugador</param>
    public void CheckWall(int facingDirection)
    {
        isTouchingWall = Physics2D.Raycast(wallCheck.position, Vector2.right * facingDirection, wallCheckDistance, wallLayer);
    }

    /// <summary>
    /// Actualiza los tiempos de reutilización de las habilidades.
    /// </summary>
    public void UpdateCooldowns()
    {
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.fixedDeltaTime;
    }

    #endregion

    #region Movement Abilities

    /// <summary>
    /// Consume el Coyote Time después de realizar un salto desde el aire.
    /// </summary>
    public void ConsumeCoyoteTime()
    {
        coyoteTimeTimer = 0f;
    }

    /// <summary>
    /// Consume la posibilidad de realizar un doble salto.
    /// </summary>
    public void ConsumeDoubleJump()
    {
        canDoubleJump = false;
    }

    /// <summary>
    /// Recupera las habilidades de movimiento al agarrarse a una pared.
    /// </summary>
    public void RefreshAirAbilities()
    {
        canDoubleJump = true;
        canDash = true;
    }

    /// <summary>
    /// Consume la disponibilidad del dash e inicia su cooldown.
    /// </summary>
    public void ConsumeDash()
    {
        canDash = false;
        dashCooldownTimer = dashCooldown;
    }

    #endregion

    #region Ground Movement

    /// <summary>
    /// Aplica la desaceleración horizontal del estado Idle.
    /// </summary>
    public void ApplyIdleMovement()
    {
        // Ajustamos progresivamente la velocidad horizontal
        float newSpeed = Mathf.MoveTowards(rb.linearVelocity.x, 0f, airborneAcceleration * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);
    }

    /// <summary>
    /// Aplica la velocidad horizontal de caminar.
    /// </summary>
    /// <param name="movementDirection">Dirección del jugador</param>
    public void ApplyWalkMovement(float movementDirection)
    {
        // Actualizamos velocidad lineal a walkSpeed en X
        rb.linearVelocity = new Vector2(movementDirection * walkSpeed, rb.linearVelocity.y);
    }

    /// <summary>
    /// Aplica la velocidad horizontal de correr.
    /// </summary>
    /// <param name="movementDirection">Dirección del jugador</param>
    public void ApplyRunMovement(float movementDirection)
    {
        // Actualizamos velocidad lineal a runSpeed en X
        rb.linearVelocity = new Vector2(movementDirection * runSpeed, rb.linearVelocity.y);
    }

    #endregion

    #region Air Movement

    /// <summary>
    /// Aplica el impulso inicial del salto.
    /// </summary>
    public void ApplyJump()
    {
        rb.gravityScale = originalGravity;

        // Actualizamos velocidad lineal a jumpForce en Y
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    /// <summary>
    /// Aplica el impulso del doble salto.
    /// </summary>
    public void ApplyDoubleJump()
    {
        rb.gravityScale = originalGravity;

        // Actualizamos velocidad lineal a doubleJumpForce en Y
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
    }

    /// <summary>
    /// Controla el movimiento horizontal mientras el jugador está en el aire.
    /// </summary>
    /// <param name="movementDirection">Dirección del jugador</param>
    public void ApplyAirborneMovement(float movementDirection)
    {
        // Ajustamos la gravedad dependiendo de si ascendemos o descendemos
        if (rb.linearVelocity.y < 0f)
        {
            rb.gravityScale = originalGravity * fallGravityMultiplier;

            // Limitamos la velocidad máxima de caída
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = originalGravity;
        }

        // Control horizontal en el aire
        float targetSpeed = movementDirection * currentAirborneSpeed;

        float newSpeed = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, airborneAcceleration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);
    }

    /// <summary>
    /// Reduce la velocidad vertical al soltar el botón de salto.
    /// </summary>
    public void ApplyJumpCut()
    {
        if (rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

    /// <summary>
    /// Establece la velocidad horizontal permitida en el aire.
    /// </summary>
    /// <param name="speed">Velocidad a establecer</param>
    public void SetAirborneSpeed(float speed)
    {
        currentAirborneSpeed = speed;
    }

    #endregion

    #region Dash Movement

    /// <summary>
    /// Inicializa el dash y calcula su dirección.
    /// </summary>
    /// <param name="startedFromWall">Indica si el dash ha empezado desde un muro</param>
    /// <param name="facingDirection">Hacia donde está mirando el jugador</param>
    /// <param name="movementDirection">´Dirección del jugador</param>
    public void EnterDash(bool startedFromWall, int facingDirection, float movementDirection)
    {
        dashTimer = dashDuration;

        if (startedFromWall)
        {
            dashDirection = -facingDirection;
        }
        else
        {
            dashDirection = movementDirection;

            if (Mathf.Abs(dashDirection) < 0.01f)
                dashDirection = facingDirection;
        }

        rb.gravityScale = 0f;
    }

    /// <summary>
    /// Ejecuta el movimiento del dash.
    /// </summary>
    /// <returns>True cuando termina la duración del dash</returns>
    public bool ApplyDashMovement()
    {
        dashTimer -= Time.fixedDeltaTime;

        // Actualizamos velocidad lineal a dashSpeed en X
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        return dashTimer <= 0f;
    }

    #endregion

    #region Wall Movement

    /// <summary>
    /// Inicializa el estado de agarre a la pared.
    /// </summary>
    public void EnterWallGrab()
    {
        wallGrabTimer = wallGrabDuration;

        rb.gravityScale = 0f;

        RefreshAirAbilities();
    }

    /// <summary>
    /// Actualiza el tiempo de agarre y detiene el movimiento.
    /// </summary>
    public void ApplyWallGrab()
    {
        wallGrabTimer -= Time.fixedDeltaTime;

        StopMovement();
    }

    /// <summary>
    /// Inicializa el deslizamiento por la pared.
    /// </summary>
    public void EnterWallSlide()
    {
        wallSlideSpeed = 0f;

        rb.gravityScale = originalGravity;
    }

    /// <summary>
    /// Aplica el movimiento de deslizamiento por la pared.
    /// </summary>
    public void ApplyWallSlide()
    {
        wallSlideSpeed += wallSlideAcceleration * Time.fixedDeltaTime;

        wallSlideSpeed = Mathf.Clamp(wallSlideSpeed, 0f, wallSlideMaxSpeed);

        rb.linearVelocity = new Vector2(0f, -wallSlideSpeed);
    }

    /// <summary>
    /// Aplica el impulso del salto desde una pared.
    /// </summary>
    public void ApplyWallJump(int facingDirection)
    {
        rb.linearVelocity = new Vector2(-facingDirection * wallJumpForceX, wallJumpForceY);
    }

    #endregion

    #region General Physics

    /// <summary>
    /// Restaura la gravedad original del Rigidbody2D.
    /// </summary>
    public void RestoreGravity()
    {
        rb.gravityScale = originalGravity;
    }

    /// <summary>
    /// Detiene completamente el movimiento del jugador.
    /// </summary>
    public void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// Detiene únicamente el movimiento horizontal del jugador.
    /// </summary>
    public void StopHorizontalMovement()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * wallCheckDistance);

            Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.left * wallCheckDistance);
        }
    }

    #endregion
}