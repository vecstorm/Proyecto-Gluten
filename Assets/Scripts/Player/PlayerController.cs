using System;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Define todos los estados en los que puede encontrarse el jugador.
/// </summary>
public enum PlayerState { Idle, Walk, Run, Jump, Airborne, DoubleJump, Dash, WallGrab, WallSlide, WallJump, Hook, Attack, Parry, Hurt, Dead }

/// <summary>
/// Gestiona todo el control del jugador mediante una m�quina de estados.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerMovement))]
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

    [Tooltip("Estado actual de la m�quina de estados del jugador")]
    [SerializeField] private PlayerState currentState = PlayerState.Idle;

    /// <summary>
    /// Devuelve el estado actual del jugador en modo lectura.
    /// </summary>
    public PlayerState CurrentState => currentState;

    /// <summary>
    /// Se utiliza para guardar el estado previo del jugador.
    /// </summary>
    private PlayerState previousState;

    /// <summary>
    /// Se invoca cuando el jugador cambia de estado, incluye el estado anterior y el nuevo.
    /// </summary>
    public event Action<PlayerState, PlayerState> OnStateChanged;

    #endregion

    #region Components

    [Header("References")]

    [Tooltip("Gestor encargado de comprobar qu� habilidades tiene desbloqueadas el jugador")]
    [SerializeField] private SkillUnlockManager skillManager;

    /// <summary>
    /// Referencia al sistema de inputs del jugador.
    /// </summary>
    private PlayerInputs playerInputs;

    /// <summary>
    /// Referencia al sistema de movimiento del jugador.
    /// </summary>
    private PlayerMovement playerMovement;

    /// <summary>
    /// Referencia al sistema de gancho del jugador.
    /// </summary>
    private PlayerHook playerHook;

    /// <summary>
    /// Referencia al sistema de combate del jugador.
    /// </summary>
    private PlayerCombat playerCombat;

    /// <summary>
    /// Referencia al sistema de estad�sticas del jugador.
    /// </summary>
    private CharacterStats characterStats;

    private Rigidbody2D rb;
    private Vector3 initialSpawnPosition;
    private FMOD.Studio.EventInstance wallSlideInstance;

    #endregion

    #region Particles

    [Header("Particles")]
    [SerializeField] ParticleSystem walkingDust;
    [SerializeField] ParticleSystem wallSlideDust;
    [SerializeField] Transform parryPoint;

    #endregion

    #region Hurt

    [Header("Damage")]

    [Tooltip("Tiempo que dura el estado de da�o recibido")]
    [SerializeField] float hurtTimerDuration = 1f;

    /// <summary>
    /// Tiempo que queda de estado de da�o recibido.
    /// </summary>
    private float hurtTimer;

    #endregion

    #region Hook

    [Tooltip("Duraci�n de la inercia tras engancharse")]
    [SerializeField] private float hookMomentumDuration = 0.1f;

    /// <summary>
    /// Tiempo restante de duraci�n de la inercia tras engancharse actual.
    /// </summary>
    private float hookMomentumTimer;

    #endregion

    #region Orientation and Sprite

    /// <summary>
    /// Direcci�n hacia la que mira el personaje. 1 = derecha, -1 = izquierda. 
    /// </summary>
    private int facingDirection = 1;

    [Header("Sprite")]

    [Tooltip("Transform del sprite del personaje")]
    [SerializeField] private Transform spriteTransform;

    [Header("Parry Visual")]

    [Tooltip("Color del personaje mientras realiza un parry")]
    [SerializeField] private Color parryColor = Color.cyan;

    /// <summary>
    /// Renderer del sprite del personaje.
    /// </summary>
    private SpriteRenderer[] spriteRenderers;

    /// <summary>
    /// Color original del sprite del personaje.
    /// </summary>
    private Color originalSpriteColor;

    [Tooltip("Prefab que aparece cuando se realiza un parry exitoso")]
    [SerializeField] private GameObject parryHitPrefab;

    [Tooltip("Posici�n del indicador respecto al jugador")]
    [SerializeField] private Vector3 parryHitOffset = new Vector3(0.5f, 1f, 0f);

    [Tooltip("Tiempo que permanece visible el indicador")]
    [SerializeField, Min(0f)] private float parryHitDuration = 0.5f;

    /// <summary>
    /// Instancia de la representaci�n visual del parry exitoso.
    /// </summary>
    private GameObject parryHitInstance;

    /// <summary>
    /// Temporizador de la representaci�n visual del parry exitoso.
    /// </summary>
    private float parryHitTimer;

    [Header("Damage Visual")]

    [Tooltip("Color que adquiere el personaje al recibir da�o")]
    [SerializeField] private Color damageFlashColor = Color.red;

    [Tooltip("Intervalo entre cambios de color")]
    [SerializeField, Min(0.01f)] private float damageFlashInterval = 0.05f;

    /// <summary>
    /// Tiempo restante del parpadeo.
    /// </summary>
    private float damageFlashTimer;

    /// <summary>
    /// Tiempo restante hasta el siguiente cambio de color.
    /// </summary>
    private float damageFlashIntervalTimer;

    /// <summary>
    /// Indica si el personaje debe mostrar el color de da�o.
    /// </summary>
    private bool damageFlashActive;

    #endregion

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initialSpawnPosition = transform.position;

        characterStats = GetComponent<CharacterStats>();

        characterStats.OnDeath += HandleDeath;

        if (playerInputs == null)
            playerInputs = GetComponent<PlayerInputs>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (playerHook == null)
            playerHook = GetComponent<PlayerHook>();

        if (playerCombat == null)
            playerCombat = GetComponent<PlayerCombat>();

        if (spriteTransform != null)
        {
            spriteRenderers = spriteTransform.GetComponentsInChildren<SpriteRenderer>(true);

            if (spriteRenderers.Length > 0)
                originalSpriteColor = spriteRenderers[0].color;
        }

        if (parryHitPrefab != null)
        {
            parryHitInstance = Instantiate(parryHitPrefab, transform);

            parryHitInstance.transform.localPosition = parryHitOffset;
            parryHitInstance.SetActive(false);
        }

        characterStats.OnParrySuccess += ShowParryHit;
    }

    private void Start()
    {
        EnterState(currentState);
    }

    private void Update()
    {
        playerInputs.ReadInputs();

        UpdateFacingDirection();

        UpdateParryHit();
        UpdateDamageFlash();
    }

    private void FixedUpdate()
    {
        if (currentState == PlayerState.Dead)
        {
            playerMovement.StopMovement();
            return;
        }

        if (hurtTimer > 0f)
            hurtTimer -= Time.fixedDeltaTime;

        playerInputs.UpdateInputBuffers();

        playerMovement.CheckGround(currentState == PlayerState.Idle || currentState == PlayerState.Walk || currentState == PlayerState.Run);
        playerMovement.CheckWall(facingDirection);

        playerMovement.UpdateCooldowns();
        
        // Procesamos el gancho solo si el jugador lo tiene desbloqueado
        if (CanUseSkill("GANCHO"))
            playerHook.TickFixed();

        if (playerHook.WasPullingThisStep && !playerHook.IsPulling)
        {
            hookMomentumTimer = hookMomentumDuration;
        }

        // Mientras el gancho est� activo, bloqueamos el movimiento normal
        if (playerHook.IsBusy || hookMomentumTimer > 0f)
        {
            hookMomentumTimer = Mathf.Max(0f, hookMomentumTimer - Time.fixedDeltaTime);

            // Si est� lanz�ndose o recogi�ndose, detenemos el movimiento horizontal, durante Pulling dejamos que PlayerHook controle la velocidad
            if (!playerHook.IsPulling)
                playerMovement.StopHorizontalMovement();

            // Permitimos cancelar el gancho mediante salto o dash
            CheckHookAbilities();
        }
        else
        {
            if (!TryCombatActions())
                UpdateState();
        }

        playerInputs.ConsumeJumpRelease();
    }

    private void OnDestroy()
    {
        if (characterStats != null)
        {
            characterStats.OnDeath -= HandleDeath;
            characterStats.OnParrySuccess -= ShowParryHit;
        }
    }

    #endregion

    #region Direction

    /// <summary>
    /// Actualiza la direcci�n hacia la que mira el personaje.
    /// </summary>
    private void UpdateFacingDirection()
    {
        if (currentState == PlayerState.Dead)
            return;

        // Mientras el gancho est� activo, mantenemos la orientaci�n.
        if (!playerHook.IsBusy)
        {
            if (playerInputs.MovementDirection > 0)
                facingDirection = 1;
            else if (playerInputs.MovementDirection < 0)
                facingDirection = -1;
        }

        UpdateSpriteDirection();

        playerCombat?.SetFacingDirection(facingDirection);
        playerMovement?.SetFacingDirection(facingDirection);
        characterStats?.SetFacingDirection(facingDirection);
    }

    private void UpdateSpriteDirection()
    {
        Vector3 scale = spriteTransform.localScale;

        scale.x = Mathf.Abs(scale.x) * facingDirection;

        spriteTransform.localScale = scale;

        walkingDust.gameObject.transform.localScale = scale;

        Vector3 wallDustPos = wallSlideDust.transform.localPosition;

        wallSlideDust.gameObject.transform.localPosition = wallDustPos;

        Vector3 parryPos = parryPoint.localPosition;
        parryPos.x = Mathf.Abs(parryPos.x) * -facingDirection;

        parryPoint.localPosition = parryPos;
    }

    #endregion

    #region State Machine

    /// <summary>
    /// Intenta realizar una transici�n al estado indicado si est� permitida.
    /// </summary>
    /// <param name="newState">Estado al que se desea cambiar</param>
    /// <returns>
    /// True si la transici�n se ha realizado correctamente.
    /// </returns>
    public bool ChangeState(PlayerState newState)
    {
        //Si el nuevo estado es el mismo al estado actual, no hace nada
        if (newState == currentState)
            return false;

        //Si la transici�n no est� permitida, avisamos
        if (!IsTransitionAllowed(currentState, newState))
        {
            Debug.LogWarning($"Transici�n no permitida: {currentState} -> {newState}", this);
            return false;
        }

        // Guardamos el estado anterior
        PlayerState oldState = currentState;

        // Actualizamos el estado
        ExitState(currentState);
        previousState = currentState;
        currentState = newState;
        EnterState(currentState);

        // Notificamos el cambio a los sistemas interesados
        OnStateChanged?.Invoke(oldState, currentState);

        return true;
    }

    /// <summary>
    /// Comprueba si una transici�n entre dos estados es v�lida seg�n
    /// las reglas definidas por la m�quina de estados.
    /// </summary>
    /// <param name="from">Estado de origen</param>
    /// <param name="to">Estado de destino</param>
    /// <returns>
    /// True si la transici�n est� permitida.
    /// </returns> 
    public bool IsTransitionAllowed(PlayerState from, PlayerState to)
    {
        // No permitimos cambios entre el mismo estado
        if (from == to) 
            return false;

        // Desde cualquier estado excepto Hurt y Dead, el jugador puede pasar a Hurt o Dead
        // Ejemplo: no puede pasar de dead a hurt porque en esta condici�n no est� permitido
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
                return IsOneOf(to, PlayerState.Idle, PlayerState.Walk, PlayerState.Run, PlayerState.Jump, PlayerState.Airborne, PlayerState.WallGrab, PlayerState.Dash, PlayerState.DoubleJump, PlayerState.Attack, PlayerState.Parry);

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
    /// <param name="options">Lista de estados v�lidos</param>
    /// <returns>
    /// True si el estado se encuentra dentro de la lista.
    /// </returns>
    private bool IsOneOf(PlayerState value, params PlayerState[] options)
    {
        // Devuelve true siempre que los estados que se escojan est�n dentro de la lista
        foreach (PlayerState option in options)
        {
            if (value == option)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Ejecuta la l�gica necesaria al entrar en un nuevo estado.
    /// </summary>
    /// <param name="state">Estado al que se accede</param>
    private void EnterState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
                break;

            case PlayerState.Walk:
                walkingDust.gameObject.SetActive(true);
                break;

            // Aplicamos velocidad en el aire seg�n de que estado vengamos
            case PlayerState.Jump:

                playerMovement.SetAirborneSpeed(previousState == PlayerState.Run ? playerMovement.RunSpeed : playerMovement.WalkSpeed);
                AudioController.Instance.PlayPlayerMovement(PlayerMovementSFX.Jump);
                break;

            case PlayerState.DoubleJump:

                AudioController.Instance.PlayPlayerMovement(PlayerMovementSFX.DoubleJump);
                break;

            case PlayerState.Airborne:

                // Si hemos ca�do directamente desde Walk o Run, conservamos la velocidad del estado anterior
                if (previousState == PlayerState.Walk || previousState == PlayerState.Run)
                {
                    playerMovement.SetAirborneSpeed(previousState == PlayerState.Run ? playerMovement.RunSpeed : playerMovement.WalkSpeed);
                }
                break;

            // Definimos timer, direcci�n y desactivamos la gravedad
            case PlayerState.Dash:

                AudioController.Instance.PlayPlayerMovement(PlayerMovementSFX.Dash);

                playerMovement.EnterDash(IsWallState(previousState), facingDirection, playerInputs.MovementDirection);
                break;

            // Definimos timer y desactivamos la gravedad
            case PlayerState.WallGrab:

                AudioController.Instance.PlayPlayerMovement(PlayerMovementSFX.WallGrab);

                playerMovement.EnterWallGrab();
                break;

            // Definimos velocidad y reactivamos la gravedad
            case PlayerState.WallSlide:

                wallSlideInstance = AudioController.Instance.StartPlayerMovementLoop(PlayerMovementSFX.WallSlide);

                playerMovement.EnterWallSlide();
                wallSlideDust.gameObject.SetActive(true);
                break;

            case PlayerState.WallJump:

                AudioController.Instance.PlayPlayerMovement(PlayerMovementSFX.WallJump);
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

                    // Si no hay direcci�n, utilizamos la orientaci�n del jugador
                    if (direction.sqrMagnitude < 0.01f)
                        direction = Vector2.right * facingDirection;
                }

                if (playerHook.FireHook(direction))
                {
                    // Actualizamos la orientaci�n seg�n la direcci�n real del gancho
                    float hookDirectionX = playerHook.HookDirection.x;

                    if (Mathf.Abs(hookDirectionX) > 0.01f)
                    {
                        facingDirection = hookDirectionX > 0f ? 1 : -1;

                        UpdateSpriteDirection();

                        playerCombat?.SetFacingDirection(facingDirection);
                        characterStats?.SetFacingDirection(facingDirection);
                    }
                }

                break;

            case PlayerState.Attack:

                playerCombat?.PerformBasicAttack();
                break;

            case PlayerState.Parry:

                ParticleManager.SpawnParticles("PlayerParry", parryPoint.position, Quaternion.identity);
                characterStats.BeginParry();
                AudioController.Instance.PlayPlayerCombat(PlayerCombatSFX.ParryStart);
                SetSpriteColor(parryColor);

                break;

            case PlayerState.Hurt:

                playerMovement.StopMovement();
                hurtTimer = hurtTimerDuration;
                StartDamageFlash();
                AudioController.Instance.PlayPlayerStatus(PlayerStatusSFX.Hurt);

                break;

            case PlayerState.Dead:

                playerMovement.StopMovement();
                playerMovement.RestoreGravity();

                playerHook.CancelHook();
                characterStats.SetCanTakeDamage(false);

                break;
        }
    }

    /// <summary>
    /// Ejecuta la l�gica necesaria al abandonar un estado.
    /// </summary>
    /// <param name="state">Estado que se abandona</param>
    private void ExitState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Walk:
                walkingDust.gameObject.SetActive(false);
                break;

            // Reiniciamos la gravedad en los estados necesarios
            case PlayerState.Dash:

                playerMovement.RestoreGravity();
                break;

            case PlayerState.WallSlide:

                if (!wallSlideInstance.isValid())
                    return;

                AudioController.Instance.StopSFX(ref wallSlideInstance);
                wallSlideDust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                break;

            case PlayerState.WallGrab:

                playerMovement.RestoreGravity();
                break;

            case PlayerState.Parry:

                characterStats.EndParry();
                RestoreSpriteColor();

                break;
        }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Comprueba si una habilidad est� desbloqueada.
    /// </summary>
    /// <param name="skill">Nombre de la habilidad</param>
    /// <returns>True si la habilidad est� desbloqueada.</returns>
    private bool CanUseSkill(string skill)
    {
        return skillManager.IsSkillUnlocked(skill);
    }

    /// <summary>
    /// Ejecuta la l�gica correspondiente al estado actual.
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
    /// Cambia al estado Idle, Walk o Run dependiendo de la direcci�n de movimiento y del estado de carrera.
    /// </summary>
    private void ChangeToGroundState()
    {
        AudioController.Instance.PlayPlayerMovement(PlayerMovementSFX.Land);

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
    /// Intenta iniciar un salto si el jugador est� en el suelo y se ha detectado una entrada de salto.
    /// </summary>
    /// <returns>
    /// True si el salto se ha iniciado correctamente.
    /// </returns>
    private bool TryJump()
    {
        if (!playerInputs.JumpPressed || (!playerMovement.IsGrounded && !playerMovement.HasCoyoteTime))
            return false;

        if (!ChangeState(PlayerState.Jump))
            return false;

        // Si hemos saltado desde el aire, consumimos el Coyote Time
        if (!playerMovement.IsGrounded)
            playerMovement.ConsumeCoyoteTime();

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
        if (!playerInputs.DashPressed || !playerMovement.CanDash)
            return false;

        if (!CanUseSkill("DASH"))
            return false;
        
        if (!ChangeState(PlayerState.Dash))
            return false;

        playerMovement.ConsumeDash();

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
        // Salto normal si estamos en el suelo
        if (playerInputs.JumpPressed && playerMovement.IsGrounded)
        {
            playerHook.CancelHook();

            if (ChangeState(PlayerState.Jump))
            {
                playerInputs.ConsumeJumpInput();
                return;
            }
        }

        // Doble salto
        if (playerInputs.JumpPressed && playerMovement.CanDoubleJump && CanUseSkill("DOUBLE_JUMP"))
        {
            playerHook.CancelHook();

            if (ChangeState(PlayerState.DoubleJump))
            {
                playerMovement.ConsumeDoubleJump();
                playerInputs.ConsumeJumpInput();
                return;
            }
        }

        // Dash
        if (playerInputs.DashPressed && playerMovement.CanDash && CanUseSkill("DASH"))
        {
            playerHook.CancelHook();

            TryDash();
        }
    }

    /// <summary>
    /// Comprueba si se puede ejecutar alguna acci�n de combate, el parry tiene prioridad sobre el ataque.
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

        // Ataque b�sico
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

    /// <summary>
    /// Aplica un color a todos los sprites del personaje.
    /// </summary>
    private void SetSpriteColor(Color color)
    {
        if (spriteRenderers == null)
            return;

        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            if (renderer != null)
                renderer.color = color;
        }
    }

    /// <summary>
    /// Restaura el color original de todos los sprites del personaje.
    /// </summary>
    private void RestoreSpriteColor()
    {
        if (spriteRenderers == null)
            return;

        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            if (renderer != null)
                renderer.color = originalSpriteColor;
        }
    }

    /// <summary>
    /// Muestra el indicador visual al realizar un parry exitoso.
    /// </summary>
    private void ShowParryHit()
    {
        if (parryHitInstance == null)
            return;

        parryHitTimer = parryHitDuration;
        parryHitInstance.SetActive(true);
    }

    /// <summary>
    /// Controla cu�nto tiempo permanece visible el indicador.
    /// </summary>
    private void UpdateParryHit()
    {
        if (parryHitTimer <= 0f)
            return;

        parryHitTimer -= Time.deltaTime;

        if (parryHitTimer <= 0f)
        {
            parryHitTimer = 0f;
            parryHitInstance.SetActive(false);
        }
    }

    /// <summary>
    /// Inicia el parpadeo visual cuando el jugador recibe da�o.
    /// </summary>
    private void StartDamageFlash()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
            return;

        damageFlashTimer = hurtTimerDuration;
        damageFlashIntervalTimer = 0f;

        // Comenzamos mostrando el color de da�o.
        damageFlashActive = true;
        SetSpriteColor(damageFlashColor);
    }

    /// <summary>
    /// Actualiza el parpadeo visual mientras permanece activo.
    /// </summary>
    private void UpdateDamageFlash()
    {
        if (damageFlashTimer <= 0f)
            return;

        damageFlashTimer -= Time.deltaTime;
        damageFlashIntervalTimer -= Time.deltaTime;

        // Cambiamos entre el color original y el color de da�o.
        if (damageFlashIntervalTimer <= 0f)
        {
            damageFlashActive = !damageFlashActive;

            SetSpriteColor(damageFlashActive ? damageFlashColor : originalSpriteColor);

            damageFlashIntervalTimer = damageFlashInterval;
        }

        // Al finalizar, restauramos el color original.
        if (damageFlashTimer <= 0f)
        {
            damageFlashTimer = 0f;
            damageFlashActive = false;

            RestoreSpriteColor();
        }
    }

    private void HandleDeath()
    {
        if (currentState == PlayerState.Dead)
            return;

        ChangeState(PlayerState.Dead);
    }

    /// <summary>
    /// Reaparece al jugador cuando termina la animaci�n de muerte.
    /// </summary>
    public void RespawnPlayer()
    {
        if (currentState != PlayerState.Dead)
            return;

        AudioController.Instance.PlayPlayerStatus(PlayerStatusSFX.Respawn);

        // Determinamos la posici�n de reaparici�n
        Vector2 respawnPosition = CheckpointZone2D.hasSafePoint
            ? CheckpointZone2D.lastSafePosition
            : (Vector2)initialSpawnPosition;

        // Detenemos completamente la f�sica
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Restauramos la gravedad por seguridad
        playerMovement.RestoreGravity();

        // Cancelamos cualquier actividad del gancho
        playerHook.CancelHook();

        // Recolocamos al jugador
        transform.position = new Vector3(
            respawnPosition.x,
            respawnPosition.y,
            initialSpawnPosition.z
        );

        // Recuperamos vida y estamina
        characterStats.FullHeal();

        // Permitimos volver a recibir da�o
        characterStats.SetCanTakeDamage(true);

        // Restablecemos el temporizador de da�o
        hurtTimer = 0f;

        // Restablecemos los enemigos
        EnemyManager.ResetEnemies();

        // Volvemos al estado inicial
        ChangeState(PlayerState.Idle);
    }

    #endregion

    #region Character States

    #region Ground States

    /// <summary>
    /// Estado de reposo del jugador.
    /// </summary>
    private void Idle()
    {
        playerMovement.ApplyIdleMovement();

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
    /// Estado de desplazamiento b�sico.
    /// </summary>
    private void Walk()
    {
        playerMovement.ApplyWalkMovement(playerInputs.MovementDirection);

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

        if (!playerMovement.IsGrounded)
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
    /// Estado de movimiento r�pido en suelo.
    /// </summary>
    private void Run()
    {
        playerMovement.ApplyRunMovement(playerInputs.MovementDirection);

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

        if (!playerMovement.IsGrounded)
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
        playerMovement.ApplyJump();
        ChangeState(PlayerState.Airborne);
    }

    /// <summary>
    /// Estado utilizado mientras el jugador permanece en el aire.
    /// </summary>
    private void Airborne()
    {
        playerMovement.ApplyAirborneMovement(playerInputs.MovementDirection);

        // Si se ha soltado el salto, bajamos antes
        if (playerInputs.JumpReleased)
        {
            playerMovement.ApplyJumpCut();
            playerInputs.ConsumeJumpRelease();
        }

        // Intentamos realizar un salto normal mediante Coyote Time
        if (!playerMovement.IsGrounded && TryJump())
            return;

        // Si se ha presionado el salto en el aire, se puede realizar un doble salto y tenemos la habilidad, se ejecuta
        if (playerInputs.JumpPressed && playerMovement.CanDoubleJump && CanUseSkill("DOUBLE_JUMP"))
        {
            if (ChangeState(PlayerState.DoubleJump))
            {
                playerMovement.ConsumeDoubleJump();
                playerInputs.ConsumeJumpInput();
                ParticleManager.SpawnParticles("DoubleJumpSmoke", transform.position, Quaternion.identity);
                return;
            }
        }

        // Si estamos tocando la pared, no estamos en el suelo y tenemos la habilidad, hacemos wall grab
        if (playerMovement.IsTouchingWall && !playerMovement.IsGrounded && playerMovement.Velocity.y <= 0 && CanUseSkill("WALL_SKILL"))
        {
            ChangeState(PlayerState.WallGrab);
            return;
        }

        // Si dejamos de caer, pasamos a un estado terrestre
        if (playerMovement.IsGrounded && playerMovement.Velocity.y <= 0)
        {
            ChangeToGroundState();
            ParticleManager.SpawnParticlesOnSurface("LandingSmoke", transform.position, Vector3.down, playerMovement.GroundLayer);
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
        playerMovement.ApplyDoubleJump();
        ChangeState(PlayerState.Airborne);
    }

    /// <summary>
    /// Estado en el que el jugador se agarra a una pared.
    /// </summary>
    private void WallGrab()
    {
        playerMovement.RefreshAirAbilities();
        playerMovement.ApplyWallGrab();

        if (playerInputs.JumpPressed && ChangeState(PlayerState.WallJump))
        {
            playerInputs.ConsumeJumpInput();
            return;
        }

        if (!playerMovement.IsTouchingWall)
        {
            ChangeState(PlayerState.Airborne);
            return;
        }

        if (playerMovement.IsGrounded)
        {
            ChangeToGroundState();
            return;
        }

        if (playerMovement.WallGrabFinished)
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
        playerMovement.ApplyWallSlide();

        if (playerInputs.JumpPressed && ChangeState(PlayerState.WallJump))
        {
            playerInputs.ConsumeJumpInput();
            return;
        }

        if (!playerMovement.IsTouchingWall)
        {
            ChangeState(PlayerState.Airborne);
            return;
        }

        if (playerMovement.IsGrounded)
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
        playerMovement.ApplyWallJump(facingDirection);
        ChangeState(PlayerState.Airborne);
    }

    #endregion

    #region Special States

    /// <summary>
    /// Estado de movimiento r�pido horizontal que ignora la gravedad temporalmente.
    /// </summary>
    private void Dash()
    {
        if (playerMovement.ApplyDashMovement())
        {
            if (playerMovement.IsGrounded)
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
        if (!playerMovement.IsGrounded)
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
        if (playerMovement.IsGrounded)
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
        playerMovement.ApplyIdleMovement();

        // Cuando termina la ventana de parry, abandonamos el estado.
        if (playerCombat == null || !characterStats.IsParrying)
        {
            if (playerMovement.IsGrounded)
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
    /// Estado de da�o recibido.
    /// </summary>
    private void Hurt()
    {
        if (hurtTimer <= 0)
        {
            if (playerMovement.IsGrounded)
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
        playerMovement.StopMovement();
    }

    #endregion

    #endregion
}