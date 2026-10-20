using UnityEngine;
using UnityEngine.UI;

public class StaminaBarUI : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastra aquí el objeto que tenga el script CharacterStats (Jugador).")]
    [SerializeField] private CharacterStats characterStats;

    [Tooltip("El componente Slider de la barra de estamina.")]
    [SerializeField] private Slider staminaSlider;

    private void Start()
    {
        if (characterStats != null && staminaSlider != null)
        {
            // Inicializa la barra llena con los valores máximos al arrancar
            staminaSlider.maxValue = characterStats.MaxStamina;
            staminaSlider.value = characterStats.CurrentStamina;
        }
    }

    private void OnEnable()
    {
        if (characterStats != null)
        {
            // Nos suscribimos al evento de estamina
            characterStats.OnStaminaChanged += ActualizarBarra;
        }
    }

    private void OnDisable()
    {
        if (characterStats != null)
        {
            // Desuscripción obligatoria para evitar fugas de memoria o errores
            characterStats.OnStaminaChanged -= ActualizarBarra;
        }
    }

    private void ActualizarBarra(float estaminaActual, float estaminaMaxima)
    {
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = estaminaMaxima;
            staminaSlider.value = estaminaActual;
        }
    }
}