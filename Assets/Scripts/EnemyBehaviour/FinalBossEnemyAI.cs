using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class FinalBossEnemyAI : BossAI
{
    private enum BOSS_STATE { INTRO, IDLE, STUNNED, SLAMMING, RAINING, PREPARING_CLAP, CLAPPING, PUNCHING };

    [Header("States:")]
    [SerializeField] private BOSS_STATE state;
    [SerializeField] private BOSS_STATE defaultState;

    [Header("Armour:")]
    [SerializeField] private int maxArmourHealth = 30;
    [SerializeField] private int currentArmourHealth;

    [Header("Pieces:")]
    [SerializeField] private FinalBossHand leftArm;
    [SerializeField] private FinalBossHand rightArm;
    [SerializeField] private GameObject hookPoints;
    [SerializeField] private GameObject chest;
    [SerializeField] private GameObject neck;
    [SerializeField] private GameObject head;
    [SerializeField] private GameObject helmet;

    [Header("Movement:")]
    [SerializeField] private float movementSpeed = 7f;
    [SerializeField] private float minDistanceToWall = 8f;
    [SerializeField] private float backHandOffset = 2f;
    [SerializeField] private float frontHandOffset = 0.5f;

    [Header("Intro / Idle:")]
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private float introTime = 1f;

    [Header("Stunned:")]
    [SerializeField] private float stunnedTime = 4f;

    [Header("Slam Attack:")]
    [SerializeField] private float handFollowingSpeed = 15f;
    [SerializeField] private float slamHandBeneathDistance = 7f;
    [SerializeField] private float slamFollowDelay = 3f;
    [SerializeField] private float anticipationDistance = 2f;
    [SerializeField] private float slamDelay = 0.6f;
    [SerializeField] private float slammingSpeed = 25f;
    [SerializeField] private float slamRecoveryDelay = 1.5f;

    [Header("Raining Attack:")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int minProjectiles = 15;
    [SerializeField] private int maxProjectiles = 20;
    [SerializeField] private float rainDelay = 1f;
    [SerializeField] private float rainWaitDelay = 1f;
    [SerializeField] private float projectileDelay = 0.4f;
    [SerializeField] private float projectileHorizontalArea = 15f;
    [SerializeField] private float projectileVerticalPosition = 10f;
    [SerializeField] private float minProjectileDistance = 3f;

    [Header("Clap Attack:")]
    [SerializeField] private float clapFollowDelay = 5f;
    [SerializeField] private float handHorizontalDistance = 10f;

    [Header("Punch Attack:")]
    [SerializeField] private GameObject punchWarning;
    [SerializeField] private int maxConsecutivePunches = 4;
    [SerializeField] private float punchingSpeed = 30f;
    [SerializeField] private float punchDelay = 1f;
    [SerializeField] private float punchHandBeneathDistance = 10f;
    [SerializeField] private float warningDelay = 0.4f;
    [SerializeField] private float punchRecoveryDelay = 1f;

    [Header("Difficulty:")]
    [SerializeField, Min(2)] private int healthToDivideDifficulty = 2;

    private float anticipation = 0f;

    private bool alreadySlamming = false;
    private bool isFollowing;

    private int consecutivePunches = 0;

    private float timer = 1f;

    private Vector3 lastTargetPosition;


    #region MAIN LOOP

    private void Awake()
    {
        InitializeBasicComponents();

        defaultState = state;
        currentArmourHealth = maxArmourHealth;
    }

    private void OnEnable()
    {
        characterStats.OnDamageTaken += FlashColor;
        characterStats.OnDamageBlocked += DamageArmour;
    }

    private void OnDisable()
    {
        characterStats.OnDamageTaken -= FlashColor;
        characterStats.OnDamageBlocked -= DamageArmour;
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

        switch (state)
        {
            case BOSS_STATE.INTRO:
                OnIntro();
                break;

            case BOSS_STATE.IDLE:
                break;

            case BOSS_STATE.STUNNED:
                OnStunned();
                break;

            case BOSS_STATE.SLAMMING:
                OnSlamming();
                break;

            case BOSS_STATE.RAINING:
                OnRaining();
                break;

            case BOSS_STATE.CLAPPING:
                OnClapping();
                break;

            case BOSS_STATE.PUNCHING:
                OnPunching();
                break;
        }

        if (TargetBehind(targetTransform.position, chest.transform.position))
        {
            FlipObject(neck);
        }
    }

    #endregion

    #region STATE BEHAVIOUR

    // Comportamiento en estado INTRO
    private void OnIntro()
    {
        timer -= Time.fixedDeltaTime;

        if (timer <= 0)
        {
            ToSlamming();
            //ToClapping();
            //ToPunching();
            //ToRaining();
            //ToStunned();
        }
    }

    private void OnStunned()
    {
        timer -= Time.fixedDeltaTime;

        if (timer <= 0)
        {
            ResetArmour();
            DisarmBoss(false);
            rightArm.SetColliderActive(true);
            leftArm.SetColliderActive(true);
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

        BodyMovement();
    }

    private void OnRaining()
    {
        if (isFollowing)
        {
            rightArm.MoveToPosition(new Vector3(rightArm.transform.position.x, transform.position.y + 2f, 0), handFollowingSpeed);
            leftArm.MoveToPosition(new Vector3(leftArm.transform.position.x, transform.position.y + 2f, 0), handFollowingSpeed);
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

        BodyMovement();
    }

    #endregion

    #region STATE TRANSITION

    private void ToStunned()
    {
        timer = stunnedTime;

        ResetArmsOffset();

        rightArm.ReturnToOrigin();
        rightArm.ResetRotation();
        rightArm.SetColliderActive(false);
        leftArm.ReturnToOrigin();
        leftArm.ResetRotation();
        leftArm.SetColliderActive(false);

        DisarmBoss(true);
        state = BOSS_STATE.STUNNED;
    }

    private void ToSlamming()
    {
        if (!alreadySlamming)
        {
            StartCoroutine(SlamProcess());
        }
        
        state = BOSS_STATE.SLAMMING;
    }

    private void ToRaining()
    {
        StartCoroutine(RainingProcess());

        state = BOSS_STATE.RAINING;
    }

    private void ToClapping()
    {
        StartCoroutine(ClappingProcess());

        state = BOSS_STATE.CLAPPING;
    }

    private void ToPunching()
    {
        StartCoroutine(PunchingProcess());

        state = BOSS_STATE.PUNCHING;
    }

    #endregion

    #region STATE CONDITION

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

        rightArm.ResetRotation();
        rightArm.ReturnToOrigin();
        alreadySlamming = false;
    }

    private IEnumerator RainingProcess()
    {
        isFollowing = true;

        rightArm.transform.Rotate(new Vector3(0, 0, 200f));
        leftArm.transform.Rotate(new Vector3(0, 0, -200f));

        yield return new WaitForSeconds(rainDelay);

        isFollowing = false;

        yield return new WaitForSeconds(rainWaitDelay);

        List<float> usedPositions = new List<float>();

        int projectileQuantity = Random.Range(minProjectiles, maxProjectiles);

        for (int i = 0; i < projectileQuantity; i++)
        {
            float horizontalPosition = 0;
            bool validPosition = false;
            int placingAttempts = 0;
            
            while (!validPosition && placingAttempts < 20)
            {
                placingAttempts++;

                horizontalPosition = Random.Range(targetTransform.position.x - projectileHorizontalArea, targetTransform.position.x + projectileHorizontalArea);

                validPosition = true;

                foreach (float position in usedPositions)
                {
                    if (Mathf.Abs(horizontalPosition - position) < minProjectileDistance)
                    {
                        validPosition = false;
                    }
                }
            }

            if (!validPosition)
            {
                continue;
            }

            usedPositions.Add(horizontalPosition);

            GameObject newProjectile = Instantiate(projectilePrefab, new Vector3(horizontalPosition, transform.position.y + projectileVerticalPosition, 0), Quaternion.identity);

            yield return new WaitForSeconds(projectileDelay);
        }
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

        RaycastHit2D hit = Physics2D.Raycast(lastTargetPosition, Vector3.down, 20f, groundLayer);

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

        consecutivePunches++;

        if (consecutivePunches <= maxConsecutivePunches)
        {
            ToPunching();
        }
    }

    private void DisarmBoss(bool value)
    {
        characterStats.SetCanTakeDamage(value);
        head.SetActive(value);
        hookPoints.SetActive(value);
        helmet.SetActive(!value);
    }

    private bool BothArmsCollided()
    {
        return rightArm.ArmCollided && leftArm.ArmCollided;
    }

    private void BodyMovement()
    {
        Vector3 targetPosition = new Vector3(targetTransform.position.x, chest.transform.position.y, chest.transform.position.z);

        Vector3 movementDirection = (targetPosition - chest.transform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(chest.transform.position, movementDirection, minDistanceToWall, groundLayer);
        Debug.DrawRay(chest.transform.position, movementDirection * minDistanceToWall, Color.red);

        float targetDistance = targetTransform.position.x - chest.transform.position.x;

        if (hit.collider != null)
        {
            ResetArmsOffset();
            return;
        }

        chest.transform.position = Vector3.MoveTowards(chest.transform.position, targetPosition, movementSpeed * Time.fixedDeltaTime);

        if (Mathf.Abs(targetDistance) < 0.5f)
        {
            ResetArmsOffset();
        }
        else if (targetDistance > 0f)
        {
            leftArm.SetMovementOffset(-backHandOffset);
            rightArm.SetMovementOffset(frontHandOffset);
        }
        else if (targetDistance < 0f)
        {
            leftArm.SetMovementOffset(-frontHandOffset);
            rightArm.SetMovementOffset(backHandOffset);
        }
    }

    private void DamageArmour(CombatDamage damage)
    {
        currentArmourHealth -= (int)damage.Amount;

        if (currentArmourHealth <= 0)
        {
            currentArmourHealth = 0;

            StopAllCoroutines();

            isFollowing = false;
            alreadySlamming = false;

            ToStunned();
        }

        FlashColor(new Color(234 / 255f, 162 / 255f, 109 / 255f));

        Debug.Log("Boss blocked damage | Remaining armour: " + currentArmourHealth);
    }

    private void ResetArmsOffset()
    {
        leftArm.SetMovementOffset(0f);
        rightArm.SetMovementOffset(0f);
    }

    private void ResetArmour()
    {
        currentArmourHealth = maxArmourHealth;
    }

    private void FlipObject(GameObject objectToFlip)
    {
        invertedDirection = !invertedDirection;

        objectToFlip.transform.Rotate(0, 180, 0);
    }

    public override void ResetEnemy()
    {
        
    }

    #endregion
}
