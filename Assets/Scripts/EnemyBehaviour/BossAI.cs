using UnityEngine;

public abstract class BossAI : EnemyAI
{
    [SerializeField] protected bool bossActive = false;
    [SerializeField] protected BossAreaController bossArea;
    [SerializeField] protected GameObject areaDoors;

    [Header("Ground Detection:")]
    [SerializeField] protected Transform groundDetection;
    [SerializeField] protected LayerMask groundLayer;

    protected override void InitializeBasicComponents()
    {
        EnemyManager.EnemiesOnMap.Add(this);

        rendererColor = GetComponent<SpriteRenderer>().color;
        invertedDirection = transform.rotation == Quaternion.Euler(0, 180, 0);
        rb = GetComponent<Rigidbody2D>();
        characterStats = GetComponent<CharacterStats>();
        bossArea.SetOwnedBoss(this);
        bossArea.SetAreaDoor(areaDoors);
    }

    protected bool GroundBeneath()
    {
        if (isAerial) return true;

        Debug.DrawRay(groundDetection.position, Vector3.down, Color.red);
        return Physics2D.Raycast(groundDetection.position, Vector3.down, 1f, groundLayer);
    }

    public void BossActive(bool value)
    {
        bossActive = value;
    }

    public bool IsDead()
    {
        return isDead;
    }
}
