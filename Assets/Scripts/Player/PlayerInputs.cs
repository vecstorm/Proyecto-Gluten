using UnityEngine;

/// <summary>
/// Gestiona la lectura de los controles del jugador y el Input Buffering.
/// </summary>
public class PlayerInputs : MonoBehaviour
{
    #region Variables

    #region Input System

    /// <summary>
    /// Referencia al sistema de inputs del jugador.
    /// </summary>
    private GlutenActionControls glutenActionControls;

    #endregion

    #region Input Values

    /// <summary>
    /// Dirección horizontal introducida por el jugador. Valores entre -1 y 1.
    /// </summary>
    public float MovementDirection { get; private set; }

    /// <summary>
    /// Vector de movimiento del jugador.
    /// </summary>
    public Vector2 MoveInput { get; private set; }

    /// <summary>
    /// Indica si el jugador mantiene pulsado el botón de correr.
    /// </summary>
    public bool RunPressed { get; private set; }

    /// <summary>
    /// Indica si se ha pulsado el salto durante este frame.
    /// </summary>
    private bool jumpPressed;

    /// <summary>
    /// Indica si se ha soltado el salto durante este frame.
    /// </summary>
    private bool jumpReleased;

    /// <summary>
    /// Indica si se ha solicitado realizar un dash.
    /// </summary>
    private bool dashPressed;

    /// <summary>
    /// Indica si se ha solicitado usar el gancho.
    /// </summary>
    private bool hookPressed;

    /// <summary>
    /// Indica si se ha solicitado realizar un ataque.
    /// </summary>
    private bool attackPressed;

    /// <summary>
    /// Indica si se ha solicitado realizar un ataque a distancia.
    /// </summary>
    private bool rangedAttackPressed;

    /// <summary>
    /// Indica si se ha solicitado realizar un parry.
    /// </summary>
    private bool parryPressed;

    /// <summary>
    /// [Getter] Indica si se ha pulsado el salto durante este frame.
    /// </summary>
    public bool JumpPressed => jumpPressed;

    /// <summary>
    /// [Getter] Indica si se ha soltado el salto durante este frame.
    /// </summary>
    public bool JumpReleased => jumpReleased;

    /// <summary>
    /// [Getter] Indica si se ha solicitado realizar un dash.
    /// </summary>
    public bool DashPressed => dashPressed;

    /// <summary>
    /// [Getter] Indica si se ha solicitado usar el gancho.
    /// </summary>
    public bool HookPressed => hookPressed;

    /// <summary>
    /// [Getter] Indica si se ha solicitado realizar un ataque.
    /// </summary>
    public bool AttackPressed => attackPressed;

    /// <summary>
    /// [Getter] Indica si se ha solicitado realizar un ataque a distancia.
    /// </summary>
    public bool RangedAttackPressed => rangedAttackPressed;

    /// <summary>
    /// [Getter] Indica si se ha solicitado realizar un parry.
    /// </summary>
    public bool ParryPressed => parryPressed;

    #endregion

    #region Input Buffering

    [Header("Input Buffering")]

    [Tooltip("Tiempo durante el que se guarda una pulsación de salto")]
    [SerializeField, Min(0f)] private float jumpBufferDuration = 0.12f;

    [Tooltip("Tiempo durante el que se guarda una pulsación de dash")]
    [SerializeField, Min(0f)] private float dashBufferDuration = 0.12f;

    [Tooltip("Tiempo durante el que se guarda una pulsación de gancho")]
    [SerializeField, Min(0f)] private float hookBufferDuration = 0.12f;

    [Tooltip("Tiempo durante el que se guarda una pulsación de ataque básico")]
    [SerializeField, Min(0f)] private float attackBufferDuration = 0.12f;

    [Tooltip("Tiempo durante el que se guarda una pulsación de ataque a distancia")]
    [SerializeField, Min(0f)] private float rangedAttackBufferDuration = 0.12f;

    [Tooltip("Tiempo durante el que se guarda una pulsación de parry")]
    [SerializeField, Min(0f)] private float parryBufferDuration = 0.10f;

    /// <summary>
    /// Tiempo que queda de buffer de salto.
    /// </summary>
    private float jumpBufferTimer;

    /// <summary>
    /// Tiempo que queda de buffer de dash.
    /// </summary>
    private float dashBufferTimer;

    /// <summary>
    /// Tiempo que queda de buffer de gancho.
    /// </summary>
    private float hookBufferTimer;

    /// <summary>
    /// Tiempo que queda de buffer de ataque.
    /// </summary>
    private float attackBufferTimer;

    /// <summary>
    /// Tiempo que queda de buffer de ataque a distancia.
    /// </summary>
    private float rangedAttackBufferTimer;

    /// <summary>
    /// Tiempo que queda de buffer de parry.
    /// </summary>
    private float parryBufferTimer;

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

