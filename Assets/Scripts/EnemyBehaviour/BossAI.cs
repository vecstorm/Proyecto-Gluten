using UnityEngine;

public abstract class BossAI : EnemyAI
{
    [Header("Boss Debug:")]
    [SerializeField] protected bool bossActive = false;
    [SerializeField] protected BossAreaController bossArea;
    [SerializeField] protected GameObject areaDoors;

    protected override void InitializeBasicComponents()
    {
        EnemyManager.EnemiesOnMap.Add(this);

        defaultState = state;

        rendererColor = GetComponent<SpriteRenderer>().color;
        invertedDirection = transform.rotation == Quaternion.Euler(0, 180, 0);
        rb = GetComponent<Rigidbody2D>();
        characterStats = GetComponent<CharacterStats>();
        bossArea.SetOwnedBoss(this);
        bossArea.SetAreaDoor(areaDoors);
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
