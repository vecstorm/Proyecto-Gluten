
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HelthUpgradeMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform healthContainer;
    [SerializeField] private Image heartPrefab;

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
                heartPrefab,
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