    private void OnDisable()
    {
        glutenActionControls.Disable();
    }

    #endregion

    #region Input Reading

    /// <summary>
    /// Lee todos los inputs del jugador, lo invoca PlayerController desde su Update.
    /// </summary>
    public void ReadInputs()
    {
        MoveInput = glutenActionControls.Player.Move.ReadValue<Vector2>();
        MovementDirection = MoveInput.x;

        RunPressed = glutenActionControls.Player.Sprint.IsPressed();

        if (glutenActionControls.Player.Jump.WasPressedThisFrame())
            BufferInput(ref jumpPressed, ref jumpBufferTimer, jumpBufferDuration);

        if (glutenActionControls.Player.Jump.WasReleasedThisFrame())
            jumpReleased = true;

        if (glutenActionControls.Player.Dash.WasPressedThisFrame())
            BufferInput(ref dashPressed, ref dashBufferTimer, dashBufferDuration);

        if (glutenActionControls.Player.Hook.WasPressedThisFrame())
            BufferInput(ref hookPressed, ref hookBufferTimer, hookBufferDuration);

        if (glutenActionControls.Player.Attack.WasPressedThisFrame())
            BufferInput(ref attackPressed, ref attackBufferTimer, attackBufferDuration);

        if (glutenActionControls.Player.RangedAttack.WasPressedThisFrame())
            BufferInput(ref rangedAttackPressed, ref rangedAttackBufferTimer, rangedAttackBufferDuration);

        if (glutenActionControls.Player.Parry.WasPressedThisFrame())
            BufferInput(ref parryPressed, ref parryBufferTimer, parryBufferDuration);
    }

    #endregion

    #region Input Buffering

    /// <summary>
    /// Guarda una pulsación y reinicia su temporizador.
    /// </summary>
    private void BufferInput(ref bool inputPressed, ref float timer, float duration)
    {
        inputPressed = true;
        timer = duration;
    }

    /// <summary>
    /// Reduce el tiempo restante de un buffer, cuando llega a cero, descarta la pulsación.
    /// </summary>
    private void UpdateInputBuffer(ref bool inputPressed, ref float timer)
    {
        if (!inputPressed)
            return;

        timer = Mathf.Max(0f, timer - Time.fixedDeltaTime);

        if (timer <= 0f)
            inputPressed = false;
    }

    /// <summary>
    /// Actualiza todos los buffers del jugador.
    /// </summary>
    public void UpdateInputBuffers()
    {
        UpdateInputBuffer(ref jumpPressed, ref jumpBufferTimer);
        UpdateInputBuffer(ref dashPressed, ref dashBufferTimer);
        UpdateInputBuffer(ref hookPressed, ref hookBufferTimer);
        UpdateInputBuffer(ref attackPressed, ref attackBufferTimer);
        UpdateInputBuffer(ref rangedAttackPressed, ref rangedAttackBufferTimer);
        UpdateInputBuffer(ref parryPressed, ref parryBufferTimer);
    }

    /// <summary>
    /// Consume una pulsación y elimina su buffer.
    /// </summary>
    private void ConsumeInput(ref bool inputPressed, ref float timer)
    {
        inputPressed = false;
        timer = 0f;
    }

    /// <summary>
    /// Consume una pulsación de salto y elimina su buffer.
    /// </summary>
    public void ConsumeJumpInput()
    {
        ConsumeInput(ref jumpPressed, ref jumpBufferTimer);
    }

    /// <summary>
    /// Consume una pulsación de dash y elimina su buffer.
    /// </summary>
    public void ConsumeDashInput()
    {
        ConsumeInput(ref dashPressed, ref dashBufferTimer);
    }

    /// <summary>
    /// Consume una pulsación de gancho y elimina su buffer.
    /// </summary>
    public void ConsumeHookInput()
    {
        ConsumeInput(ref hookPressed, ref hookBufferTimer);
    }

    /// <summary>
    /// Consume una pulsación de ataque y elimina su buffer.
    /// </summary>
    public void ConsumeAttackInput()
    {
        ConsumeInput(ref attackPressed, ref attackBufferTimer);
    }

    /// <summary>
    /// Consume una pulsación ataque a distancia y elimina su buffer.
    /// </summary>
    public void ConsumeRangedAttackInput()
    {
        ConsumeInput(ref rangedAttackPressed, ref rangedAttackBufferTimer);
    }

    /// <summary>
    /// Consume una pulsación de parry y elimina su buffer.
    /// </summary>
    public void ConsumeParryInput()
    {
        ConsumeInput(ref parryPressed, ref parryBufferTimer);
    }

    /// <summary>
    /// Consume el suelte del botón de salto.
    /// </summary>
    public void ConsumeJumpRelease()
    {
        jumpReleased = false;
    }

    #endregion
}