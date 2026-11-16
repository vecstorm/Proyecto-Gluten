using UnityEngine;

public class FinalBossEnemyAI : BossAI
{
    private enum BOSS_STATE { INTRO, STUNNED, PREPARING_SLAM, SLAMMING, PREPARING_RAIN, RAINING, PREPARING_CLAP, CLAPPING, PREPARING_PUNCH, PUNCHING };

    [SerializeField] private BOSS_STATE bossState;
    [SerializeField, Min(2)] private int healthToDivideDifficulty = 2;

    [SerializeField] private FinalBossHand leftArm;
    [SerializeField] private FinalBossHand rightArm;

    [Header("Intro / Idle:")]
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private float introTime = 1f;

    [Header("Slam Attack:")]
    [SerializeField] private float handFollowingSpeed = 15f;
    [SerializeField] private float handBeneathDistance = 7f;
    [SerializeField] private float slamDelay = 3f;

    private float timer = 1f;

    private BOSS_STATE defaultBossState;

    private void Awake()
    {
        InitializeBasicComponents();

        defaultBossState = bossState;
    }

    private void Start()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation.eulerAngles;
    }

    private void FixedUpdate()
    {
        if (!bossActive)
        {
            return;
        }

        switch (bossState)
        {
            case BOSS_STATE.INTRO:
                OnIntro();
                break;

            case BOSS_STATE.STUNNED:
                break;

            case BOSS_STATE.PREPARING_SLAM:
                OnPreparingSlam();
                break;

            case BOSS_STATE.SLAMMING:
                OnSlamming();
                break;

            case BOSS_STATE.PREPARING_RAIN:
                break;

            case BOSS_STATE.RAINING:
                break;

            case BOSS_STATE.PREPARING_CLAP:
                break;

            case BOSS_STATE.CLAPPING:
                break;

            case BOSS_STATE.PREPARING_PUNCH:
                break;

            case BOSS_STATE.PUNCHING:
                break;
        }
    }

    // Comportamiento en estado INTRO
    private void OnIntro()
    {
        timer -= Time.fixedDeltaTime;

        if (timer <= 0)
        {
            ToPreparingSlam();
        }
    }

    private void OnPreparingSlam()
    {
        timer -= Time.fixedDeltaTime;

        Vector3 targetFollowPosition = new Vector3(targetTransform.position.x, targetTransform.position.y + handBeneathDistance, targetTransform.position.z);

        rightArm.MoveToPosition(targetFollowPosition, handFollowingSpeed);

        if (timer <= 0f)
        {
            ToSlamming();
        }
    }

    private void OnSlamming()
    {

    }

    private void ToPreparingSlam()
    {
        timer = slamDelay;
        bossState = BOSS_STATE.PREPARING_SLAM;
    }

    private void ToSlamming()
    {
        bossState = BOSS_STATE.SLAMMING;
    }

    public override void ResetEnemy()
    {
        
    }
}
