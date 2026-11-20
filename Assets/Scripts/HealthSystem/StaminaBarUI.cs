using UnityEngine;
using UnityEngine.UI;

public class StaminaBarUI : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El componente Slider de la barra de estamina.")]
    [SerializeField] private Slider staminaSlider;

    // Referencia interna guardada automáticamente
    private CharacterStats characterStats;

    private void Awake()
    {
        // Buscamos al jugador por su etiqueta al iniciar la escena
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            characterStats = player.GetComponent<CharacterStats>();
        }

        if (characterStats == null)
        {
            Debug.LogError("StaminaBarUI no pudo encontrar un CharacterStats con la etiqueta 'Player'.", this);
        }
    }

    private void Start()
    {
        if (characterStats != null && staminaSlider != null)
        {
            // Inicializa la barra con los valores máximos al arrancar
            staminaSlider.maxValue = characterStats.MaxStamina;
            staminaSlider.value = characterStats.CurrentStamina;
        }
    }

    private void OnEnable()
    {
        // Usamos la referencia guardada de forma segura
        if (characterStats != null)
        {
            characterStats.OnStaminaChanged += ActualizarBarra;
        }
    }

    private void OnDisable()
    {
        // Nos desuscribimos usando exactamente la misma referencia para evitar fugas de memoria
        if (characterStats != null)
        {
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