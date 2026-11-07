using UnityEngine;

public class XpDropEnemy : MonoBehaviour
{

    [SerializeField] private XpData xp;
    CharacterStats characterStats;
	private void Awake()
	{
		characterStats = GetComponent<CharacterStats>();
	}

	private void OnEnable()
	{
		characterStats.OnDeath += DropXp;
	}

	private void OnDisable()
	{
		characterStats.OnDeath -= DropXp;
	}
	public void DropXp()
    {
		if (characterStats.Team != CombatTeam.Enemy) return;
        if (xp == null) return;

        GameObject xpObject = Instantiate(xp.xpPickUp, transform.position, Quaternion.identity);

        CollectableXp collectableXp = xpObject.GetComponent<CollectableXp>();

        if (collectableXp != null)
        {
            collectableXp.SetValueXp(xp.xp);
        }

    }
}
