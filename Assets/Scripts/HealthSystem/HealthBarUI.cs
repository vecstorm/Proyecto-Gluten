using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private Slider healthSlider;

    private void Start()
    {
        if (characterStats != null && healthSlider != null)
        {
            // Configuramos el máximo y ponemos la barra llena al iniciar la escena
            healthSlider.maxValue = characterStats.MaxHealth;
            healthSlider.value = characterStats.CurrentHealth;
        }
    }

    private void OnEnable()
    {
        if (characterStats != null)
        {
            characterStats.OnHealthChanged += ActualizarBarra;
        }
    }

    private void OnDisable()
    {
        if (characterStats != null)
        {
            characterStats.OnHealthChanged -= ActualizarBarra;
        }
    }

    private void ActualizarBarra(float vidaActual, float vidaMaxima)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = vidaMaxima;
            healthSlider.value = vidaActual;
        }
    }
}