using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class FlyingEnemyAI : EnemyAI
{
    [Header("Target Detection:")]
    [SerializeField] private float areaRadius = 10f;

    [Header("Movement:")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float timeToStopSearching = 2f;
    [SerializeField] private float minTimeToTurn = 2f;
    [SerializeField] private float maxTimeToTurn = 6f;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;

    [Header("Pathfinding:")]
    [SerializeField] private LayerMask mapMask;
    [SerializeField] private float nodeSpacing = 2f;
    [SerializeField] private int nodeRadius = 4;
    [SerializeField] private float nodeCheckRadius = 0.5f;
    [SerializeField] private float pathUpdateInterval = 0.2f;

    private List<NavigationNode> nodes = new List<NavigationNode>();
    private Vector3 lastGridCenter;

    [Header("Attack:")]
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int projectileDamage = 10;
    [SerializeField] private float projectileSpeed = 6f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackMinDistance = 6f;

    [Header("Ramming:")]
    [SerializeField] private int rammingDamage = 20;
    [SerializeField] private int healthToRam = 3;
    [SerializeField] private float rammingDelay = 1f;
    [SerializeField] private float rammingSpeed = 15f;
    [SerializeField] private float rammignTurnSpeed = 10f;

    [Header("Flying Parameters:")]
    [SerializeField] private float personalBubbleRadius = 2f;
    [SerializeField] private float transitableAreaRadius = 6f;
    [SerializeField] private float minFloorDistance = 3f;
    [SerializeField] private float maxFloorDistance = 7f;

    [Header("Flying Movement:")]
    [SerializeField] private float minMovementDelay = 3f;
    [SerializeField] private float maxMovementDelay = 7f;

    private bool decidingDirection = false;
    private Vector3 movementPoint;
    private Vector3 rammingDirection;

    private Transform targetTransform;
    private Vector3 lastTargetPos;

    private float pathUpdateTimer;
    private List<NavigationNode> currentPath = new List<NavigationNode>();
    private int currentPathIndex = 0;

    private float timer;
    private float attackTimer;

    private bool lostTarget = false;

    #region MAIN LOOP

    private void Awake()
    {
        InitializeBasicComponents();
        DecideMovementPoint();
    }

    private void Start()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation.eulerAngles;
    }

    private void OnEnable()
    {
        characterStats.OnDeath += ToDead;
        characterStats.OnDamageTaken += CheckHealthLeft;
        characterStats.OnDamageTaken += FlashColor;
        characterStats.OnDamageTaken += AddKnockback;
    }

    private void OnDisable()
    {
        characterStats.OnDeath -= ToDead;
        characterStats.OnDamageTaken -= CheckHealthLeft;
        characterStats.OnDamageTaken -= FlashColor;
        characterStats.OnDamageTaken -= AddKnockback;
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case ENEMY_STATE.PATROL:
                OnPatrol();
                break;

            case ENEMY_STATE.CHASING:
                OnChasing();
                break;

            case ENEMY_STATE.ATTACK:
                OnAttack();
                break;

            case ENEMY_STATE.RAMMING:
                OnRamming();
                break;

            case ENEMY_STATE.SEARCHING:
                OnSearching();
                break;

            case ENEMY_STATE.RETREAT:
                OnRetreat();
                break;

            case ENEMY_STATE.DEAD:
                break;
        }

        externalMovement = Vector3.Lerp(externalMovement, Vector3.zero, externalMovementDecay * Time.fixedDeltaTime);

        rb.linearVelocity = localMovement + externalMovement;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, areaRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, transitableAreaRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, personalBubbleRadius);
    
        if (movementPoint != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(movementPoint, 0.1f);
        }

        foreach (NavigationNode node in nodes)
        {
            Gizmos.color = Color.cyan;

            foreach (NavigationNode neighbour in node.Neighbours)
            {
                Gizmos.DrawLine(node.Position, neighbour.Position);
            }

            Gizmos.color = Color.darkBlue;
            Gizmos.DrawSphere(node.Position, 0.1f);
        }

        if (currentPath == null) return;

        foreach (NavigationNode node in currentPath)
        {
            if (currentPath.Count > 1)
            {
                Gizmos.color = Color.green;

                for (int i = 0; i < currentPath.Count - 1; i++)
                {
                    Gizmos.DrawLine(currentPath[i].Position, currentPath[i + 1].Position);
                }

                Gizmos.color = Color.darkGreen;
                Gizmos.DrawSphere(node.Position, 0.1f);
            }
        }
    }

    #endregion

    #region STATE BEHAVIOUR
    private void OnPatrol()
    {
        if (TargetDetectedOnArea(areaRadius)) 
        {
            ToChasing();
            return;
        } 

        timer += Time.fixedDeltaTime;

        //rb.linearVelocity = new Vector3((invertedDirection ? -1 : 1) * speed, rb.linearVelocity.y);
        localMovement = new Vector3((invertedDirection ? -1 : 1) * speed, rb.linearVelocity.y);

        if (timer >= Random.Range(minTimeToTurn, maxTimeToTurn) || DetectedWall())
        {
            ResetTimer();
            Flip();
        }
    }

    private void OnChasing()
    {
        if (!TargetDetectedOnArea(areaRadius))
        {
            ToSearching();
            return;
        }

        if (Vector3.Distance(transform.position, targetTransform.position) <= attackMinDistance) 
        {
            ToAttack();
            return;
        } 

        UpdateNodeGrid();

        if (targetTransform == null) return;

        lastTargetPos = targetTransform.position;

        MoveThroughPathBetween(transform.position, targetTransform.position);

        if (TargetBehind(targetTransform.position))
        {
            Flip();
        }
    }

    private void OnAttack()
    {
        if (!TargetDetectedOnArea(areaRadius)) 
        {
            ToSearching();
            return;
        }

        if (Vector3.Distance(transform.position, targetTransform.position) >= attackMinDistance && TargetDetectedOnArea(areaRadius)) 
        {
            ToChasing();
            return;
        }

        attackTimer -= Time.fixedDeltaTime;

        lastTargetPos = targetTransform.position;

        localMovement = Vector3.Lerp(localMovement, Vector3.zero, externalMovementDecay * Time.fixedDeltaTime);

        if (attackTimer <= 0)
        {
            Debug.Log("Projectile shot!");
            GameObject newProjectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            newProjectile.GetComponent<EnemyProjectileController>().SetMovement(projectileSpeed, targetTransform.position);

            attackTimer = attackCooldown;
        }

        if (Vector3.Distance(transform.position, targetTransform.position) <= personalBubbleRadius)
        {
            localMovement = (transform.position - targetTransform.position).normalized * (speed / 2f);
        }

        /*if (Vector3.Distance(transform.position, targetTransform.position) <= transitableAreaRadius)
        {
            if (PlayerAbove())
            {
                localMovement += Vector3.up * 0.5f;
            }
        }*/

        if (TargetBehind(targetTransform.position))
        {
            Flip();
        }
    }

    private void OnRamming()
    {
        timer += Time.fixedDeltaTime;

        Vector3 targetDirection = (targetTransform.position - transform.position).normalized;

        if (timer >= rammingDelay)
        {
            rammingDirection = Vector3.Lerp(rammingDirection, targetDirection, rammignTurnSpeed * Time.fixedDeltaTime).normalized;

            //rb.linearVelocity = rammingDirection * rammingSpeed;
            localMovement = rammingDirection * rammingSpeed;
        }
        else
        {
            rammingDirection = (targetTransform.position - transform.position).normalized;
        }
    }

    private void OnSearching()
    {
        if (TargetDetectedOnArea(areaRadius)) 
        {
            ToChasing();
            return;
        } 

        if (Vector3.Distance(transform.position, lastTargetPos) > 0.1f)
        {
            UpdateNodeGrid();

            MoveThroughPathBetween(transform.position, lastTargetPos);
        } 
        else if (Vector3.Distance(transform.position, lastTargetPos) <= 0.1f)
        {
            if (!lostTarget)
            {
                lostTarget = true;
                ResetTimer();
            }

            timer += Time.fixedDeltaTime;

            //rb.linearVelocity = Vector3.zero;
            localMovement = Vector3.zero;

            if (timer >= timeToStopSearching)
            {
                ToRetreat();
                lostTarget = false;
            }
        }
    }

    private void OnRetreat()
    {
        if (TargetDetectedOnArea(areaRadius)) 
        {
            ToChasing();
            return;
        }
        
        UpdateNodeGrid();
        MoveThroughPathBetween(transform.position, spawnPosition);

        if (TargetBehind(spawnPosition)) Flip();

        if (Vector3.Distance(transform.position, spawnPosition) <= 0.1f)
        {
            ToPatrol();
        }
    }

    #endregion

    #region STATE TRANSITION


    private IEnumerator WaitToMove()
    {
        decidingDirection = true;

        yield return new WaitForSeconds(Random.Range(minMovementDelay, maxMovementDelay));

        DecideMovementPoint();
        decidingDirection = false;
    }

    private void ToPatrol()
    {
        ResetTimer();

        nodes.Clear();

        //rb.linearVelocity = Vector3.zero;
        localMovement = Vector3.zero;
        state = ENEMY_STATE.PATROL;
    }

    private void ToChasing()
    {
        ResetTimer();
        state = ENEMY_STATE.CHASING;
    }

    private void ToAttack()
    {
        ResetTimer();
        state = ENEMY_STATE.ATTACK;
    }

    private void ToRamming()
    {
        ResetTimer();
        state = ENEMY_STATE.RAMMING;
    }

    private void ToSearching()
    {
        ResetTimer();
        state = ENEMY_STATE.SEARCHING;
    }

    private void ToRetreat()
    {
        ResetTimer();
        state = ENEMY_STATE.RETREAT;
    }

    private void ToDead()
    {
        isDead = true;
        state = ENEMY_STATE.DEAD;
        gameObject.SetActive(false);
    }

    #endregion

    #region STATE CONDITION

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Limpia los objetivos del ataque anterior
            hitTargets.Clear();

            CombatDamage damage = new CombatDamage(state == ENEMY_STATE.RAMMING ? rammingDamage : contactDamage, characterStats, false);
            CombatManager.MakeDamage(transform, collision, hitTargets, damage, characterStats.Team);
        }

        if (state == ENEMY_STATE.RAMMING)
        {
            characterStats.Death();
        }
    }

    // Devuelve si ha detectado al objetivo dentro de un area determinada
    private bool TargetDetectedOnArea(float radius)
    {
        Collider2D[] detectionArea = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D collider in detectionArea)
        {
            if (collider.CompareTag("Player") && !ObstacleBlocksTarget(collider.transform.position, areaRadius))
            {
                lastTargetPos = collider.transform.position;
                targetTransform = collider.transform;

                return true;
            }
        }

        return false;
    }

    private void DecideMovementPoint()
    {
        Vector3 candidatePoint = Vector3.zero;

        bool validPoint = false;

        while (!validPoint)
        {
            candidatePoint = transform.position + (Vector3)Random.insideUnitCircle * transitableAreaRadius;
            Collider2D areaCheck = Physics2D.OverlapCircle(candidatePoint, personalBubbleRadius);

            if (areaCheck == null)
            {
                validPoint = true;
            }
        }

        movementPoint = candidatePoint;
    }

    private bool PlayerAbove()
    {
        float deltaY = targetTransform.position.y - transform.position.y;

        return deltaY > 0;
    }

    private void ResetTimer()
    {
        timer = 0;
    }

    public override void ResetEnemy()
    {
        if (!canRespawn) return;

        ResetColor();
        transform.position = spawnPosition;
        transform.rotation = Quaternion.Euler(spawnRotation);
        invertedDirection = transform.rotation == Quaternion.Euler(0, 180, 0);
        state = defaultState;
        isDead = false;

        localMovement = Vector3.zero;
        externalMovement = Vector3.zero;
        characterStats.FullHeal();
        ResetTimer();
    }

    private void CheckHealthLeft()
    {
        if (characterStats.CurrentHealth <= healthToRam)
        {
            ToRamming();
        }
    }

    private void GenerateNodes()
    {
        nodes.Clear();

        for (int x = -nodeRadius; x <= nodeRadius; x++)
        {
            for (int y = -nodeRadius; y <= nodeRadius; y++)
            {
                Vector3 position = transform.position + new Vector3(x * nodeSpacing, y * nodeSpacing, transform.position.z);

                bool isBlocked = Physics2D.OverlapCircle(position, nodeCheckRadius, mapMask);

                if (isBlocked) continue;

                nodes.Add(new NavigationNode(position));
            }
        }

        ConnectNodes();

        lastGridCenter = transform.position;
    }

    private void ConnectNodes()
    {
        foreach (NavigationNode node in nodes)
        {
            node.Neighbours.Clear();

            foreach (NavigationNode otherNode in nodes)
            {
                if (node == otherNode) continue;

                float distance = Vector3.Distance(node.Position, otherNode.Position);

                if (distance > nodeSpacing * 1.5f) continue;

                Vector3 direction = (otherNode.Position - node.Position).normalized;

                RaycastHit2D hit = Physics2D.Raycast(node.Position, direction, distance, mapMask);

                if (!hit)
                {
                    node.Neighbours.Add(otherNode);
                }
            }
        }
    }

    private void UpdateNodeGrid()
    {
        if (Vector2.Distance(transform.position, lastGridCenter) > nodeSpacing)
        {
            GenerateNodes();
        }
    }

    private List<NavigationNode> FindPath(NavigationNode startNode, NavigationNode targetNode)
    {
        List<NavigationNode> openSet = new List<NavigationNode>();
        HashSet<NavigationNode> closedSet = new HashSet<NavigationNode>();

        openSet.Add(startNode);
        startNode.SetGCost(0f);
        startNode.SetHCost(Vector3.Distance(startNode.Position, targetNode.Position));
        startNode.SetParent(null);

        while (openSet.Count > 0)
        {
            NavigationNode currentNode = openSet[0];

            foreach (NavigationNode node in openSet)
            {
                if (node.FinalCost() < currentNode.FinalCost() || (node.FinalCost() == currentNode.FinalCost() && node.HCost() < currentNode.HCost()))
                {
                    currentNode = node;
                }
            }

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            foreach (NavigationNode neighbour in currentNode.Neighbours)
            {
                if (closedSet.Contains(neighbour)) continue;

                float tentativeGCost = currentNode.GCost() + Vector3.Distance(currentNode.Position, neighbour.Position);

                if (!openSet.Contains(neighbour) || tentativeGCost < neighbour.GCost())
                {
                    neighbour.SetGCost(tentativeGCost);
                    neighbour.SetHCost(Vector3.Distance(neighbour.Position, targetNode.Position));
                    neighbour.SetParent(currentNode);

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }

        return null;
    }

    private List<NavigationNode> RetracePath(NavigationNode startNode, NavigationNode endNode)
    {
        List<NavigationNode> path = new List<NavigationNode>();
        NavigationNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }
        path.Reverse();
        return path;
    }

    private NavigationNode GetClosestNode(Vector3 position)
    {
        NavigationNode closestNode = null;
        float minDistance = float.MaxValue;

        foreach (NavigationNode node in nodes)
        {
            float distance = Vector3.Distance(position, node.Position);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestNode = node;
            }
        }

        return closestNode;
    }

    private void MoveThroughPathBetween(Vector3 startPosition, Vector3 targetPosition)
    {
        if (nodes.Count == 0) return;

        timer += Time.fixedDeltaTime;

        if (timer >= pathUpdateInterval || currentPath == null)
        {
            timer = 0;

            NavigationNode startNode = GetClosestNode(startPosition);
            NavigationNode targetNode = GetClosestNode(targetPosition);

            if (startNode != null && targetNode != null)
            {
                currentPath = FindPath(startNode, targetNode);
                currentPathIndex = 0;
            }
        }

        if (currentPath != null && currentPath.Count > 0)
        {
            if (currentPathIndex >= currentPath.Count)
            {
                currentPathIndex = currentPath.Count - 1;
            }

            Vector3 targetNodePosition = currentPath[currentPathIndex].Position;

            //rb.linearVelocity = (targetNodePosition - startPosition).normalized * speed;
            localMovement = (targetNodePosition - startPosition).normalized * speed;

            if (Vector3.Distance(startPosition, targetNodePosition) < 0.4f)
            {
                currentPathIndex++;
            }
        }
        else
        {
            //rb.linearVelocity = (targetPosition - startPosition).normalized * speed;
            localMovement = (targetPosition - startPosition).normalized * speed; ;
        }
    }

    private bool DetectedWall()
    {
        Debug.DrawRay(wallCheck.position, Vector3.down, Color.red);
        return Physics2D.Raycast(wallCheck.position, Vector3.down, 1f, wallLayer);
    }

    #endregion
}
