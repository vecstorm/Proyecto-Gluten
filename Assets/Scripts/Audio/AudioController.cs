using UnityEngine;
using UnityEngine.SceneManagement;

#region SFX

#region Player

/// <summary>
/// Acciones de movimiento del jugador que producen sonido.
/// </summary>
public enum PlayerMovementSFX
{
    Jump,
    DoubleJump,
    Land,
    Dash,
    WallGrab,
    WallSlide,
    WallJump,
    HookLaunch,
    HookAttach,
    HookRetract,
    HookPull,
    HookRelease,
    Footstep
}

/// <summary>
/// Acciones de combate del jugador que producen sonido.
/// </summary>
public enum PlayerCombatSFX
{
    MeleeAttack,
    RangedAttack,
    ParryStart,
    ParrySuccess
}

/// <summary>
/// Eventos relacionados con el estado y la supervivencia del jugador.
/// </summary>
public enum PlayerStatusSFX
{
    Hurt,
    Death,
    Respawn
}

/// <summary>
/// Interacciones del jugador con objetos y elementos del mundo.
/// </summary>
public enum PlayerInteractionSFX
{
    CollectExperience,
    SitAtSavePoint,
    UpgradePoints,
    UnlockAbility,
    OpenDoor,
    CollectKey
}

#endregion

#region Enemies

public enum PatrolEnemySFX
{
    Footstep,
    Hurt,
    Death
}

public enum MeleeEnemySFX
{
    Footstep,
    FastFootstep,
    Hurt,
    Death,
    Stun,
    AttackPrepare,
    Attack
}

public enum FlyingEnemySFX
{
    Flapping,
    Shoot,
    ProjectileMovement,
    ProjectileImpact,
    Transform,
    Charge,
    Explode,
    Hurt,
    Death
}

public enum RangedEnemySFX
{
    Footstep,
    Shoot,
    ProjectileMovement,
    ProjectileImpact,
    Hurt,
    Death
}

public enum MiniBossSFX
{
    Intro,
    AttackPrepare,
    AttackSwing,
    HeavyAttackPrepare,
    HeavyAttackImpact,
    SpikeSpawn,
    JumpPrepare,
    JumpTakeoff,
    JumpLand,
    Hurt,
    Death
}

public enum FinalBossSFX
{
    Intro,
    BodyMovement,
    MoveArm,
    PunchSwing,
    PunchImpact,
    SlamSwing,
    SlamImpact,
    DebrisSpawn,
    DebrisFalling,
    DebrisImpact,
    CannonPrepare,
    CannonLoad,
    CannonShoot,
    HelmetOpen,
    HelmetClose,
    ChestOpen,
    ChestClose,
    ArmourBreak,
    ArmHurt,
    HeadHurt,
    Stun,
    Death
}

#endregion

#region UI

/// <summary>
/// Interacciones con los elementos de la interfaz.
/// </summary>
public enum UISFX
{
    ButtonHover,
    ButtonClick,
    SliderMove,
    TabChange,
    MenuOpen,
    MenuClose,
    Back,
    Confirm,
    Cancel
}

#endregion

#endregion

public class AudioController : MonoBehaviour
{
    #region Singleton
    public static AudioController Instance { get; private set; }

    #endregion

    #region FMOD Events

    [Header("Music")]
    [SerializeField] private FMODUnity.EventReference mainSong;
    [SerializeField] private FMODUnity.EventReference creditsSong;

    [Header("Player")]
    [SerializeField] private FMODUnity.EventReference playerMovementSFX;
    [SerializeField] private FMODUnity.EventReference playerFootstepSFX;
    [SerializeField] private FMODUnity.EventReference playerCombatSFX;
    [SerializeField] private FMODUnity.EventReference playerStatusSFX;
    [SerializeField] private FMODUnity.EventReference playerInteractionSFX;

    [Header("Enemies")]
    [SerializeField] private FMODUnity.EventReference patrolEnemySFX;
    [SerializeField] private FMODUnity.EventReference meleeEnemySFX;
    [SerializeField] private FMODUnity.EventReference flyingEnemySFX;
    [SerializeField] private FMODUnity.EventReference rangedEnemySFX;

    [Header("Bosses")]
    [SerializeField] private FMODUnity.EventReference miniBossSFX;
    [SerializeField] private FMODUnity.EventReference finalBossSFX;

    [Header("UI")]
    [SerializeField] private FMODUnity.EventReference uiSFX;

    #endregion

    #region Music Instances

    private FMOD.Studio.EventInstance mainSongInstance;
    private FMOD.Studio.EventInstance creditsSongInstance;

    #endregion

    #region Singleton

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CreateMusicInstances();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        StopAndRelease(ref mainSongInstance);
        StopAndRelease(ref creditsSongInstance);

