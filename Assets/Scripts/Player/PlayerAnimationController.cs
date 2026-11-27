
using System.Collections;
using UnityEngine;

/// <summary>
/// Gestiona las animaciones del jugador mediante su Animator.
/// Recibe los cambios de estado de PlayerController y actualiza
/// los parámetros correspondientes.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    #region References

    [Header("References")]

    [SerializeField] private Animator animator;

    private PlayerController playerController;
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;
    private PlayerHook playerHook;
    private HookState previousHookState;

    private FMOD.Studio.EventInstance deathSoundInstance;
    private bool deathSoundStarted;

    #endregion

    #region Animator Parameters

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int VerticalSpeedHash =
        Animator.StringToHash("VerticalSpeed");

    private static readonly int GroundedHash =
        Animator.StringToHash("Grounded");

    private static readonly int DoubleJumpHash =
        Animator.StringToHash("DoDoubleJump");

    private static readonly int WallGrabHash =
        Animator.StringToHash("DoWallGrab");

    private static readonly int WallJumpHash =
        Animator.StringToHash("DoWallJump");

    private static readonly int DashHash =
        Animator.StringToHash("DoDash");

    private static readonly int HookHash =
        Animator.StringToHash("DoHook");

    private static readonly int HookEndHash =
        Animator.StringToHash("DoHookEnd");

    private static readonly int AttackHash =
        Animator.StringToHash("DoAttack");

    private static readonly int RangedAttackHash =
        Animator.StringToHash("DoRangedAttack");

    private static readonly int ParryHash =
        Animator.StringToHash("DoParry");

    private static readonly int HurtHash =
        Animator.StringToHash("DoHurt");

    private static readonly int IsDeadHash =
        Animator.StringToHash("IsDead");

    private static readonly int HookAttachHash =
        Animator.StringToHash("DoHookAttach");

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        playerController = GetComponent<PlayerController>();
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();
        playerHook = GetComponent<PlayerHook>();
    }

    private void OnEnable()
    {
        if (playerController != null)
            playerController.OnStateChanged += HandleStateChanged;

        if (playerCombat != null)
            playerCombat.OnRangedAttack += HandleRangedAttack;
    }

    private void OnDisable()
    {
        if (playerController != null)
            playerController.OnStateChanged -= HandleStateChanged;

        if (playerCombat != null)
            playerCombat.OnRangedAttack -= HandleRangedAttack;
    }

    private void Start()
    {
        UpdateMovementParameters();

        // Sincronizamos el Animator con el estado inicial.
        HandleStateChanged(playerController.CurrentState, playerController.CurrentState);

        previousHookState = playerHook.CurrentState;
    }

    private void Update()
    {
        UpdateMovementParameters();

        animator.SetBool("IsWallSliding", playerController.CurrentState == PlayerState.WallSlide);

        UpdateHookAnimation();
    }

    #endregion

    #region Movement Parameters

    /// <summary>
    /// Actualiza los parámetros que describen el movimiento del jugador.
    /// </summary>
    private void UpdateMovementParameters()
    {
        Vector2 velocity = playerMovement.Velocity;

        animator.SetFloat(SpeedHash, Mathf.Abs(velocity.x));
        animator.SetFloat(VerticalSpeedHash, velocity.y);
        animator.SetBool(GroundedHash, playerMovement.IsGrounded);
    }

    /// <summary>
    /// Sincroniza las animaciones con el ciclo de vida del gancho.
    /// </summary>
    private void UpdateHookAnimation()
    {
        HookState currentHookState = playerHook.CurrentState;

        if (currentHookState == previousHookState)
            return;

        switch (currentHookState)
        {
            case HookState.Shooting:
                animator.SetTrigger(HookHash);
                break;

            case HookState.Pulling:
                animator.SetTrigger(HookAttachHash);
                break;

            case HookState.Retracting:
                animator.SetTrigger(HookEndHash);
                break;

            case HookState.Ready:
                // Si termina directamente desde Pulling,
                // también reproducimos la animación de finalización.
                if (previousHookState == HookState.Pulling)
                    animator.SetTrigger(HookEndHash);
                break;
        }

        previousHookState = currentHookState;
    }

    /// <summary>
    /// Reproduce la animación del ataque a distancia.
    /// </summary>
    private void HandleRangedAttack()
    {
        animator.SetTrigger(RangedAttackHash);
    }

    #endregion

    #region State Changes

    /// <summary>
    /// Gestiona los cambios de estado del jugador.
    /// </summary>
    private void HandleStateChanged(PlayerState previousState, PlayerState newState)
    {
        Debug.Log("Cambio de estado: " + previousState + " -> " + newState);

        switch (newState)
        {
            case PlayerState.DoubleJump:
                animator.SetTrigger(DoubleJumpHash);
                break;

            case PlayerState.WallGrab:
                animator.SetTrigger(WallGrabHash);
                break;

            case PlayerState.WallJump:
                animator.SetTrigger(WallJumpHash);
                break;

            case PlayerState.Dash:
                animator.SetTrigger(DashHash);
                break;

            case PlayerState.Attack:
                animator.SetTrigger(AttackHash);
                break;

            case PlayerState.Parry:
                animator.SetTrigger(ParryHash);
                break;

            case PlayerState.Hurt:
                animator.SetTrigger(HurtHash);
                break;

            case PlayerState.Dead:
                animator.SetBool(IsDeadHash, true);

                if (!deathSoundStarted)
                {
                    deathSoundInstance =
                        AudioController.Instance.PlayPlayerStatus(PlayerStatusSFX.Death);

                    deathSoundStarted = true;
                }

                break;

            case PlayerState.Idle:
                animator.SetBool(IsDeadHash, false);
                break;

            case PlayerState.Walk:
            case PlayerState.Run:
            case PlayerState.Jump:
            case PlayerState.Airborne:
            case PlayerState.WallSlide:
                break;
        }
    }

    /// <summary>
    /// Se ejecuta mediante un Animation Event al finalizar la animación Death.
    /// </summary>
    public void OnDeathAnimationFinished()
    {
        StartCoroutine(WaitForDeathSound());
    }

    private IEnumerator WaitForDeathSound()
    {
        FMOD.Studio.PLAYBACK_STATE playbackState;

        while (deathSoundInstance.isValid())
        {
            deathSoundInstance.getPlaybackState(out playbackState);

            if (playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED)
                break;

            yield return null;
        }

        if (deathSoundInstance.isValid())
        {
            deathSoundInstance.release();
            deathSoundInstance.clearHandle();
        }

        deathSoundStarted = false;

        playerController.RespawnPlayer();
    }

    #endregion
}