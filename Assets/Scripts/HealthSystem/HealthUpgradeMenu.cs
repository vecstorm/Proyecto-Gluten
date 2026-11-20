
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthUpgradeMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform healthContainer;
    [SerializeField] private Transform attackContainer;

    [Header("Prefabs")]
	[SerializeField] private Image healthPointPrefab;
	[SerializeField] private Image attackPointPrefab;
	[SerializeField] private Button upgradeButtonPrefab;

	private Button healthUpgradeButton;
	private Button attackUpgradeButton;

	[SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private CharacterStats characterStats;

	[SerializeField] private int initialHealthUpgradeCost = 30;
	[SerializeField] private int healthUpgradeCostIncrease = 20;

	[SerializeField] private int initialAttackUpgradeCost = 30;
	[SerializeField] private int attackUpgradeCostIncrease = 20;


	/*    [Header("Heart Sprites")]
        [SerializeField] private Sprite fullHeart;
        [SerializeField] private Sprite halfHeart;
        [SerializeField] private Sprite emptyHeart;*/

	private List<Image> hearts = new();

	private void Awake()
	{
		if (healthContainer == null)
			healthContainer = transform.Find("HealthContainer");

		if (attackContainer == null)
			attackContainer = transform.Find("AttackContainer");

		if (playerCombat == null)
			playerCombat = FindFirstObjectByType<PlayerCombat>();

		if (characterStats == null)
			characterStats = FindFirstObjectByType<CharacterStats>();

		if (healthPointPrefab == null)
			healthPointPrefab = Resources.Load<Image>("UpgradePrefabs/RanuraUpgrade");

		if (attackPointPrefab == null)
			attackPointPrefab = Resources.Load<Image>("UpgradePrefabs/RanuraUpgrade");

		if (upgradeButtonPrefab == null)
			upgradeButtonPrefab = Resources.Load<Button>("UpgradePrefabs/UpgradeButton");
	}

	public void Initialize(float maxHealth)
	{
		// Limpiar imágenes anteriores
		foreach (Transform child in healthContainer)
			Destroy(child.gameObject);

		foreach (Transform child in attackContainer)
			Destroy(child.gameObject);

		// Generar imágenes de vida y ataque
		GenerateHealthPoints(maxHealth);
		GenerateAttackPoints(playerCombat.BasicAttackDamage);

		CreateUpgradeButtons();

	}

	private void GenerateHealthPoints(float maxHealth)
	{
		int amount = Mathf.CeilToInt(maxHealth);

		for (int i = 0; i < amount; i++)
		{
			Instantiate(healthPointPrefab, healthContainer, false);
		}
	}

	private void GenerateAttackPoints(float attackDamage)
	{
		int amount = Mathf.CeilToInt(attackDamage);

		for (int i = 0; i < amount; i++)
		{
			Instantiate(attackPointPrefab, attackContainer, false);
		}
	}

	private void RefreshUpgradeButtons()
	{
		bool hasTokens = characterStats.HealthUpgradeTokens > 0;

		if (healthUpgradeButton != null)
			healthUpgradeButton.gameObject.SetActive(hasTokens);

		if (attackUpgradeButton != null)
			attackUpgradeButton.gameObject.SetActive(hasTokens);
	}


   public void UpgradeHealth()
{
    int cost = characterStats.GetHealthUpgradeCost(
        initialHealthUpgradeCost,
        healthUpgradeCostIncrease
    );

    if (characterStats.currentXp < cost)
        return;

    if (!characterStats.UseHealthUpgradeToken())
        return;

    if (!characterStats.SpendXp(cost))
        return;

    characterStats.IncreaseMaxHealth(1f);
    characterStats.RegisterHealthUpgrade();

    Instantiate(healthPointPrefab, healthContainer, false);

    RefreshUpgradeButtons();
    healthUpgradeButton.transform.SetAsLastSibling();
}

	public void UpgradeAttack()
	{
		int cost = characterStats.GetAttackUpgradeCost(
			initialAttackUpgradeCost,
			attackUpgradeCostIncrease
		);

		if (characterStats.currentXp < cost)
			return;

		if (!characterStats.UseHealthUpgradeToken())
			return;

		if (!characterStats.SpendXp(cost))
			return;

		playerCombat.IncreaseBasicAttackDamage(1f);

		characterStats.RegisterAttackUpgrade();

		Instantiate(attackPointPrefab, attackContainer, false);

		RefreshUpgradeButtons();
		attackUpgradeButton.transform.SetAsLastSibling();
	}

	private void CreateUpgradeButtons()
	{ 

        if (characterStats.HealthUpgradeTokens <= 0)
			return;


		healthUpgradeButton = Instantiate(upgradeButtonPrefab, healthContainer, false);

		attackUpgradeButton = Instantiate(upgradeButtonPrefab, attackContainer, false);

		healthUpgradeButton.onClick.AddListener(UpgradeHealth);
		attackUpgradeButton.onClick.AddListener(UpgradeAttack);
    }
}
