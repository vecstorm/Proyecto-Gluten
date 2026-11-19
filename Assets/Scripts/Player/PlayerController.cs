using System;
using UnityEngine;

/// <summary>
/// Define todos los estados en los que puede encontrarse el jugador.
/// </summary>
public enum PlayerState { Idle, Walk, Run, Jump, Airborne, DoubleJump, Dash, WallGrab, WallSlide, WallJump, Hook, Attack, Parry, Hurt, Dead }

/// <summary>
/// Gestiona todo el control del jugador mediante una máquina de estados.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputs))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerHook))]
[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(RespawnPointScript))]
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

    /// <summary>
    /// Se utiliza para guardar el estado previo del jugador.
    /// </summary>
    private PlayerState previousState;

    #endregion

    #region Components

    [Header("References")]

    [Tooltip("Gestor encargado de comprobar qué habilidades tiene desbloqueadas el jugador")]
    [SerializeField] private SkillUnlockManager skillManager;

    /// <summary>
    /// Referencia al Rigidbody2D utilizado para mover al personaje.
    /// </summary>
    private Rigidbody2D rb;

    /// <summary>
    /// Referencia al sistema de inputs del jugador.
    /// </summary>
    private PlayerInputs playerInputs;

    /// <summary>
    /// Referencia al sistema de gancho del jugador.
    /// </summary>
    private PlayerHook playerHook;

    /// <summary>
    /// Referencia al sistema de combate del jugador.
    /// </summary>
    private PlayerCombat playerCombat;

    /// <summary>
    /// Referencia al sistema de estadísticas del jugador.
    /// </summary>
    private CharacterStats characterStats;

    #endregion

    #region Movement

    [Header("Movement")]

    [Tooltip("Velocidad de movimiento caminando")]
    [SerializeField] private float walkSpeed = 5f;

    [Tooltip("Velocidad de movimiento corriendo")]
    [SerializeField] private float runSpeed = 7f;

    [Tooltip("Control horizontal en el aire")]
    [SerializeField] private float airborneSpeed = 5f;

    [Tooltip("Aceleración horizontal del jugador en el aire")]
    [SerializeField] private float airborneAcceleration = 100f;

    [Tooltip("Fuerza aplicada al salto")]
    [SerializeField] private float jumpForce = 16.5f;

    [Tooltip("Fuerza aplicada al doble salto")]
    [SerializeField] private float doubleJumpForce = 14f;

    [Tooltip("Indica si el jugador puede realizar un doble salto")]
    [SerializeField] private bool canDoubleJump;

    [Header("Coyote Time")]

    [Tooltip("Tiempo durante el que el jugador puede saltar después de abandonar el suelo")]
    [SerializeField, Min(0f)] private float coyoteTimeDuration = 0.10f;

    /// <summary>
    /// Tiempo restante para poder realizar un salto después de abandonar el suelo.
    /// </summary>
    private float coyoteTimeTimer;

    #endregion

    #region Hurt

    [Header("Damage")]

    [Tooltip("Tiempo que dura el estado de daño recibido")]
    [SerializeField] float hurtTimerDuration = 1f;

    /// <summary>
    /// Tiempo que queda de estado de daño recibido.
    /// </summary>
    private float hurtTimer;

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

    #endregion

    #region Hook

    [Tooltip("Duración de la inercia tras engancharse")]
    [SerializeField] private float hookMomentumDuration = 0.1f;

    /// <summary>
    /// Tiempo restante de duración de la inercia tras engancharse actual.
    /// </summary>
    private float hookMomentumTimer;

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
        characterStats = GetComponent<CharacterStats>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (playerInputs == null)
            playerInputs = GetComponent<PlayerInputs>();

        if (playerHook == null)
            playerHook = GetComponent<PlayerHook>();

        if (playerCombat == null)
            playerCombat = GetComponent<PlayerCombat>();
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
        playerInputs.ReadInputs();

        UpdateFacingDirection();
    }

    private void FixedUpdate()
    {
        if (hurtTimer > 0f)
            hurtTimer -= Time.fixedDeltaTime;

        playerInputs.UpdateInputBuffers();
        CheckGround();
        CheckWall();

        UpdateCooldowns();
        
        // Procesamos el gancho solo si el jugador lo tiene desbloqueado
        if (CanUseSkill("GANCHO"))
            playerHook.TickFixed();

        if (playerHook.WasPullingThisStep && !playerHook.IsPulling)
        {
            hookMomentumTimer = hookMomentumDuration;
        }

        // Mientras el gancho tira del jugador, no ejecutamos el movimiento normal de los estados
        if (playerHook.IsPulling || hookMomentumTimer > 0f)
        {
            hookMomentumTimer -= Time.fixedDeltaTime;

            // Permitimos procesar habilidades sin ejecutar el movimiento normal de Airborne
            CheckHookAbilities();
        }
        else
        {
            if (!TryCombatActions())
                UpdateState();
        }

        playerInputs.ConsumeJumpRelease();
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

    #region Direction

    /// <summary>
    /// Actualiza la dirección hacia la que mira el personaje.
    /// </summary>
    private void UpdateFacingDirection()
    {
        if (playerInputs.MovementDirection > 0)
            facingDirection = 1;
        else if (playerInputs.MovementDirection < 0)
            facingDirection = -1;

        playerCombat?.SetFacingDirection(facingDirection);
        characterStats?.SetFacingDirection(facingDirection);
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
        previousState = currentState;
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
                return IsOneOf(to, PlayerState.Walk, PlayerState.Run, PlayerState.Jump, PlayerState.Dash, PlayerState.Attack, PlayerState.Parry, PlayerState.Hook);

            case PlayerState.Walk:
                return IsOneOf(to, PlayerState.Idle, PlayerState.Run, PlayerState.Airborne, PlayerState.Jump, PlayerState.Dash, PlayerState.Attack, PlayerState.Parry, PlayerState.Hook);

            case PlayerState.Run:
                return IsOneOf(to, PlayerState.Idle, PlayerState.Walk, PlayerState.Airborne, PlayerState.Jump, PlayerState.Dash, PlayerState.Attack, PlayerState.Parry, PlayerState.Hook);

            case PlayerState.Jump:
                return IsOneOf(to, PlayerState.Airborne, PlayerState.DoubleJump, PlayerState.Dash, PlayerState.WallGrab, PlayerState.Hook, PlayerState.Attack, PlayerState.Parry);

            case PlayerState.Airborne:
                return IsOneOf(to, PlayerState.Idle, PlayerState.Walk, PlayerState.Jump, PlayerState.Run, PlayerState.DoubleJump, PlayerState.Dash, PlayerState.WallGrab, PlayerState.WallSlide, PlayerState.Hook, PlayerState.Attack, PlayerState.Parry);

            case PlayerState.DoubleJump:
                return IsOneOf(to, PlayerState.Airborne, PlayerState.Dash, PlayerState.WallGrab, PlayerState.Hook, PlayerState.Attack, PlayerState.Parry);

            case PlayerState.Dash:
                return IsOneOf(to, PlayerState.Idle, PlayerState.Walk, PlayerState.Run, PlayerState.Airborne, PlayerState.WallGrab, PlayerState.Hook, PlayerState.Attack);

            case PlayerState.WallGrab:
                return IsOneOf(to, PlayerState.Idle, PlayerState.Walk, PlayerState.WallSlide, PlayerState.WallJump, PlayerState.Airborne, PlayerState.Hook, PlayerState.Dash);

            case PlayerState.WallSlide:
                return IsOneOf(to, PlayerState.Idle, PlayerState.Walk, PlayerState.WallJump, PlayerState.WallGrab, PlayerState.Airborne, PlayerState.Hook, PlayerState.Dash);

            case PlayerState.WallJump:
                return IsOneOf(to, PlayerState.Airborne, PlayerState.Dash, PlayerState.DoubleJump, PlayerState.WallGrab, PlayerState.Hook, PlayerState.Attack, PlayerState.Parry);

            case PlayerState.Hook:
                return IsOneOf(to, PlayerState.Idle, PlayerState.Walk, PlayerState.Run, PlayerState.Airborne, PlayerState.WallGrab, PlayerState.Dash, PlayerState.DoubleJump, PlayerState.Attack, PlayerState.Parry);

            case PlayerState.Attack:
                return IsOneOf(to, PlayerState.Idle, PlayerState.Walk, PlayerState.Run, PlayerState.Airborne, PlayerState.Parry, PlayerState.Hook, PlayerState.Dash);

            case PlayerState.Parry:
                return IsOneOf(to, PlayerState.Idle, PlayerState.Walk, PlayerState.Run, PlayerState.Airborne, PlayerState.Attack);

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

                if (IsWallState(previousState))
                {
                    // Desde una pared, el dash siempre se dirige hacia fuera.
                    dashDirection = -facingDirection;
                }
                else
                {
                    dashDirection = playerInputs.MovementDirection;

                    if (Mathf.Abs(dashDirection) < 0.01f)
                        dashDirection = facingDirection;
                }

                rb.gravityScale = 0f;

                break;

            // Definimos timer y desactivamos la gravedad
            case PlayerState.WallGrab:

                wallGrabTimer = wallGrabDuration;

                rb.gravityScale = 0f;

                break;

            // Definimos velocidad y reactivamos la gravedad
            case PlayerState.WallSlide:

                wallSlideSpeed = 0f;

                rb.gravityScale = originalGravity;

                break;

            case PlayerState.Hook:

                Vector2 direction;

                if (IsWallState(previousState))
                {
                    // Desde una pared, lanzamos el gancho hacia fuera
                    direction = Vector2.left * facingDirection;
                }
                else
                {
                    direction = playerInputs.MoveInput;

                    // Evitamos lanzar hacia abajo
                    direction.y = Mathf.Max(0f, direction.y);

                    // Si no hay dirección, utilizamos la orientación del jugador
                    if (direction.sqrMagnitude < 0.01f)
                        direction = Vector2.right * facingDirection;
                }

                playerHook.FireHook(direction);

                break;

            case PlayerState.Attack:

                playerCombat?.PerformBasicAttack();

                break;

            case PlayerState.Parry:

                characterStats.BeginParry();

                break;

            case PlayerState.Hurt:

                rb.linearVelocity = Vector3.zero;
                hurtTimer = hurtTimerDuration;

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

            case PlayerState.Parry:

                characterStats.EndParry();

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
        // Guardamos si el jugador estaba tocando el suelo en el paso anterior
        bool wasGrounded = isGrounded;

        // Comprobamos si sigue tocando el suelo
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            // Recuperamos las habilidades al tocar el suelo
            canDash = true;
            canDoubleJump = true;

            // Mientras estamos en el suelo, mantenemos el Coyote Time preparado.
            coyoteTimeTimer = coyoteTimeDuration;
        }
        else
        {
            // Si acabamos de abandonar el suelo caminando o estando quietos, activamos la ventana de Coyote Time
            if (wasGrounded && (currentState == PlayerState.Idle || currentState == PlayerState.Walk || currentState == PlayerState.Run))
            {
                coyoteTimeTimer = coyoteTimeDuration;
            }
            else if (coyoteTimeTimer > 0f)
            {
                // Reducimos el tiempo disponible mientras estamos en el aire
                coyoteTimeTimer = Mathf.Max(0f, coyoteTimeTimer - Time.fixedDeltaTime);
            }
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

            case PlayerState.Attack:
                Attack();
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
        if (Mathf.Abs(playerInputs.MovementDirection) > 0.01f)
        {
            if (playerInputs.RunPressed)
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
    /// Se utiliza para saber si estamos en estado de pared.
    /// </summary>
    /// <param name="state">Estado a comprobar</param>
    /// <returns>El estado de pared en el que se encontraba el jugador</returns>
    private bool IsWallState(PlayerState state)
    {
        return state == PlayerState.WallGrab || state == PlayerState.WallSlide;
    }

    /// <summary>
    /// Intenta iniciar un salto si el jugador está en el suelo y se ha detectado una entrada de salto.
    /// </summary>
    /// <returns>
    /// True si el salto se ha iniciado correctamente.
    /// </returns>
    private bool TryJump()
    {
        if (!playerInputs.JumpPressed || (!isGrounded && coyoteTimeTimer <= 0f))
            return false;

        if (!ChangeState(PlayerState.Jump))
            return false;

        // Si hemos saltado desde el aire, consumimos el Coyote Time
        if (!isGrounded)
            coyoteTimeTimer = 0f;

        playerInputs.ConsumeJumpInput();

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
        if (!playerInputs.DashPressed || !canDash || dashCooldownTimer > 0)
            return false;

        if (!CanUseSkill("DASH"))
            return false;
        
        if (!ChangeState(PlayerState.Dash))
            return false;

        canDash = false;
        dashCooldownTimer = dashCooldown;

        playerInputs.ConsumeDashInput();

        return true;
    }

    /// <summary>
    /// Intenta lanzar el gancho si se cumplen todas las condiciones necesarias.
    /// </summary>
    /// <returns>
    /// True si el gancho se ha lanzado correctamente.
    /// </returns>
    private bool TryHook()
    {
        if (!playerInputs.HookPressed)
            return false;

        if (!CanUseSkill("GANCHO"))
            return false;

        if (!playerHook.CanFire)
            return false;

        if (!ChangeState(PlayerState.Hook))
            return false;

        playerInputs.ConsumeHookInput();

        return true;
    }

    /// <summary>
    /// Comprobamos si se pueden usar las habilidades mientras se lanza el gancho.
    /// </summary>
    private void CheckHookAbilities()
    {
        // Doble salto
        if (playerInputs.JumpPressed && canDoubleJump && CanUseSkill("DOUBLE_JUMP"))
        {
            playerHook.CancelHook();

            if (ChangeState(PlayerState.DoubleJump))
            {
                canDoubleJump = false;
                playerInputs.ConsumeJumpInput();
                return;
            }
        }

        // Dash
        if (playerInputs.DashPressed && canDash && dashCooldownTimer <= 0f && CanUseSkill("DASH"))
        {
            playerHook.CancelHook();

            TryDash();
        }
    }

    /// <summary>
    /// Comprueba si se puede ejecutar alguna acción de combate, el parry tiene prioridad sobre el ataque.
    /// </summary>
    private bool TryCombatActions()
    {
        if (playerCombat == null)
            return false;

        // El parry tiene prioridad si se pulsan ambos botones
        if (playerInputs.ParryPressed && characterStats.CanParry)
        {
            if (ChangeState(PlayerState.Parry))
            {
                playerInputs.ConsumeParryInput();
                playerInputs.ConsumeAttackInput();
                playerInputs.ConsumeRangedAttackInput();
            
                return true;
            }
        }

        // Ataque básico
        if (playerInputs.AttackPressed && playerCombat.CanAttack)
        {
            if (ChangeState(PlayerState.Attack))
            {
                playerInputs.ConsumeAttackInput();
                playerInputs.ConsumeRangedAttackInput();

                return true;
            }
        }

        // Ataque a distancia
        if (playerInputs.RangedAttackPressed)
        {
            if (playerCombat.PerformRangedAttack())
            {
                playerInputs.ConsumeRangedAttackInput();
            }
        }

        return false;
    }

    #endregion

    #region Character States

    #region Ground States

    /// <summary>
    /// Estado de reposo del jugador.
    /// </summary>
    private void Idle()
    {
        // Calculamos la velocidad horizontal deseada
        float targetSpeed = 0f;

        // Ajustamos progresivamente la velocidad horizontal
        float newSpeed = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, airborneAcceleration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);

        // En cuanto se mueve, pasamos a estado de movimiento
        if (Mathf.Abs(playerInputs.MovementDirection) > 0.01f)
        {
            if (playerInputs.RunPressed)
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

        if (TryHook())
            return;
    }

    /// <summary>
    /// Estado de desplazamiento básico.
    /// </summary>
    private void Walk()
    {
        // Actualizamos velocidad lineal a walkSpeed en X
        rb.linearVelocity = new Vector2(playerInputs.MovementDirection * walkSpeed, rb.linearVelocity.y);

        // Si se deja de mover, pasamos a estado de reposo
        if (Mathf.Abs(playerInputs.MovementDirection) < 0.01f)
        {
            ChangeState(PlayerState.Idle);
            return;
        }

        if (playerInputs.RunPressed)
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

        if (TryHook())
            return;
    }

    /// <summary>
    /// Estado de movimiento rápido en suelo.
    /// </summary>
    private void Run()
    {
        // Actualizamos velocidad lineal a runSpeed en X
        rb.linearVelocity = new Vector2(playerInputs.MovementDirection * runSpeed, rb.linearVelocity.y);
        
        // Si se deja de mover, pasamos a estado de reposo
        if (Mathf.Abs(playerInputs.MovementDirection) < 0.01f)
        {
            ChangeState(PlayerState.Idle);
            return;
        }

        if (!playerInputs.RunPressed)
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

        if (TryHook())
            return;
    }

    #endregion

    #region Air States

    /// <summary>
    /// Aplica el impulso inicial del salto.
    /// </summary>
    private void Jump()
    {
        // Actualizamos velocidad lineal a jumpForce en Y
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        ChangeState(PlayerState.Airborne);
    }

    /// <summary>
    /// Estado utilizado mientras el jugador permanece en el aire.
    /// </summary>
    private void Airborne()
    {
        // Calculamos la velocidad horizontal deseada
        float targetSpeed = playerInputs.MovementDirection * airborneSpeed;

        // Ajustamos progresivamente la velocidad horizontal
        float newSpeed = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, airborneAcceleration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);

        // Si se ha soltado el salto, bajamos antes
        if (playerInputs.JumpReleased && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);

            playerInputs.ConsumeJumpRelease();
        }

        // Intentamos realizar un salto normal mediante Coyote Time
        if (!isGrounded && TryJump())
            return;

        // Si se ha presionado el salto en el aire, se puede realizar un doble salto y tenemos la habilidad, se ejecuta
        if (playerInputs.JumpPressed && canDoubleJump && CanUseSkill("DOUBLE_JUMP"))
        {
            if (ChangeState(PlayerState.DoubleJump))
            {
                canDoubleJump = false;
                playerInputs.ConsumeJumpInput();
                return;
            }
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

        if (TryHook())
            return;
    }

    /// <summary>
    /// Estado encargado de realizar un segundo salto en el aire.
    /// </summary>
    private void DoubleJump()
    {
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

        if (playerInputs.JumpPressed && ChangeState(PlayerState.WallJump))
        {
            playerInputs.ConsumeJumpInput();
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

        if (playerInputs.MovementDirection != facingDirection)
        {
            ChangeState(PlayerState.WallSlide);
            return;
        }

        if (TryDash())
            return;

        if (TryHook())
            return;
    }

    /// <summary>
    /// Estado de deslizamiento por pared.
    /// </summary>
    private void WallSlide()
    {
        wallSlideSpeed += wallSlideAcceleration * Time.fixedDeltaTime;

        wallSlideSpeed = Mathf.Clamp(wallSlideSpeed, 0f, wallSlideMaxSpeed);

        rb.linearVelocity = new Vector2(0f, -wallSlideSpeed);

        if (playerInputs.JumpPressed && ChangeState(PlayerState.WallJump))
        {
            playerInputs.ConsumeJumpInput();
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

        if (TryDash())
            return;

        if (TryHook())
            return;
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
        if (!isGrounded)
        {
            ChangeState(PlayerState.Airborne);
        }
        else
            ChangeState(PlayerState.Idle);
    }

    #endregion

    #region Combat States

    /// <summary>
    /// Estado de ataque del jugador.
    /// </summary>
    private void Attack()
    {
        if (isGrounded)
        {
            ChangeToGroundState();
        }
        else
        {
            ChangeState(PlayerState.Airborne);
        }
    }

    /// <summary>
    /// Estado utilizado para realizar un parry.
    /// </summary>
    private void Parry()
    {
        // Cuando termina la ventana de parry, abandonamos el estado.
        if (playerCombat == null || !characterStats.IsParrying)
        {
            if (isGrounded)
            {
                ChangeToGroundState();
            }
            else
            {
                ChangeState(PlayerState.Airborne);
            }
        }
    }

    /// <summary>
    /// Estado de daño recibido.
    /// </summary>
    private void Hurt()
    {
        if (hurtTimer <= 0)
        {
            if (isGrounded)
                ChangeState(PlayerState.Idle);
            else
                ChangeState(PlayerState.Airborne);
        }
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