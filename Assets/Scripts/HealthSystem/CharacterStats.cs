using UnityEngine;
using System;

public class CharacterStats : MonoBehaviour
{
    [Header("Combate")]
    [SerializeField] private CombatTeam team = CombatTeam.Neutral;

    public CombatTeam Team => team;

    IDamageable damageable;

    [Header("Configuración de Vida")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

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

    private bool isPlayer;

    public int currentXp = 0;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        isPlayer = gameObject.CompareTag("Player");
    }

    private void Update()
    {
        if (isPlayer)
        {
            RegenerateStamina();
        }
    }

    /// <summary>
    /// Recibe daño a través del contrato común de combate.
    /// </summary>
    /// <param name="damage">Daño a recibir</param>
    /// <returns>Devuelve false si el impacto no se ha aplicado.</returns>
    public bool ReceiveDamage(CombatDamage damage)
    {
        if (currentHealth <= 0f || damage.Amount <= 0f)
            return false;

        // Evita daño entre miembros del mismo equipo. Los objetos neutrales pueden recibir daño de cualquiera
        if (team != CombatTeam.Neutral && team == damage.SourceTeam)
        {
            return false;
        }

        TakeDamage(damage.Amount);
        return true;
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"{gameObject.name} took {amount} damage. Current Health: {currentHealth}/{maxHealth}");

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
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
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

    // Restablece la estamina al máximo al reaparecer
    public void ResetStaminaOnRespawn()
    {
        currentStamina = maxStamina;
        Debug.Log(currentStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    public void AddXp(int x)
    {
        currentXp += x;

        Debug.Log(currentXp);
    }
}