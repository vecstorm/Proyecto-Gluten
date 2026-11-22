using UnityEngine;
using UnityEngine.UI;

public class UpgradeStationUI : MonoBehaviour
{
    private bool playerNearby = false;
    [SerializeField] PlayerInputs playerInputs;

    [SerializeField] public GameObject healUpgradePanel;
    [SerializeField] public GameObject upgradePanel;
    [SerializeField] private HealthUpgradeMenu healthUpgradeMenu;
    [SerializeField] private CharacterStats characterStats;

    [SerializeField] private Button closeUpgradeButton;
    [SerializeField] private Button closeHealButton;
    [SerializeField] private Button openUpgradeButton;

	private void Awake()
	{
		if (playerInputs == null)
			playerInputs = FindFirstObjectByType<PlayerInputs>();

		if (healthUpgradeMenu == null)
			healthUpgradeMenu = FindFirstObjectByType<HealthUpgradeMenu>();

		if (characterStats == null)
			characterStats = FindFirstObjectByType<CharacterStats>();

		GameObject canvasObject = GameObject.Find("MejoraCanvas");

		if (canvasObject != null)
		{
			if (healUpgradePanel == null)
				healUpgradePanel = canvasObject.transform.Find("HealPanel")?.gameObject;

			if (upgradePanel == null)
				upgradePanel = canvasObject.transform.Find("UpgradePanel")?.gameObject;

            if (closeUpgradeButton == null)
                closeUpgradeButton = canvasObject.transform.Find("UpgradePanel/CloseButton")?.GetComponent<Button>();

            if (closeHealButton == null)
                closeHealButton = canvasObject.transform.Find("HealPanel/CloseButton")?.GetComponent<Button>();

            if (openUpgradeButton == null)
                openUpgradeButton = canvasObject.transform.Find("HealPanel/UpgradeButton")?.GetComponent<Button>();

            if (closeUpgradeButton != null)
                closeUpgradeButton.onClick.AddListener(CloseUpgradePanel);

            if (closeHealButton != null)
                closeHealButton.onClick.AddListener(CloseHealPanel);
            
            if (openUpgradeButton != null)
                openUpgradeButton.onClick.AddListener(OpenUpgradePanel);

		}

	}
	private void Start()
    {
        SetPanel(false, healUpgradePanel);
        SetPanel(false, upgradePanel);

    }


    private void Update()
    {
        if (playerNearby && playerInputs.ActionPressed)
        {
            playerInputs.ConsumeActionPressed();
            Debug.Log("Has interactuado con la estación");
            SetPanel(true, healUpgradePanel);


        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
           
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            Debug.Log("Te has alejado dse la estación");
            SetPanel(false, healUpgradePanel);
            SetPanel(false, upgradePanel);
        }
    }

    public void SetPanel(bool setPanel, GameObject panel)
    {
        panel.SetActive(setPanel);
    }

    public void CloseHealPanel()
    {
        healUpgradePanel.SetActive(false);
        upgradePanel.SetActive(false);

    }

    public void CloseUpgradePanel()
    {
        upgradePanel.SetActive(false);
        healUpgradePanel.SetActive(true);

    }

    public void OpenUpgradePanel()
    {
        upgradePanel.SetActive(true);
        healthUpgradeMenu.Initialize(characterStats.MaxHealth);
        healUpgradePanel.SetActive(false);
    }

}