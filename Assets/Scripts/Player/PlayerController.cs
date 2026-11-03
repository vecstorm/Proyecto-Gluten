using System;
using UnityEngine;

/// <summary>
/// Define todos los estados en los que puede encontrarse el jugador.
/// </summary>
public enum PlayerState { Idle, Walk, Run, Jump, Airborne, DoubleJump, Dash, WallGrab, WallSlide, WallJump, Hook, Crouch, Attack, Defend, Parry, Hurt, Dead }

public class PlayerController : MonoBehaviour
{
    #region Variables

    #region State Machine

    [Header("State")]

    [Tooltip("Estado actual de la máquina de estados del jugador")]
    [SerializeField] private PlayerState currentState = PlayerState.Idle;

    /// <summary>
    /// Devuelve el estado actual del jugador en modo lectura.
    /// </summary>
    public PlayerState CurrentState => currentState;

    #endregion

    #region Components

    [Header("References")]

    [Tooltip("Referencia al Rigidbody2D utilizado para mover al personaje")]
    [SerializeField] private Rigidbody2D rb;

    /// <summary>
    /// Gestor encargado de comprobar qué habilidades tiene desbloqueadas el jugador.
    /// </summary>
    [SerializeField] private SkillUnlockManager skillManager;

    #endregion

    #region Inputs

    /// <summary>
    /// Referencia al sistema de inputs del jugador.
    /// </summary>
    private GlutenActionControls glutenActionControls;

    /// <summary>
    /// Dirección horizontal introducida por el jugador. Valores entre -1 y 1.
    /// </summary>
    private float movementDirection;

    /// <summary>
    /// Indica si se ha pulsado el salto durante este frame.
    /// </summary>
    private bool jumpPressed;

    /// <summary>
    /// Indica si se ha soltado el salto durante este frame.
    /// </summary>
    private bool jumpReleased;

    /// <summary>
    /// Indica si el jugador mantiene pulsado el botón de correr.
    /// </summary>
    private bool runPressed;

    /// <summary>
    /// Indica si se ha solicitado realizar un dash.
    /// </summary>
    private bool dashPressed;

    #endregion

    #region Movement

    [Header("Movement")]

    [Tooltip("Velocidad de movimiento caminando")]
    [SerializeField] private float walkSpeed = 5f;

    [Tooltip("Velocidad de movimiento corriendo")]
    [SerializeField] private float runSpeed = 7f;

    [Tooltip("Control horizontal en el aire")]
    [SerializeField] private float airborneSpeed = 5f;

    [Tooltip("Fuerza aplicada al salto")]
    [SerializeField] private float jumpForce = 16.5f;

    [Tooltip("Fuerza aplicada al doble salto")]
    [SerializeField] private float doubleJumpForce = 14f;

    [Tooltip("Indica si el jugador puede realizar un doble salto")]
    [SerializeField] private bool canDoubleJump;

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

    #endregion

    #region Dash

    [Header("Dash")]

    [Tooltip("Velocidad del dash")]
    [SerializeField] private float dashSpeed = 30f;

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

    /// <summary>
    /// Determina si el jugador puede utilizar el dash.
    /// </summary>
    private bool canDash;

    #endregion

    #region Wall

    [Header("Wall")]

    [Tooltip("Duración del deslizamiento en la pared")]
    [SerializeField] private float wallGrabDuration = 0.5f;

    [Tooltip("Velocidad de deslizamiento en la pared")]
    [SerializeField] private float wallSlideMaxSpeed = 3f;

    [Tooltip("Velocidad de deslizamiento en la pared")]
    [SerializeField] private float wallSlideAcceleration = 1.5f;

    [Tooltip("Fuerza aplicada al salto en la pared en X")]
    [SerializeField] private float wallJumpForceX = 30f;

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

    #endregion

    #region Orientation

    /// <summary>
    /// Dirección hacia la que mira el personaje. 1 = derecha, -1 = izquierda. 
    /// </summary>
    private int facingDirection = 1;

    #endregion

