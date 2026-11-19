using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Collections;
using UnityEngine;

public class FinalBossEnemyAI : BossAI
{
    private enum BOSS_STATE { INTRO, IDLE, STUNNED, SLAMMING, RAINING, SHOOTING, PUNCHING };

    [Header("States:")]
    [SerializeField] private BOSS_STATE state;
    [SerializeField] private BOSS_STATE defaultState;
    [SerializeField] private BOSS_STATE lastAttackState;
    [SerializeField] private bool slamAfterIntro = false;
    [SerializeField] private bool rainAfterIntro = false;
    [SerializeField] private bool shootAfterIntro = false;
    [SerializeField] private bool punchAfterIntro = false;

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
    [SerializeField] private int timesStunned = 0;
    [SerializeField] private float stunnedTime = 4f;

    [Header("Slam Attack:")]
    [SerializeField] private int slamDamage = 20;
    [SerializeField] private float handFollowingSpeed = 15f;
    [SerializeField] private float slamHandBeneathDistance = 7f;
    [SerializeField] private float slamFollowDelay = 3f;
    [SerializeField] private float anticipationDistance = 2f;
    [SerializeField] private float slamDelay = 0.6f;
    [SerializeField] private float slammingSpeed = 25f;
    [SerializeField] private float slamRecoveryDelay = 1.5f;
    [SerializeField] private int minSlamProjectiles = 2;
    [SerializeField] private int maxSlamProjectiles = 4;
    [SerializeField] private float minSlamProjectileDistance = 5f;
    [SerializeField] private int knocksToAddDebris = 2;

    [Header("Raining Attack:")]
    [SerializeField] private GameObject fallingDebrisPrefab;
    [SerializeField] private int debrisDamage = 10;
    [SerializeField] private int minDebris = 15;
    [SerializeField] private int maxDebris = 20;
    [SerializeField] private float rainDelay = 1f;
    [SerializeField] private float rainWaitDelay = 1f;
    [SerializeField] private float debrisDelay = 0.4f;
    [SerializeField] private float debrisHorizontalArea = 15f;
    [SerializeField] private float debrisVerticalPosition = 10f;
    [SerializeField] private float minDebrisDistance = 3f;
    [SerializeField] private float rainRecoveryDelay = 2f;

