using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState { Idle, Walk, Run, Jump, Airborne, DoubleJump, Dash, WallGrab, WallSlide, WallJump, Hook, Crouch, Attack, Defend, Parry, Hurt, Dead }

    [SerializeField] private PlayerState currentState = PlayerState.Idle;
    public PlayerState CurrentState => currentState;

    private GlutenActionControls glutenActionControls;
    private float movementDirection;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float airborneSpeed = 4f;
    [SerializeField] private float runSpeed = 7f;

    [SerializeField] private float jumpForce = 8f;
    private bool jumpPressed;
    private bool runPressed;
    private bool dashPressed;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [SerializeField] private bool isGrounded;

    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;

    private float dashTimer;
    [SerializeField] private float dashCooldown = 0.5f;
    private float dashCooldownTimer;
    private float dashDirection;
    private bool canDash;

    private int facingDirection = 1;

    private float originalGravity;

    [SerializeField] private SkillUnlockManager skillManager;

    private void Awake()
    {
        glutenActionControls = new GlutenActionControls();
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    private void OnEnable()
    {
        glutenActionControls.Enable();
    }

    private void OnDisable()
    {
        glutenActionControls.Disable();
    }

    private void Start()
    {
        canDash = true;
        originalGravity = rb.gravityScale;
        EnterState(currentState);
    }

    private void Update()
    {
        movementDirection = glutenActionControls.Player.Move.ReadValue<Vector2>().x;

        if (movementDirection > 0)
        {
            facingDirection = 1;
        }
        else if (movementDirection < 0)
        {
            facingDirection = -1;
        }

        if (glutenActionControls.Player.Jump.WasPressedThisFrame())
        {
            jumpPressed = true;
        }

        runPressed = glutenActionControls.Player.Sprint.IsPressed();

        if (glutenActionControls.Player.Dash.WasPressedThisFrame())
        {
            dashPressed = true;
        }
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            canDash = true;
        }

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.fixedDeltaTime;
        }

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

        jumpPressed = false;
        dashPressed = false;
    }

    public bool ChangeState(PlayerState newState)
    {
        if (newState == currentState) //Si el nuevo estado es el mismo al estado actual, no hace nada
            return false;

        if (!IsTransitionAllowed(currentState, newState)) //SI la transicion no esta permitida, avisa
        {
            Debug.LogWarning($"Transición no permitida: {currentState} -> {newState}", this);
            return false;
        }

        ExitState(currentState); //Limpia acciones del estado anterior
        currentState = newState; //Actualiza el estado
        EnterState(currentState); //Entra en el nuevo estado
        return true;
    }

    //Metodo que comprueba 
    public bool IsTransitionAllowed(PlayerState from, PlayerState to)
    {
        if (from == to) //Para evitar que el personaje cambie a un estado el cual ya esta
            return false;

        //Desde cualquier estado excepto Hurt y Dead, el jugador puede pasar a Hurt o Dead
        ///Ejemplo: no puede pasar de dead a hurt porque en esta condicion no esta permitido
        ///Por ello no hace falta poner en el switch las condiciones de dead y hurt, se controlan aqui
        if (from != PlayerState.Hurt && from != PlayerState.Dead && (to == PlayerState.Hurt || to == PlayerState.Dead))
            return true;

        //Aqui esta la lista de estados a los cuales desde uno inicial se puede pasar a otro.
        switch (from)
        {
            case PlayerState.Idle:
                return IsOneOf(to,
                    PlayerState.Walk, PlayerState.Run, PlayerState.Jump,
                    PlayerState.Dash, PlayerState.Crouch, PlayerState.Attack,
                    PlayerState.Defend, PlayerState.Hook);

            case PlayerState.Walk:
                return IsOneOf(to,
                    PlayerState.Idle, PlayerState.Run, PlayerState.Jump,
                    PlayerState.Dash, PlayerState.Crouch, PlayerState.Attack,
                    PlayerState.Defend, PlayerState.Hook);

            case PlayerState.Run:
                return IsOneOf(to,
                    PlayerState.Idle, PlayerState.Walk, PlayerState.Jump,
                    PlayerState.Dash, PlayerState.Crouch, PlayerState.Attack,
                    PlayerState.Defend, PlayerState.Hook);

            case PlayerState.Jump:
                return IsOneOf(to,
                    PlayerState.Airborne, PlayerState.DoubleJump,
                    PlayerState.Dash, PlayerState.WallGrab, PlayerState.Hook,
                    PlayerState.Attack);

            case PlayerState.Airborne:
                return IsOneOf(to,
                    PlayerState.Idle, PlayerState.Walk, PlayerState.Run,
                    PlayerState.DoubleJump, PlayerState.Dash,
                    PlayerState.WallGrab, PlayerState.WallSlide,
                    PlayerState.Hook, PlayerState.Attack);

            case PlayerState.DoubleJump:
                return IsOneOf(to,
                    PlayerState.Airborne, PlayerState.Dash,
                    PlayerState.WallGrab, PlayerState.Hook,
                    PlayerState.Attack);

            case PlayerState.Dash:
                return IsOneOf(to,
                    PlayerState.Idle, PlayerState.Walk, PlayerState.Run,
                    PlayerState.Airborne, PlayerState.WallGrab,
                    PlayerState.Hook);

            case PlayerState.WallGrab:
                return IsOneOf(to,
                    PlayerState.WallSlide, PlayerState.WallJump,
                    PlayerState.Airborne, PlayerState.Hook, PlayerState.Dash);

            case PlayerState.WallSlide:
                return IsOneOf(to,
                    PlayerState.WallJump, PlayerState.WallGrab,
                    PlayerState.Airborne, PlayerState.Hook, PlayerState.Dash);

            case PlayerState.WallJump:
                return IsOneOf(to,
                    PlayerState.Airborne, PlayerState.Dash,
                    PlayerState.DoubleJump, PlayerState.WallGrab,
                    PlayerState.Hook, PlayerState.Attack);

            case PlayerState.Hook:
                return IsOneOf(to,
                    PlayerState.Idle, PlayerState.Walk, PlayerState.Run,
                    PlayerState.Airborne, PlayerState.WallGrab,
                    PlayerState.Dash, PlayerState.Attack);

            case PlayerState.Crouch:
                return IsOneOf(to,
                    PlayerState.Idle, PlayerState.Walk, PlayerState.Attack,
                    PlayerState.Hook, PlayerState.Dash);

            case PlayerState.Attack:
                return IsOneOf(to,
                    PlayerState.Idle, PlayerState.Walk, PlayerState.Run,
                    PlayerState.Airborne, PlayerState.Crouch,
                    PlayerState.Defend, PlayerState.Parry,
                    PlayerState.Hook, PlayerState.Dash);

            case PlayerState.Defend:
                return IsOneOf(to,
                    PlayerState.Idle, PlayerState.Walk, PlayerState.Parry);

            case PlayerState.Parry:
                return IsOneOf(to,
                    PlayerState.Defend, PlayerState.Idle,
                    PlayerState.Walk, PlayerState.Attack);

            case PlayerState.Hurt:
                return IsOneOf(to,
                    PlayerState.Idle, PlayerState.Walk, PlayerState.Run,
                    PlayerState.Airborne, PlayerState.WallGrab,
                    PlayerState.WallSlide, PlayerState.Dead);

            case PlayerState.Dead:
                //Se permite volver a idle despues de reaparecer en un checkpoint
                return to == PlayerState.Idle;

            default:
                return false;
        }
    }

    //Metodo que comprueba si un estado esta en la lista de estados permitidos
    private bool IsOneOf(PlayerState value, params PlayerState[] options)
    {
        foreach (PlayerState option in options) //Devuelve true siempre que los estados que se escojan esten dentro de la lista
        {
            if (value == option)
                return true;
        }

        return false; //Sino devuelve false
    }

    private void EnterState(PlayerState state)
    {
        //Preparar el estado al entrar: por ejemplo, iniciar un temporizador o activar una animación.
        switch (state)
        {
            case PlayerState.Dash:

                dashTimer = dashDuration;

                dashDirection = movementDirection;

                if (Mathf.Abs(dashDirection) < 0.01f)
                    dashDirection = facingDirection;

                rb.gravityScale = 0f;

                break;
        }
    }

    private void ExitState(PlayerState state)
    {
        // Limpiar el estado al salir: por ejemplo, cancelar un temporizador o detener una acción.
        
        switch (state)
        {
            case PlayerState.Dash:

                rb.gravityScale = originalGravity;

                break;
        }
    }

    //METER TODA LA LOGIDA DENTRO DE LOS METODOS DE ABAJO

    #region Estados
    private void Dead()
    {
        throw new NotImplementedException();
    }

    private void Hurt()
    {
        throw new NotImplementedException();
    }

    private void Parry()
    {
        throw new NotImplementedException();
    }

    private void Defend()
    {
        throw new NotImplementedException();
    }

    private void Attack()
    {
        throw new NotImplementedException();
    }

    private void Crouch()
    {
        throw new NotImplementedException();
    }

    private void Hook()
    {
        throw new NotImplementedException();
    }

    private void WallJump()
    {
        throw new NotImplementedException();
    }

    private void WallSlide()
    {
        throw new NotImplementedException();
    }

    private void WallGrab()
    {
        throw new NotImplementedException();
    }

    private void Dash()
    {
        

        dashTimer -= Time.fixedDeltaTime;

        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        if (dashTimer <= 0f)
        {
            if (isGrounded)
            {
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
                else
                {
                    ChangeState(PlayerState.Idle);
                    return;
                }
            }
            else
            {
                ChangeState(PlayerState.Airborne);
                return;
            }
        }
    }

    private void DoubleJump()
    {
        throw new NotImplementedException();
    }

    private void Airborne()
    {
        rb.linearVelocity = new Vector2(movementDirection * airborneSpeed, rb.linearVelocity.y);

        if (isGrounded && rb.linearVelocity.y <= 0)
        {
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
            else
            {
                ChangeState(PlayerState.Idle);
                return;
            }
        }

        if (dashPressed && canDash && dashCooldownTimer <= 0)
        {
            bool canUseDash = skillManager.IsSkillUnlocked("DASH");

            if (!canUseDash)
            {
                Debug.Log("DASH no está desbloqueado: el jugador no puede utilizarlo.");
                return;
            }

            canDash = false;
            dashPressed = false;

            dashCooldownTimer = dashCooldown;

            ChangeState(PlayerState.Dash);
            return;
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        ChangeState(PlayerState.Airborne);
        return;
    }

    private void Run()
    {
        rb.linearVelocity = new Vector2(movementDirection * runSpeed, rb.linearVelocity.y);

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

        if (jumpPressed && isGrounded)
        {
            jumpPressed = false;
            ChangeState(PlayerState.Jump);
            return;
        }

        if (dashPressed && canDash && dashCooldownTimer <= 0)
        {
            bool canUseDash = skillManager.IsSkillUnlocked("DASH");

            if (!canUseDash)
            {
                Debug.Log("DASH no está desbloqueado: el jugador no puede utilizarlo.");
                return;
            }

            canDash = false;
            dashPressed = false;

            dashCooldownTimer = dashCooldown;

            ChangeState(PlayerState.Dash);
            return;
        }
    }

    private void Walk()
    {
        rb.linearVelocity = new Vector2(movementDirection * walkSpeed, rb.linearVelocity.y);

        if (runPressed)
        {
            ChangeState(PlayerState.Run);
            return;
        }

        if (Mathf.Abs(movementDirection) < 0.01f)
        {
            ChangeState(PlayerState.Idle);
            return;
        }

        if (jumpPressed && isGrounded)
        {
            jumpPressed = false;
            ChangeState(PlayerState.Jump);
            return;
        }

        if (dashPressed && canDash && dashCooldownTimer <= 0 )
        {
            
            if (!CanUseSkill("DASH")) return;

            canDash = false;
            dashPressed = false;

            dashCooldownTimer = dashCooldown;

            ChangeState(PlayerState.Dash);
            return;
        }
    }
    /// <summary>
    /// Metodo para saber si tienes una skill y asi poder usarla 
    /// JOEL PONLO DONDE TE QUEPA
    /// </summary>
    /// <param name="skill"></param>
    /// <returns></returns>
    private bool CanUseSkill(string skill)
    {
        bool canUseSkill = skillManager.IsSkillUnlocked(skill);
        return canUseSkill;
        
    }

    private void Idle()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

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

        if (jumpPressed && isGrounded)
        {
            jumpPressed = false;
            ChangeState(PlayerState.Jump);
            return;
        }

        if (dashPressed && canDash && dashCooldownTimer <= 0)
        {
            canDash = false;
            dashPressed = false;

            dashCooldownTimer = dashCooldown;

            ChangeState(PlayerState.Dash);
            return;
        }
    }

    #endregion
}
