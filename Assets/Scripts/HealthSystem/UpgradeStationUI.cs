using UnityEngine;

public class UpgradeStationUI : MonoBehaviour
{
    private bool playerNearby = false;
    [SerializeField] PlayerInputs playerInputs;

    [SerializeField] public GameObject healUpgradePanel;
    [SerializeField] public GameObject upgradePanel;
    [SerializeField] private HealthUpgradeMenu healthUpgradeMenu;
    [SerializeField] private CharacterStats characterStats;

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
