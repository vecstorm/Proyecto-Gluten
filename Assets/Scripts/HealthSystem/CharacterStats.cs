using UnityEngine;
using System;

public class CharacterStats : MonoBehaviour
{
    [Header("Configuración de Vida")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Configuración de Estamina / Magia")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 15f;
    private float currentStamina;

    // Eventos de C# para notificar cambios de forma desacoplada
    public event Action<float, float> OnHealthChanged;   // (Vida Actual, Vida Máxima)
    public event Action<float, float> OnStaminaChanged;  // (Estamina Actual, Estamina Máxima)
    public event Action OnDeath;                         // Se dispara al llegar a 0 de vida

    public float CurrentHealth => currentHealth;
    public float CurrentStamina => currentStamina;
    public float MaxHealth => maxHealth;
    public float MaxStamina => maxStamina;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }

    private void Update()
    {
        RegenerateStamina();
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            OnDeath?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void FullHeal()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Death() {
        currentHealth = 0f;
        currentStamina = 0f;
        OnDeath?.Invoke();
    }

    public bool UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            return true;
        }
        return false;
    }

    private void RegenerateStamina()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }
    }
}