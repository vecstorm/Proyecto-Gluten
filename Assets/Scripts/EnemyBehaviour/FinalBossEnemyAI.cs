using System.Collections;
using UnityEngine;

public class FinalBossEnemyAI : BossAI
{
    private enum BOSS_STATE { INTRO, IDLE, STUNNED, SLAMMING, PREPARING_RAIN, RAINING, PREPARING_CLAP, CLAPPING, PREPARING_PUNCH, PUNCHING };

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
    [SerializeField] private float followDelay = 3f;
    [SerializeField] private float anticipationDistance = 2f;
    [SerializeField] private float slamDelay = 0.6f;
    [SerializeField] private float slammingSpeed = 25f;
    [SerializeField] private float slamRecoveryDelay = 1.5f;

    private float anticipation = 0f;

    private bool isFollowing;

    private float timer = 1f;

    private Vector3 lastTargetPosition;

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

            case BOSS_STATE.IDLE:
                break;

            case BOSS_STATE.STUNNED:
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
            //ToPreparingSlam();
            ToSlamming();
        }
    }

    private void OnSlamming()
    {
        if (isFollowing)
        {
            Vector3 targetFollowPosition = new Vector3(targetTransform.position.x, targetTransform.position.y + (handBeneathDistance + anticipation), targetTransform.position.z);

            rightArm.MoveToPosition(targetFollowPosition, handFollowingSpeed);

            lastTargetPosition = targetTransform.position;
        }
    }

    private void ToSlamming()
    {
        StartCoroutine(SlamProcess());
        bossState = BOSS_STATE.SLAMMING;
    }

    private IEnumerator SlamProcess()
    {
        isFollowing = true;

        yield return new WaitForSeconds(followDelay);

        Vector3 targetSlamDirection = (lastTargetPosition - rightArm.transform.position).normalized;

        anticipation = anticipationDistance;

        yield return new WaitForSeconds(slamDelay);

        anticipation = 0;

        isFollowing = false;

        rightArm.MoveTillCollision(targetSlamDirection, slammingSpeed);

        yield return new WaitUntil(() => rightArm.ArmCollided);

        yield return new WaitForSeconds(slamRecoveryDelay);

        rightArm.ReturnToOrigin();
    }

    public override void ResetEnemy()
    {
        
    }
}
