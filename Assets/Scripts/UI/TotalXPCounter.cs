using TMPro;
using UnityEngine;

public class TotalXPCounter : MonoBehaviour
{
    [SerializeField] CharacterStats CharacterStatsInstance;

    [SerializeField] TextMeshProUGUI textTotalXP;
    int totalXPCounter;

    private void Update()
    {
        totalXPCounter = CharacterStatsInstance.currentXp;

        UpdateText();
    }

    private void UpdateText()
    {
        textTotalXP.text = "XP: " + totalXPCounter.ToString();
    }
}
