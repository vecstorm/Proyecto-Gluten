
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HelthUpgradeMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform healthContainer;
    [SerializeField] private Transform attackContainer;

    [Header("Prefabs")]
    [SerializeField] private Image statPointPrefab;
    [SerializeField] private Button upgradeButtonPrefab;

    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private CharacterStats characterStats;

    /*    [Header("Heart Sprites")]
        [SerializeField] private Sprite fullHeart;
        [SerializeField] private Sprite halfHeart;
        [SerializeField] private Sprite emptyHeart;*/

    private List<Image> hearts = new();

    public void Initialize(float maxHealth)
    {
        foreach (Image heart in hearts)
            Destroy(heart.gameObject);

        hearts.Clear();

        int heartCount = Mathf.CeilToInt(maxHealth / 2f);

        for (int i = 0; i < heartCount; i++)
        {
            Image newHeart = Instantiate(
                statPointPrefab,
                healthContainer,
                false
            );

            hearts.Add(newHeart);
        }
    }

  /*  public void UpdateHealth(float currentHealth)
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            float heartHealth = currentHealth - (i * 2f);

            if (heartHealth >= 2f)
                hearts[i].sprite = fullHeart;

            else if (heartHealth >= 1f)
                hearts[i].sprite = halfHeart;

            else
                hearts[i].sprite = emptyHeart;
        }
    }*/
}