    #region Physics

    /// <summary>
    /// Valor de gravedad original del Rigidbody. Se restaura al finalizar un dash.
    /// </summary>
    private float originalGravity;

    #endregion

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        glutenActionControls = new GlutenActionControls();
    }

    private void OnEnable()
    {
        glutenActionControls.Enable();
    }

    private void Start()
    {
        // Marcamos el dash como usable y guardamos la gravedad del player
        canDash = true;
        originalGravity = rb.gravityScale;

        EnterState(currentState);
    }

    private void Update()
    {
        ReadInputs();
        UpdateFacingDirection();
    }

    private void FixedUpdate()
    {
        CheckGround();
        CheckWall();

        UpdateCooldowns();
        UpdateState();

        // Desactivamos los indicadores de salto y dash
        jumpPressed = false;
        jumpReleased = false;
        dashPressed = false;
    }

    private void OnDisable()
    {
        glutenActionControls.Disable();
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * facingDirection * wallCheckDistance);
    }

    #endregion

    #region Inputs

    /// <summary>
    /// Lee todos los inputs del jugador.
    /// </summary>
    private void ReadInputs()
    {
        movementDirection = glutenActionControls.Player.Move.ReadValue<Vector2>().x;

        runPressed = glutenActionControls.Player.Sprint.IsPressed();

        if (glutenActionControls.Player.Jump.WasPressedThisFrame())
            jumpPressed = true;

        if (glutenActionControls.Player.Jump.WasReleasedThisFrame())
            jumpReleased = true;

        if (glutenActionControls.Player.Dash.WasPressedThisFrame())
            dashPressed = true;
    }

    /// <summary>
    /// Actualiza la dirección hacia la que mira el personaje.
    /// </summary>
    private void UpdateFacingDirection()
    {
        if (movementDirection > 0)
            facingDirection = 1;
        else if (movementDirection < 0)
            facingDirection = -1;
    }

    #endregion

    #region State Machine

    /// <summary>
    /// Intenta realizar una transición al estado indicado si está permitida.
    /// </summary>
    /// <param name="newState">Estado al que se desea cambiar</param>
    /// <returns>
    /// True si la transición se ha realizado correctamente.
    /// </returns>
    public bool ChangeState(PlayerState newState)
    {
        //Si el nuevo estado es el mismo al estado actual, no hace nada
        if (newState == currentState)
            return false;

        //Si la transición no está permitida, avisamos
        if (!IsTransitionAllowed(currentState, newState))
        {
            Debug.LogWarning($"Transición no permitida: {currentState} -> {newState}", this);
            return false;
        }

        // Actualizamos el estado
        ExitState(currentState);
        currentState = newState;
        EnterState(currentState);

        return true;
    }

    /// <summary>
    /// Comprueba si una transición entre dos estados es válida según
    /// las reglas definidas por la máquina de estados.
    /// </summary>
    /// <param name="from">Estado de origen</param>
    /// <param name="to">Estado de destino</param>
    /// <returns>
    /// True si la transición está permitida.
    /// </returns> 
    public bool IsTransitionAllowed(PlayerState from, PlayerState to)
    {
        // No permitimos cambios entre el mismo estado
        if (from == to) 
            return false;

        // Desde cualquier estado excepto Hurt y Dead, el jugador puede pasar a Hurt o Dead
        // Ejemplo: no puede pasar de dead a hurt porque en esta condición no está permitido
        if (from != PlayerState.Hurt && from != PlayerState.Dead && (to == PlayerState.Hurt || to == PlayerState.Dead))
            return true;

        // Lista de transiciones entre estados
        switch (from)
        {
            case PlayerState.Idle:
                return IsOneOf(to, PlayerState.Walk, PlayerState.Run, PlayerState.Jump, PlayerState.Dash, PlayerState.Crouch, PlayerState.Attack, PlayerState.Defend, PlayerState.Hook);

            case PlayerState.Walk:
                return IsOneOf(to, PlayerState.Idle, PlayerState.Run, PlayerState.Airborne, PlayerState.Jump, PlayerState.Dash, PlayerState.Crouch, PlayerState.Attack, PlayerState.Defend, PlayerState.Hook);

            case PlayerState.Run:
                return IsOneOf(to, PlayerState.Idle, PlayerState.Walk, PlayerState.Airborne, PlayerState.Jump, PlayerState.Dash, PlayerState.Crouch, PlayerState.Attack, PlayerState.Defend, PlayerState.Hook);

            case PlayerState.Jump:
                return IsOneOf(to, PlayerState.Airborne, PlayerState.DoubleJump, PlayerState.Dash, PlayerState.WallGrab, PlayerState.Hook, PlayerState.Attack);

            case PlayerState.Airborne:
                return IsOneOf(to, PlayerState.Idle, PlayerState.Walk, PlayerState.Run, PlayerState.DoubleJump, PlayerState.Dash, PlayerState.WallGrab, PlayerState.WallSlide, PlayerState.Hook, PlayerState.Attack);

            case PlayerState.DoubleJump:
                return IsOneOf(to, PlayerState.Airborne, PlayerState.Dash, PlayerState.WallGrab, PlayerState.Hook, PlayerState.Attack);

            case PlayerState.Dash:
                return IsOneOf(to, PlayerState.Idle, PlayerState.Walk, PlayerState.Run, PlayerState.Airborne, PlayerState.WallGrab, PlayerState.Hook);

            case PlayerState.WallGrab:
                return IsOneOf(to, PlayerState.Idle, PlayerState.Walk, PlayerState.WallSlide, PlayerState.WallJump, PlayerState.Airborne, PlayerState.Hook, PlayerState.Dash);

            case PlayerState.WallSlide:
                return IsOneOf(to, PlayerState.Idle, PlayerState.Walk, PlayerState.WallJump, PlayerState.WallGrab, PlayerState.Airborne, PlayerState.Hook, PlayerState.Dash);

            case PlayerState.WallJump:
                return IsOneOf(to, PlayerState.Airborne, PlayerState.Dash, PlayerState.DoubleJump, PlayerState.WallGrab, PlayerState.Hook, PlayerState.Attack);

            case PlayerState.Hook:
                return IsOneOf(to, PlayerState.Idle, PlayerState.Walk, PlayerState.Run, PlayerState.Airborne, PlayerState.WallGrab, PlayerState.Dash, PlayerState.Attack);

            case PlayerState.Crouch:
                return IsOneOf(to, PlayerState.Idle, PlayerState.Walk, PlayerState.Attack, PlayerState.Hook, PlayerState.Dash);

            case PlayerState.Attack:
                return IsOneOf(to, PlayerState.Idle, PlayerState.Walk, PlayerState.Run, PlayerState.Airborne, PlayerState.Crouch, PlayerState.Defend, PlayerState.Parry, PlayerState.Hook, PlayerState.Dash);

            case PlayerState.Defend:
                return IsOneOf(to, PlayerState.Idle, PlayerState.Walk, PlayerState.Parry);

            case PlayerState.Parry:
                return IsOneOf(to, PlayerState.Defend, PlayerState.Idle, PlayerState.Walk, PlayerState.Attack);

            case PlayerState.Hurt:
                return IsOneOf(to, PlayerState.Idle, PlayerState.Walk, PlayerState.Run, PlayerState.Airborne, PlayerState.WallGrab, PlayerState.WallSlide, PlayerState.Dead);

            // Se permite volver a idle despues de reaparecer en un checkpoint
            case PlayerState.Dead:
                return to == PlayerState.Idle;

            default:
                return false;
        }
    }

    /// <summary>
    /// Comprueba si un estado pertenece a una lista de estados permitidos.
    /// </summary>
    /// <param name="value">Estado a comprobar</param>
    /// <param name="options">Lista de estados válidos</param>
    /// <returns>
    /// True si el estado se encuentra dentro de la lista.
    /// </returns>
    private bool IsOneOf(PlayerState value, params PlayerState[] options)
    {
        // Devuelve true siempre que los estados que se escojan estén dentro de la lista
        foreach (PlayerState option in options)
        {
            if (value == option)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Ejecuta la lógica necesaria al entrar en un nuevo estado.
    /// </summary>
    /// <param name="state">Estado al que se accede</param>
    private void EnterState(PlayerState state)
    {
        switch (state)
        {
            // Definimos timer, dirección y desactivamos la gravedad
            case PlayerState.Dash:

                dashTimer = dashDuration;
                dashDirection = movementDirection;

                if (Mathf.Abs(dashDirection) < 0.01f)
                    dashDirection = facingDirection;

                rb.gravityScale = 0f;

                break;

            // Definimos timer y desactivamos la gravedad
            case PlayerState.WallGrab:

                wallGrabTimer = wallGrabDuration;

                rb.gravityScale = 0f;

                break;

            // Definimos velocidad y reacti la gravedad
            case PlayerState.WallSlide:

                wallSlideSpeed = 0f;

                rb.gravityScale = originalGravity;

                break;
        }
    }

    /// <summary>
    /// Ejecuta la lógica necesaria al abandonar un estado.
    /// </summary>
    /// <param name="state">Estado que se abandona</param>
    private void ExitState(PlayerState state)
    {
        switch (state)
        {
            // Reiniciamos la gravedad en los estados necesarios
            case PlayerState.Dash:

                rb.gravityScale = originalGravity;

                break;

            case PlayerState.WallGrab:

                rb.gravityScale = originalGravity;

                break;
        }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Comprueba si una habilidad está desbloqueada.
    /// </summary>
    /// <param name="skill">Nombre de la habilidad</param>
    /// <returns>True si la habilidad está desbloqueada.</returns>
    private bool CanUseSkill(string skill)
    {
        return skillManager.IsSkillUnlocked(skill);
    }

    /// <summary>
    /// Comprueba si el jugador está tocando el suelo.
    /// </summary>
    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            canDash = true;
            canDoubleJump = true;
        }
    }

    /// <summary>
    /// Comprueba si el jugador está tocando la pared.
    /// </summary>
    private void CheckWall()
    {
        isTouchingWall = Physics2D.Raycast(wallCheck.position, Vector2.right * facingDirection, wallCheckDistance, wallLayer);
    }

    /// <summary>
    /// Actualiza los temporizadores de habilidades.
    /// </summary>
    private void UpdateCooldowns()
    {
        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.fixedDeltaTime;
    }

    /// <summary>
    /// Ejecuta la lógica correspondiente al estado actual.
    /// </summary>
    private void UpdateState()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                Idle();
                break;

            case PlayerState.Walk:
                Walk();
                break;

            case PlayerState.Run:
                Run();
                break;

            case PlayerState.Jump:
                Jump();
                break;

            case PlayerState.Airborne:
                Airborne();
                break;

            case PlayerState.DoubleJump:
                DoubleJump();
                break;

            case PlayerState.Dash:
                Dash();
                break;

            case PlayerState.WallGrab:
                WallGrab();
                break;

            case PlayerState.WallSlide:
                WallSlide();
                break;

            case PlayerState.WallJump:
                WallJump();
                break;

            case PlayerState.Hook:
                Hook();
                break;

            case PlayerState.Crouch:
                Crouch();
                break;

            case PlayerState.Attack:
                Attack();
                break;

            case PlayerState.Defend:
                Defend();
                break;

            case PlayerState.Parry:
                Parry();
                break;

            case PlayerState.Hurt:
                Hurt();
                break;

            case PlayerState.Dead:
                Dead();
                break;
        }
    }

    /// <summary>
    /// Cambia al estado Idle, Walk o Run dependiendo de la dirección de movimiento y del estado de carrera.
    /// </summary>
    private void ChangeToGroundState()
    {
        if (Mathf.Abs(movementDirection) > 0.01f)
        {
            if (runPressed)
                ChangeState(PlayerState.Run);
            else
                ChangeState(PlayerState.Walk);
        }
        else
        {
            ChangeState(PlayerState.Idle);
        }
    }

    /// <summary>
    /// Intenta iniciar un salto si el jugador está en el suelo y se ha detectado una entrada de salto.
    /// </summary>
    /// <returns>
    /// True si el salto se ha iniciado correctamente.
    /// </returns>
    private bool TryJump()
    {
        if (!jumpPressed || !isGrounded)
            return false;

        jumpPressed = false;
        ChangeState(PlayerState.Jump);

        return true;
    }

    /// <summary>
    /// Intenta iniciar un dash si se cumplen todas las condiciones necesarias.
    /// </summary>
    /// <returns>
    /// True si el dash se ha iniciado correctamente.
    /// </returns>
    private bool TryDash()
    {
        if (!dashPressed || !canDash || dashCooldownTimer > 0)
            return false;

        if (!CanUseSkill("DASH"))
            return false;

        canDash = false;
        dashPressed = false;
        dashCooldownTimer = dashCooldown;

        ChangeState(PlayerState.Dash);

        return true;
    }

    #endregion

    #region Character States

    #region Ground States

    /// <summary>
    /// Estado de reposo del jugador.
    /// </summary>
    private void Idle()
    {
        // Actualizamos velocidad lineal a 0 en X
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // En cuanto se mueve, pasamos a estado de movimiento
        if (Mathf.Abs(movementDirection) > 0.01f)
        {
            if (runPressed)
            {
                ChangeState(PlayerState.Run);
                return;
            }
            else
            {
                ChangeState(PlayerState.Walk);
                return;
            }
        }

        if (TryJump())
            return;

        if (TryDash())
            return;
    }

    /// <summary>
    /// Estado de desplazamiento básico.
    /// </summary>
    private void Walk()
    {
        // Actualizamos velocidad lineal a walkSpeed en X
        rb.linearVelocity = new Vector2(movementDirection * walkSpeed, rb.linearVelocity.y);

        // Si se deja de mover, pasamos a estado de reposo
        if (Mathf.Abs(movementDirection) < 0.01f)
        {
            ChangeState(PlayerState.Idle);
            return;
        }

        if (runPressed)
        {
            ChangeState(PlayerState.Run);
            return;
        }

        if (!isGrounded)
        {
            ChangeState(PlayerState.Airborne);
            return;
        }

        if (TryJump())
            return;

        if (TryDash())
            return;
    }

    /// <summary>
    /// Estado de movimiento rápido en suelo.
    /// </summary>
    private void Run()
    {
        // Actualizamos velocidad lineal a runSpeed en X
        rb.linearVelocity = new Vector2(movementDirection * runSpeed, rb.linearVelocity.y);
        
        // Si se deja de mover, pasamos a estado de reposo
        if (Mathf.Abs(movementDirection) < 0.01f)
        {
            ChangeState(PlayerState.Idle);
            return;
        }

        if (!runPressed)
        {
            ChangeState(PlayerState.Walk);
            return;
        }

        if (!isGrounded)
        {
            ChangeState(PlayerState.Airborne);
            return;
        }

        if (TryJump())
            return;

        if (TryDash())
            return;
    }

    /// <summary>
    /// Estado de agacharse.
    /// </summary>
    private void Crouch()
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Air States

    /// <summary>
    /// Aplica el impulso inicial del salto.
    /// </summary>
    private void Jump()
    {
        jumpPressed = false;

        // Actualizamos velocidad lineal a jumpForce en Y
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        ChangeState(PlayerState.Airborne);
    }

    /// <summary>
    /// Estado utilizado mientras el jugador permanece en el aire.
    /// </summary>
    private void Airborne()
    {
        // Actualizamos velocidad lineal a airborneSpeed en X
        rb.linearVelocity = new Vector2(movementDirection * airborneSpeed, rb.linearVelocity.y);

        // Si se ha soltado el salto, bajamos antes
        if (jumpReleased && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);

            jumpReleased = false;
        }

        // Si se ha presionado el salto en el aire, se puede realizar un doble salto y tenemos la habilidad, se ejecuta
        if (jumpPressed && canDoubleJump && CanUseSkill("DOUBLE_JUMP"))
        {
            canDoubleJump = false;
            ChangeState(PlayerState.DoubleJump);
            return;
        }

        // Si estamos tocando la pared, no estamos en el suelo y tenemos la habilidad, hacemos wall grab
        if (isTouchingWall && !isGrounded && rb.linearVelocity.y <= 0 && CanUseSkill("WALL_SKILL"))
        {
            ChangeState(PlayerState.WallGrab);
            return;
        }

        // Si dejamos de caer, pasamos a un estado terrestre
        if (isGrounded && rb.linearVelocity.y <= 0)
        {
            ChangeToGroundState();
            return;
        }

        if (TryDash())
            return;
    }

    /// <summary>
    /// Estado encargado de realizar un segundo salto en el aire.
    /// </summary>
    private void DoubleJump()
    {
        jumpPressed = false;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);

        ChangeState(PlayerState.Airborne);
    }

    /// <summary>
    /// Estado en el que el jugador se agarra a una pared.
    /// </summary>
    private void WallGrab()
    {
        canDoubleJump = true;
        canDash = true;

        wallGrabTimer -= Time.fixedDeltaTime;

        rb.linearVelocity = Vector2.zero;

        if (jumpPressed)
        {
            ChangeState(PlayerState.WallJump);
            return;
        }

        if (!isTouchingWall)
        {
            ChangeState(PlayerState.Airborne);
            return;
        }

        if (isGrounded)
        {
            ChangeToGroundState();
            return;
        }

        if (wallGrabTimer <= 0)
        {
            ChangeState(PlayerState.WallSlide);
            return;
        }

        if (movementDirection != facingDirection)
        {
            ChangeState(PlayerState.WallSlide);
            return;
        }
    }

    /// <summary>
    /// Estado de deslizamiento por pared.
    /// </summary>
    private void WallSlide()
    {
        wallSlideSpeed += wallSlideAcceleration * Time.fixedDeltaTime;

        wallSlideSpeed = Mathf.Clamp(wallSlideSpeed, 0f, wallSlideMaxSpeed);

        rb.linearVelocity = new Vector2(0f, -wallSlideSpeed);

        if (jumpPressed)
        {
            ChangeState(PlayerState.WallJump);
            return;
        }

        if (!isTouchingWall)
        {
            ChangeState(PlayerState.Airborne);
            return;
        }

        if (isGrounded)
        {
            ChangeToGroundState();
            return;
        }
    }

    /// <summary>
    /// Estado que ejecuta un salto desde una pared.
    /// </summary>
    private void WallJump()
    {
        rb.linearVelocity = new Vector2(-facingDirection * wallJumpForceX, wallJumpForceY);

        ChangeState(PlayerState.Airborne);
    }

    #endregion

    #region Special States

    /// <summary>
    /// Estado de movimiento rápido horizontal que ignora la gravedad temporalmente.
    /// </summary>
    private void Dash()
    {
        dashTimer -= Time.fixedDeltaTime;

        // Actualizamos velocidad lineal a dashSpeed en X
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        if (dashTimer <= 0f)
        {
            if (isGrounded)
            {
                ChangeToGroundState();
                return;
            }
            else
            {
                ChangeState(PlayerState.Airborne);
                return;
            }
        }
    }

    /// <summary>
    /// Estado asociado al uso del gancho.
    /// </summary>
    private void Hook()
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Combat States

    /// <summary>
    /// Estado de ataque del jugador.
    /// </summary>
    private void Attack()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Estado defensivo.
    /// </summary>
    private void Defend()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Estado utilizado para realizar un parry.
    /// </summary>
    private void Parry()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Estado de daño recibido.
    /// </summary>
    private void Hurt()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Estado de muerte del jugador.
    /// </summary>
    private void Dead()
    {
        throw new NotImplementedException();
    }

    #endregion

    #endregion
}