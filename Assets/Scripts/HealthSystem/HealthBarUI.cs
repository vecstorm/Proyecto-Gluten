using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    private CharacterStats characterStats; // Referencia interna guardada

    private void Awake()
    {
        // Buscamos al jugador al arrancar el script
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            characterStats = player.GetComponent<CharacterStats>();
        }

        if (characterStats == null)
        {
            Debug.LogError("HealthBarUI no pudo encontrar un CharacterStats con la etiqueta 'Player'.", this);
        }
    }

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
        // Usamos la referencia guardada de forma segura
        if (characterStats != null)
        {
            characterStats.OnHealthChanged += ActualizarBarra;
        }
    }

    private void OnDisable()
    {
        // Nos desuscribimos usando exactamente la misma referencia para evitar fugas
        if (characterStats != null)
        {
            characterStats.OnHealthChanged -= ActualizarBarra;
        }
    }

    private void ActualizarBarra(float vidaActual, float vidaMaxima)
    {
        if (healthSlider != null)
        {
            Debug.Log($"Actualizando barra de vida: Actual = {vidaActual}, Máxima = {vidaMaxima}");
            healthSlider.maxValue = vidaMaxima;
            healthSlider.value = vidaActual;
        }
    }
}