    [Header("Shooting Attack:")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int projectileDamage = 10;
    [SerializeField] private Transform leftArenaCorner;
    [SerializeField] private Transform rightArenaCorner;
    [SerializeField] private float shootingPreparationDelay = 2f;
    [SerializeField] private float projectileSpeed = 7f;
    [SerializeField] private int minShots = 5;
    [SerializeField] private int maxShots = 8;
    [SerializeField] private float shotsDelay = 1f;

    [Header("Punch Attack:")]
    [SerializeField] private int punchDamage = 10;
    [SerializeField] private GameObject punchWarning;
    [SerializeField] private int maxConsecutivePunches = 4;
    [SerializeField] private float punchingSpeed = 30f;
    [SerializeField] private float punchDelay = 1f;
    [SerializeField] private float punchHandBeneathDistance = 10f;
    [SerializeField] private float warningDelay = 0.4f;
    [SerializeField] private float hitboxLifetime = 0.1f;
    [SerializeField] private float punchRecoveryDelay = 1f;

    [Header("Difficulty:")]
    [SerializeField, Min(2)] private int healthToDivideDifficulty = 2;

    private float anticipation = 0f;

    private bool alreadySlamming = false;
    private bool alreadyRaining = false;
    private bool alreadyShooting = false;
    private bool alreadyPunching = false;
    private bool isFollowing;
    private bool movementEnabled = true;
    private bool damagedWhileStunned = false;

    private int consecutivePunches = 0;

    private bool lastProjectileShot = false;

    private float timer = 1f;

    private Vector3 spawnChestPosition;
    private Vector3 spawnNeckRotation;
    private Vector3 lastTargetPosition;
    private Vector3 chosenCornerPosition;

    private FinalBossHand primaryBossHand;
    private FinalBossHand secondaryBossHand;

    private List<GameObject> spawneables = new List<GameObject>();

    #region MAIN LOOP

    private void Awake()
    {
        InitializeBasicComponents();

        defaultState = state;
        currentArmourHealth = maxArmourHealth;
    }

    private void OnEnable()
    {
        characterStats.OnDeath += ToDead;
        characterStats.OnDamageTaken += DamageReaction;
        characterStats.OnDamageBlocked += DamageArmour;
    }

    private void OnDisable()
    {
        characterStats.OnDeath -= ToDead;
        characterStats.OnDamageTaken -= DamageReaction;
        characterStats.OnDamageBlocked -= DamageArmour;
    }

    private void Start()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation.eulerAngles;

        spawnChestPosition = chest.transform.position;
        spawnNeckRotation = neck.transform.rotation.eulerAngles;

        leftArm.SetOwnerBoss(this);
        rightArm.SetOwnerBoss(this);
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

            case BOSS_STATE.SHOOTING:
                OnShooting();
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
            if (slamAfterIntro)
            {
                ToSlamming();
                return;
            }

            if (rainAfterIntro)
            {
                ToRaining();
                return;
            }

            if (shootAfterIntro)
            {
                ToShooting();
                return;
            }

            if (punchAfterIntro)
            {
                ToPunching();
                return;
            }
        }
    }

    // Comportamiento en estado STUNNED
    private void OnStunned()
    {
        if (!damagedWhileStunned) return;

        timer -= Time.fixedDeltaTime;

        if (timer <= 0)
        {
            ResetArmour();
            DisarmBoss(false);
            rightArm.SetColliderActive(true);
            leftArm.SetColliderActive(true);
            ToRaining();
            damagedWhileStunned = false;
        }
    }

    // Comportamiento en estado SLAMMING
    private void OnSlamming()
    {
        if (isFollowing)
        {
            Vector3 targetFollowPosition = new Vector3(targetTransform.position.x, transform.position.y + (slamHandBeneathDistance + anticipation), targetTransform.position.z);

            primaryBossHand.MoveToPosition(targetFollowPosition, handFollowingSpeed);

            lastTargetPosition = targetTransform.position;
        }

        BodyMovement();
    }

    // Comportamiento en estado RAINING
    private void OnRaining()
    {
        if (isFollowing)
        {
            rightArm.MoveToPosition(new Vector3(rightArm.transform.position.x, transform.position.y + 2f, 0), handFollowingSpeed);
            leftArm.MoveToPosition(new Vector3(leftArm.transform.position.x, transform.position.y + 2f, 0), handFollowingSpeed);
            return;
        }

        BodyMovement();
    }

    // Comportamiento en estado SHOOTING
    private void OnShooting()
    {
        if (isFollowing)
        {
            secondaryBossHand.MoveToPosition(chosenCornerPosition, handFollowingSpeed);
            return;
        }

        BodyMovement();
    }

    // Comportamiento en estado PUNCHING
    private void OnPunching()
    {
        if (isFollowing)
        {
            Vector3 targetFollowPosition = new Vector3(targetTransform.position.x, targetTransform.position.y + punchHandBeneathDistance, targetTransform.position.z);

            primaryBossHand.MoveToPosition(targetFollowPosition, slammingSpeed);

            lastTargetPosition = targetTransform.position;
        }

        BodyMovement();
    }

    #endregion

    #region STATE TRANSITION

    // Comportamiento de transición a STUNNED
    private void ToStunned()
    {
        timesStunned++;

        timer = stunnedTime;

        ResetArmsOffset();

        rightArm.ResetHand();
        rightArm.SetColliderActive(false);
        leftArm.ResetHand();
        leftArm.SetColliderActive(false);

        DisarmBoss(true);
        state = BOSS_STATE.STUNNED;
    }

    // Comportamiento de transición a SLAMMING
    private void ToSlamming()
    {
        if (!alreadySlamming)
        {
            alreadySlamming = true;

            primaryBossHand = ClosestHandToTarget(targetTransform.position);

            StartCoroutine(SlamProcess());

            state = BOSS_STATE.SLAMMING;
            lastAttackState = state;
        }
    }

    // Comportamiento de transición a RAINING
    private void ToRaining()
    {
        if (!alreadyRaining)
        {
            alreadyRaining = true;

            StartCoroutine(RainingProcess());

            state = BOSS_STATE.RAINING;
            lastAttackState = state;
        }
    }

    // Comportamiento de transición a SHOOTING
    private void ToShooting()
    {
        if (!alreadyShooting)
        {
            alreadyShooting = true;

            chosenCornerPosition = FarthestArenaCornerToTarget(targetTransform.position);
            secondaryBossHand = FarthestHandToTarget(targetTransform.position);

            StartCoroutine(ShootingProcess());

            state = BOSS_STATE.SHOOTING;
            lastAttackState = state;
        }
    }

    // Comportamiento de transición a PUNCHING
    private void ToPunching()
    {
        if (!alreadyPunching)
        {
            alreadyPunching = true;

            primaryBossHand = ClosestHandToTarget(targetTransform.position);

            StartCoroutine(PunchingProcess());

            state = BOSS_STATE.PUNCHING;
            lastAttackState = state;
        }
    }

    #endregion

    #region STATE CONDITION

    // Seqüencia de ataque slam del enemigo
    private IEnumerator SlamProcess()
    {
        isFollowing = true;

        primaryBossHand.transform.Rotate(new Vector3(0, 0, 90));

        yield return new WaitForSeconds(slamFollowDelay);

        Vector3 targetSlamDirection = (lastTargetPosition - primaryBossHand.transform.position).normalized;

        anticipation = anticipationDistance;

        yield return new WaitForSeconds(slamDelay);

        anticipation = 0;

        isFollowing = false;

        primaryBossHand.MoveTillCollision(targetSlamDirection, slammingSpeed);

        yield return new WaitUntil(() => primaryBossHand.ArmCollided);

        if (timesStunned >= knocksToAddDebris)
        {
            StartCoroutine(DebrisGeneration(minSlamProjectiles, maxSlamProjectiles, minSlamProjectileDistance));
        }
        
        yield return new WaitForSeconds(slamRecoveryDelay);

        primaryBossHand.ResetHand();
        alreadySlamming = false;

        ProbabilityToAct(100f, ToPunching);
    }

    // Seqüencia de ataque raining del enemigo
    private IEnumerator RainingProcess()
    {
        movementEnabled = false;

        yield return new WaitUntil(() => BothArmsOnPosition());

        isFollowing = true;
        movementEnabled = true;

        rightArm.transform.Rotate(new Vector3(0, 0, 200f));
        leftArm.transform.Rotate(new Vector3(0, 0, -200f));

        yield return new WaitForSeconds(rainDelay);

        yield return new WaitForSeconds(rainWaitDelay);

        StartCoroutine(DebrisGeneration(minDebris, maxDebris, minDebrisDistance));

        yield return new WaitForSeconds(rainRecoveryDelay);

        isFollowing = false;

        leftArm.ResetHand();
        rightArm.ResetHand();

        alreadyRaining = false;

        ProbabilityToAct(60f, ToShooting, ToPunching);
    }

    // Seqüencia de ataque shooting del enemigo
    private IEnumerator ShootingProcess()
    {
        isFollowing = true;

        bool targetOnLeft = (targetTransform.position.x - chosenCornerPosition.x) < 0;

        secondaryBossHand.transform.Rotate(new Vector3(0, 0, targetOnLeft ? -90 : 90));

        yield return new WaitForSeconds(shootingPreparationDelay);

        StartCoroutine(ProjectileGeneration(targetOnLeft ? -Vector3.right : Vector3.right));

        yield return new WaitUntil(() => lastProjectileShot);

        isFollowing = false;
        lastProjectileShot = false;

        secondaryBossHand.ResetHand();

        alreadyShooting = false;

        ProbabilityToAct(100, ToSlamming);
    }

    // Genera un numero de proyectiles que se mueven en una direccion especifica
    private IEnumerator ProjectileGeneration(Vector3 projectileDirection)
    {
        int projectileQuantity = Random.Range(minShots, maxShots);

        for (int i = 0; i < projectileQuantity; i++)
        {
            GameObject newProjectile = Instantiate(projectilePrefab, secondaryBossHand.transform.position, Quaternion.identity);
            newProjectile.transform.localScale = new Vector3(2, 2, 2);
            spawneables.Add(newProjectile);

            EnemyProjectileController projectileController = newProjectile.GetComponent<EnemyProjectileController>();
            projectileController.SetLinearDirection(projectileSpeed, projectileDirection);
            projectileController.SetContactDamage(projectileDamage);

            yield return new WaitForSeconds(shotsDelay);

            lastProjectileShot = i == projectileQuantity - 1;
        }
    }

    // Genera un numero de escombros que caen del techo con cierto delay
    private IEnumerator DebrisGeneration(int minDebris, int maxDebris, float minDebrisDistance)
    {
        List<float> usedPositions = new List<float>();

        int debrisQuantity = Random.Range(minDebris, maxDebris);

        for (int i = 0; i < debrisQuantity; i++)
        {
            float horizontalPosition = 0;
            bool validPosition = false;
            int placingAttempts = 0;

            while (!validPosition && placingAttempts < 20)
            {
                placingAttempts++;

                horizontalPosition = Random.Range(targetTransform.position.x - debrisHorizontalArea, targetTransform.position.x + debrisHorizontalArea);

                validPosition = true;

                foreach (float position in usedPositions)
                {
                    if (Mathf.Abs(horizontalPosition - position) < minDebrisDistance)
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

            GameObject newProjectile = Instantiate(fallingDebrisPrefab, new Vector3(horizontalPosition, transform.position.y + debrisVerticalPosition, 0), Quaternion.identity);
            spawneables.Add(newProjectile);

            EnemyProjectileController projectileController = newProjectile.GetComponent<EnemyProjectileController>();
            projectileController.SetContactDamage(debrisDamage);

            yield return new WaitForSeconds(debrisDelay);
        }
    }

    // Seqüencia de ataque punching del enemigo
    private IEnumerator PunchingProcess()
    {
        isFollowing = true;

        primaryBossHand.transform.Rotate(new Vector3(0, 0, 180));

        yield return new WaitForSeconds(punchDelay);

        isFollowing = false;

        Vector3 targetSlamDirection = (lastTargetPosition - primaryBossHand.transform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(lastTargetPosition, Vector3.down, 20f, groundLayer);

        GameObject warning = null;
        HitboxWithWarning hitbox = null;

        if (hit.collider != null)
        {
            warning = Instantiate(punchWarning, hit.point, Quaternion.identity);
            spawneables.Add(warning);
            hitbox = warning.GetComponent<HitboxWithWarning>();
        }

        yield return new WaitForSeconds(warningDelay);

        primaryBossHand.transform.Rotate(new Vector3(0, 0, -180));

        primaryBossHand.MoveTillCollision(targetSlamDirection, punchingSpeed);

        yield return new WaitUntil(() => primaryBossHand.ArmCollided);

        if (hitbox != null)
        {
            hitbox.SetContactDamage(punchDamage);
            hitbox.SetHitboxActive(true);
        }

        yield return new WaitForSeconds(hitboxLifetime);

        if (warning != null)
        {
            spawneables.Remove(warning);
            Destroy(warning);
        }

        yield return new WaitForSeconds(punchRecoveryDelay);

        primaryBossHand.ReturnToOrigin();

        consecutivePunches++;

        alreadyPunching = false;

        if (consecutivePunches <= maxConsecutivePunches)
        {
            ToPunching();
        }
        else
        {
            consecutivePunches = 0;

            if (lastAttackState == BOSS_STATE.RAINING)
            {
                ToShooting();
            }
            else
            {
                ProbabilityToAct(70f, ToRaining, ToSlamming);
            }
        }
    }

    private void DisarmBoss(bool value)
    {
        characterStats.SetCanTakeDamage(value);
        head.SetActive(value);
        hookPoints.SetActive(value);
        helmet.SetActive(!value);
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

        if (movementEnabled)
        {
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
        else
        {
            ResetArmsOffset();
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
            alreadyPunching = false;
            alreadyRaining = false;
            alreadyShooting = false;

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

    private FinalBossHand ClosestHandToTarget(Vector3 targetPosition)
    {
        float leftArmDistance = (leftArm.transform.position - targetPosition).sqrMagnitude;
        float rightArmDistance = (rightArm.transform.position - targetPosition).sqrMagnitude;

        return leftArmDistance <= rightArmDistance ? leftArm : rightArm;
    }

    private FinalBossHand FarthestHandToTarget(Vector3 targetPosition)
    {
        float leftArmDistance = (leftArm.transform.position - targetPosition).sqrMagnitude;
        float rightArmDistance = (rightArm.transform.position - targetPosition).sqrMagnitude;

        return leftArmDistance >= rightArmDistance ? leftArm : rightArm;
    }

    private Vector3 FarthestArenaCornerToTarget(Vector3 targetPosition)
    {
        float leftCornerDistance = (leftArenaCorner.transform.position - targetPosition).sqrMagnitude;
        float rightCornerDistance = (rightArenaCorner.transform.position - targetPosition).sqrMagnitude;

        return leftCornerDistance >= rightCornerDistance ? leftArenaCorner.position : rightArenaCorner.position;
    }

    private bool BothArmsOnPosition()
    {
        return leftArm.OnOrigin && rightArm.OnOrigin;
    }

    public void ArmHitPlayer(Transform armTransform, Collider2D collision)
    {
        int damageQuantity = 0;

        switch (state)
        {
            case BOSS_STATE.SLAMMING:
                damageQuantity = slamDamage;
                break;

            case BOSS_STATE.PUNCHING:
                damageQuantity = punchDamage;
                break;

            default:
                damageQuantity = contactDamage;
                break;
        }

        hitTargets.Clear();
        CombatDamage damage = new CombatDamage(damageQuantity, characterStats, armTransform.position, false);
        CombatManager.MakeDamage(armTransform, collision, hitTargets, damage, characterStats.Team);
    }
    
    private void DamageReaction()
    {
        FlashColor();

        if (state == BOSS_STATE.STUNNED)
        {
            damagedWhileStunned = true;
        }
    }

    private void ToDead()
    {
        isDead = true;
        gameObject.SetActive(false);
        bossArea.DisableDoors();
    }

    private void DestroySpawneables()
    {
        foreach (GameObject spawneable in spawneables)
        {
            if (spawneable != null)
            {
                Destroy(spawneable);
            }
        }

        spawneables.Clear();
    }

    public override void ResetEnemy()
    {
        if (isDead) return;
        
        Debug.Log("FINAL BOSS RESETED");

        bossArea.DisableDoors();

        gameObject.SetActive(true);

        timer = 1f;

        isFollowing = false;
        alreadySlamming = false;
        alreadyPunching = false;
        alreadyRaining = false;
        alreadyShooting = false;

        StopAllCoroutines();
        DestroySpawneables();

        state = defaultState;
        targetTransform = null;

        ResetColor();
        localMovement = Vector3.zero;
        externalMovement = Vector3.zero;
        isDead = false;
        bossActive = false;
        characterStats.FullHeal();

        chest.transform.position = spawnChestPosition;
        neck.transform.rotation = Quaternion.Euler(spawnNeckRotation);

        ResetArmour();
        DisarmBoss(false);
        rightArm.SetColliderActive(true);
        leftArm.SetColliderActive(true);
        leftArm.ResetHand();
        rightArm.ResetHand();

        transform.position = spawnPosition;
        transform.rotation = Quaternion.Euler(spawnRotation);
        invertedDirection = neck.transform.rotation == Quaternion.Euler(0, 180, 0);
    }

    #endregion
}
