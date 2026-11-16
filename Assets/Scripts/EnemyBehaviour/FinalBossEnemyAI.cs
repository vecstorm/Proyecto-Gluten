using System.Collections;
using UnityEngine;

public class FinalBossEnemyAI : BossAI
{
    private enum BOSS_STATE { INTRO, IDLE, STUNNED, SLAMMING, PREPARING_RAIN, RAINING, PREPARING_CLAP, CLAPPING, PUNCHING };

    [SerializeField] private BOSS_STATE bossState;
    [SerializeField, Min(2)] private int healthToDivideDifficulty = 2;

    [SerializeField] private FinalBossHand leftArm;
    [SerializeField] private FinalBossHand rightArm;

    [Header("Intro / Idle:")]
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private float introTime = 1f;

    [Header("Slam Attack:")]
    [SerializeField] private float handFollowingSpeed = 15f;
    [SerializeField] private float slamHandBeneathDistance = 7f;
    [SerializeField] private float slamFollowDelay = 3f;
    [SerializeField] private float anticipationDistance = 2f;
    [SerializeField] private float slamDelay = 0.6f;
    [SerializeField] private float slammingSpeed = 25f;
    [SerializeField] private float slamRecoveryDelay = 1.5f;

    [Header("Clap Attack:")]
    [SerializeField] private float clapFollowDelay = 5f;
    [SerializeField] private float handHorizontalDistance = 10f;

    [Header("Punch Attack:")]
    [SerializeField] private GameObject punchWarning;
    [SerializeField] private float punchingSpeed = 30f;
    [SerializeField] private float punchDelay = 1f;
    [SerializeField] private float punchHandBeneathDistance = 10f;
    [SerializeField] private float warningDelay = 0.4f;
    [SerializeField] private float punchRecoveryDelay = 1f;

    private float anticipation = 0f;

    private bool alreadySlamming = false;
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

            case BOSS_STATE.RAINING:
                break;

            case BOSS_STATE.CLAPPING:
                OnClapping();
                break;

            case BOSS_STATE.PUNCHING:
                OnPunching();
                break;
        }
    }

    // Comportamiento en estado INTRO
    private void OnIntro()
    {
        timer -= Time.fixedDeltaTime;

        if (timer <= 0)
        {
            //ToSlamming();
            //ToClapping();
            ToPunching();
        }
    }

    private void OnSlamming()
    {
        if (isFollowing)
        {
            Vector3 targetFollowPosition = new Vector3(targetTransform.position.x, targetTransform.position.y + (slamHandBeneathDistance + anticipation), targetTransform.position.z);

            rightArm.MoveToPosition(targetFollowPosition, handFollowingSpeed);

            lastTargetPosition = targetTransform.position;
        }
    }

    private void OnClapping()
    {
        if (isFollowing)
        {
            rightArm.MoveToPosition(new Vector3(transform.position.x + handHorizontalDistance, rightArm.transform.position.y, 0), handFollowingSpeed);
            leftArm.MoveToPosition(new Vector3(transform.position.x - handHorizontalDistance, leftArm.transform.position.y, 0), handFollowingSpeed);
        }
    }

    private void OnPunching()
    {
        if (isFollowing)
        {
            Vector3 targetFollowPosition = new Vector3(targetTransform.position.x, targetTransform.position.y + punchHandBeneathDistance, targetTransform.position.z);

            leftArm.MoveToPosition(targetFollowPosition, slammingSpeed);

            lastTargetPosition = targetTransform.position;
        }
    }

    private void ToSlamming()
    {
        if (!alreadySlamming)
        {
            StartCoroutine(SlamProcess());
        }
        
        bossState = BOSS_STATE.SLAMMING;
    }

    private void ToClapping()
    {
        StartCoroutine(ClappingProcess());

        bossState = BOSS_STATE.CLAPPING;
    }

    private void ToPunching()
    {
        StartCoroutine(PunchingProcess());

        bossState = BOSS_STATE.PUNCHING;
    }

    private IEnumerator SlamProcess()
    {
        isFollowing = true;

        rightArm.transform.Rotate(new Vector3(0, 0, 90));

        yield return new WaitForSeconds(slamFollowDelay);

        Vector3 targetSlamDirection = (lastTargetPosition - rightArm.transform.position).normalized;

        anticipation = anticipationDistance;

        yield return new WaitForSeconds(slamDelay);

        anticipation = 0;

        isFollowing = false;

        rightArm.MoveTillCollision(targetSlamDirection, slammingSpeed);

        yield return new WaitUntil(() => rightArm.ArmCollided);

        yield return new WaitForSeconds(slamRecoveryDelay);

        rightArm.transform.Rotate(new Vector3(0, 0, -90));
        rightArm.ReturnToOrigin();
        alreadySlamming = false;
    }

    private IEnumerator ClappingProcess()
    {
        isFollowing = true;

        rightArm.transform.Rotate(new Vector3(0, 0, 180));
        leftArm.transform.Rotate(new Vector3(0, 0, 180));

        yield return new WaitForSeconds(slamFollowDelay);

        isFollowing = false;

        //rightArm.MoveTillCollision((leftArm.transform.position - rightArm.transform.position).normalized, slammingSpeed);
        //leftArm.MoveTillCollision((rightArm.transform.position - leftArm.transform.position).normalized, slammingSpeed);

        yield return new WaitForSeconds(0);

        rightArm.transform.Rotate(new Vector3(0, 0, -180));
        leftArm.transform.Rotate(new Vector3(0, 0, -180));
        rightArm.ReturnToOrigin();
        leftArm.ReturnToOrigin();
    }

    private IEnumerator PunchingProcess()
    {
        isFollowing = true;

        leftArm.transform.Rotate(new Vector3(0, 0, 180));

        yield return new WaitForSeconds(punchDelay);

        isFollowing = false;

        Vector3 targetSlamDirection = (lastTargetPosition - leftArm.transform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(lastTargetPosition, Vector3.down, 8f, groundLayer);

        GameObject warning = null;

        if (hit.collider != null)
        {
            warning = Instantiate(punchWarning, hit.point, Quaternion.identity);
        }

        yield return new WaitForSeconds(warningDelay);

        if (warning != null)
        {
            Destroy(warning);
        }

        leftArm.transform.Rotate(new Vector3(0, 0, -180));

        leftArm.MoveTillCollision(targetSlamDirection, punchingSpeed);

        yield return new WaitUntil(() => leftArm.ArmCollided);

        yield return new WaitForSeconds(punchRecoveryDelay);

        leftArm.ReturnToOrigin();
        ToPunching();
    }

    private bool BothArmsCollided()
    {
        return rightArm.ArmCollided && leftArm.ArmCollided;
    }

    public override void ResetEnemy()
    {
        
    }
}