        Instance = null;
    }

    #endregion

    #region Music

    private void CreateMusicInstances()
    {
        mainSongInstance = FMODUnity.RuntimeManager.CreateInstance(mainSong);
        creditsSongInstance = FMODUnity.RuntimeManager.CreateInstance(creditsSong);
    }

    private void Start()
    {
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    private void PlayMusicForScene(string sceneName)
    {
        StopMusicInstances();

        if (sceneName == "MainMenuGame")
        {
            mainSongInstance.start();
        }
        else if (sceneName == "CreditsScene")
        {
            creditsSongInstance.start();
        }
    }

    private void StopMusicInstances()
    {
        StopInstance(mainSongInstance);
        StopInstance(creditsSongInstance);
    }

    /// <summary>
    /// Pausa o reanuda la música principal.
    /// </summary>
    public void PauseAndResumeMainSong()
    {
        if (mainSongInstance.getPaused(out bool paused) == FMOD.RESULT.OK)
        {
            mainSongInstance.setPaused(!paused);
        }
    }

    #endregion

    #region Generic SFX Methods

    /// <summary>
    /// Reproduce un sonido puntual y libera su instancia.
    /// </summary>
    private void PlayEvent(FMODUnity.EventReference eventReference, string parameterName, int action)
    {
        FMOD.Studio.EventInstance instance = FMODUnity.RuntimeManager.CreateInstance(eventReference);

        instance.setParameterByName(parameterName, action);
        instance.start();
        instance.release();
    }

    /// <summary>
    /// Inicia un sonido que necesita mantenerse activo.
    /// Devuelve la instancia para poder detenerla posteriormente.
    /// </summary>
    private FMOD.Studio.EventInstance StartLoopEvent(FMODUnity.EventReference eventReference, string parameterName, int action)
    {
        FMOD.Studio.EventInstance instance = FMODUnity.RuntimeManager.CreateInstance(eventReference);

        instance.setParameterByName(parameterName, action);
        instance.start();

        return instance;
    }

    /// <summary>
    /// Detiene y libera una instancia de sonido.
    /// </summary>
    public void StopSFX(ref FMOD.Studio.EventInstance instance)
    {
        if (!instance.isValid())
            return;

        instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        instance.release();
        instance.clearHandle();
    }

    private void StopInstance(FMOD.Studio.EventInstance instance)
    {
        if (instance.isValid())
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    private void StopAndRelease(ref FMOD.Studio.EventInstance instance)
    {
        if (!instance.isValid())
            return;

        instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        instance.release();
        instance.clearHandle();
    }

    #endregion

    #region Player

    public void PlayPlayerMovement(PlayerMovementSFX action)
    {
        PlayEvent(playerMovementSFX, "PlayerMovementActions", (int)action);
    }

    public FMOD.Studio.EventInstance StartPlayerMovementLoop(PlayerMovementSFX action)
    {
        return StartLoopEvent(playerMovementSFX, "PlayerMovementActions", (int)action);
    }

    public void PlayPlayerCombat(PlayerCombatSFX action)
    {
        PlayEvent(playerCombatSFX, "PlayerCombatActions", (int)action);
    }

    public FMOD.Studio.EventInstance PlayPlayerStatus(PlayerStatusSFX action)
    {
        FMOD.Studio.EventInstance instance =
            FMODUnity.RuntimeManager.CreateInstance(playerStatusSFX);

        instance.setParameterByName("PlayerStatusActions", (int)action);
        instance.start();

        return instance;
    }

    public void PlayPlayerInteraction(PlayerInteractionSFX action)
    {
        PlayEvent(playerInteractionSFX, "PlayerInteractionActions", (int)action);
    }

    #endregion

    #region Enemies

    public void PlayPatrolEnemy(PatrolEnemySFX action)
    {
        PlayEvent(patrolEnemySFX, "PatrolEnemyActions", (int)action);
    }

    public void PlayMeleeEnemy(MeleeEnemySFX action)
    {
        PlayEvent(meleeEnemySFX, "MeleeEnemyActions", (int)action);
    }

    public void PlayFlyingEnemy(FlyingEnemySFX action)
    {
        PlayEvent(flyingEnemySFX, "FlyingEnemyActions", (int)action);
    }

    public void PlayRangedEnemy(RangedEnemySFX action)
    {
        PlayEvent(rangedEnemySFX, "RangedEnemyActions", (int)action);
    }

    #endregion

    #region Bosses

    public void PlayMiniBoss(MiniBossSFX action)
    {
        PlayEvent(miniBossSFX, "MiniBossActions", (int)action);
    }

    public void PlayFinalBoss(FinalBossSFX action)
    {
        PlayEvent(finalBossSFX, "FinalBossActions", (int)action);
    }

    #endregion

    #region UI

    public void PlayUI(UISFX action)
    {
        PlayEvent(uiSFX, "UIActions", (int)action);
    }

    #endregion
}