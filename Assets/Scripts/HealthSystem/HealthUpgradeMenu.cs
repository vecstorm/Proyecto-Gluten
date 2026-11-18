
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


    /*    [Header("Heart Sprites")]
        [SerializeField] private Sprite fullHeart;
        [SerializeField] private Sprite halfHeart;
        [SerializeField] private Sprite emptyHeart;*/

    private List<Image> hearts = new();

	public void Initialize(float maxHealth)
	{
		Debug.Log("Initialize ejecutado. Vida máxima: " + maxHealth);

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
		if (!characterStats.UseHealthUpgradeToken())
			return;

		characterStats.IncreaseMaxHealth(1f);

		Image newPoint = Instantiate(
		healthPointPrefab,
		healthContainer,
		false
		);
        RefreshUpgradeButtons();
        healthUpgradeButton.transform.SetAsLastSibling();
    }

	public void UpgradeAttack()
	{
		if (!characterStats.UseHealthUpgradeToken())
			return;

		playerCombat.IncreaseBasicAttackDamage(1f);

		Instantiate(
			attackPointPrefab,
			attackContainer,
			false
		);
        RefreshUpgradeButtons();
        attackUpgradeButton.transform.SetAsLastSibling();
    }

	private void CreateUpgradeButtons()
	{
        Debug.Log("Objeto consultado: " + characterStats.gameObject.name);
        Debug.Log("ID: " + characterStats.GetInstanceID()
            + " | Fichas: " + characterStats.HealthUpgradeTokens);

        if (characterStats.HealthUpgradeTokens <= 0)
			return;


		healthUpgradeButton = Instantiate(
			upgradeButtonPrefab,
			healthContainer,
			false
		);

		attackUpgradeButton = Instantiate(
			upgradeButtonPrefab,
			attackContainer,
			false
		);

		healthUpgradeButton.onClick.AddListener(UpgradeHealth);
		attackUpgradeButton.onClick.AddListener(UpgradeAttack);

        Debug.Log("Menú consulta: " + characterStats.gameObject.name +
          " | ID: " + characterStats.GetInstanceID() +
          " | Total: " + characterStats.HealthUpgradeTokens);
    }
}
