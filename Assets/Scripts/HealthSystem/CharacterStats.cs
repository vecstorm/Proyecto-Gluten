using UnityEngine;
using System;

public class CharacterStats : MonoBehaviour, IDamageable, IStunnable
{
    [Header("Combate")]
    [SerializeField] private CombatTeam team = CombatTeam.Neutral;

    public CombatTeam Team => team;

    private Collider2D ownerCollider;
    private Collider2D lastDamageCollider;
    private CombatDamage lastCombatDamage;
    private bool couldTakeDamageLastFrame = true;

    [Header("Configuraci�n de Vida")]
    [SerializeField] private float maxHealth = 10;
    [SerializeField] private float currentHealth;
    [SerializeField] private HealthUpgradeMenu healthUI;


    [Header("Configuraci�n de Estamina / Magia")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 15f;
    private float currentStamina;

    [SerializeField] private int healthUpgradeTokens = 0;

    Rigidbody2D rb;

    [SerializeField] private bool canTakeDamage = true;

    // Eventos de C# para notificar cambios de forma desacoplada
    public event Action<float, float> OnHealthChanged;   // (Vida Actual, Vida M�xima)
    public event Action<float, float> OnStaminaChanged;  // (Estamina Actual, Estamina M�xima)
    public event Action OnDeath; // Se dispara al llegar a 0 de vida
    public event Action OnDamageTaken;
    public event Action<CombatDamage> OnDamageBlocked;
    public event Action OnStunApplied;
    public event Action OnParrySuccess;

    public float CurrentHealth => currentHealth;
    public float CurrentStamina => currentStamina;
    public float MaxHealth => maxHealth;
    public float MaxStamina => maxStamina;

    public int HealthUpgradeTokens => healthUpgradeTokens;


    private bool isPlayer;
    
    /// <summary>
    /// Temporizador para el parry.
    /// </summary>
    private float parryTimer;

    /// <summary>
    /// Temporizador para el tiempo entre "parrys".
    /// </summary>
    private float parryCooldownTimer;

    /// <summary>
    /// Indica si el jugador se encuentra actualmente dentro de la ventana temporal de parry.
    /// </summary>
    private bool isParrying;

    /// <summary>
    /// Direcci�n en la que se inici� el parry, si el jugador cambia de direcci�n, el parry se cancela.
    /// </summary>
    private int parryFacingDirection;

    [Header("Parry")]

    [Tooltip("Duraci�n de la ventana durante la que el jugador puede realizar un parry")]
    [SerializeField, Min(0f)] private float parryWindow = 0.3f;

    [Tooltip("Tiempo que debe pasar antes de poder iniciar otro parry")]
    [SerializeField, Min(0f)] private float parryCooldown = 1f;

    /// <summary>
    /// Indica si el jugador puede iniciar otro parry.
    /// </summary>
    public bool CanParry => parryCooldownTimer <= 0f;

    /// <summary>
    /// Indica si el jugador est� realizando un parry.
    /// </summary>
    public bool IsParrying => isParrying;

    private int facingDirection = 1;

    public int currentXp = 0;
    [SerializeField] private float knockbackForce = 3f;

    [Header ("CosteMejoras")]
	[SerializeField] private int healthUpgradeCount = 0;
	[SerializeField] private int attackUpgradeCount = 0;
	[SerializeField] private int initialHealthUpgradeCost = 30;
	[SerializeField] private int healthUpgradeCostIncrease = 20;

	[SerializeField] private int initialAttackUpgradeCost = 30;
	[SerializeField] private int attackUpgradeCostIncrease = 20;

	public int HealthUpgradeCount => healthUpgradeCount;
	public int AttackUpgradeCount => attackUpgradeCount;

    private Vector3 lastDamagePosition;

    public Vector3 LastDamagePosition => lastDamagePosition;

    private void Awake()
    {
        ownerCollider = GetComponent<Collider2D>();

        currentHealth = maxHealth;
        currentStamina = maxStamina;
        isPlayer = gameObject.CompareTag("Player");
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (!isPlayer) return;
        healthUI.Initialize(maxHealth);
        //healthUI.UpdateHealth(currentHealth);
    }
    private void Update()
    {
        if (isPlayer)
        {
            RegenerateStamina();
        }

        // Duraci�n de la ventana de parry
        parryTimer = Mathf.Max(0f, parryTimer - Time.deltaTime);

        // Cooldown entre intentos de parry
        parryCooldownTimer = Mathf.Max(0f, parryCooldownTimer - Time.deltaTime);

        // Finaliza autom�ticamente el parry cuando termina su ventana temporal
        if (isParrying && parryTimer <= 0f)
            isParrying = false;

        if (canTakeDamage && !couldTakeDamageLastFrame)
        {
            if (lastDamageCollider != null && lastDamageCollider.enabled && lastDamageCollider.gameObject.activeSelf)
            {
                CheckIfStillColliding();
            }
        }

        couldTakeDamageLastFrame = canTakeDamage;
    }

    public void AddHealthUpgradeToken()
    {
        healthUpgradeTokens++;

	}

    public bool UseHealthUpgradeToken()
    {
        if (healthUpgradeTokens <= 0)
            return false;

        healthUpgradeTokens--;
        //IncreaseMaxHealth(1f);

        return true;
    }

	public void IncreaseMaxHealth(float amount)
	{
		maxHealth += amount;
	}

    public int GetHealthUpgradeCost(int initialCost, int costIncrease)
    {
        return initialCost + (healthUpgradeCount * costIncrease);
    }
    public int GetAttackUpgradeCost(int initialCost, int costIncrease)
    {
        return initialCost + (attackUpgradeCount * costIncrease);
    }

	public bool SpendXp(int amount)
	{
		if (currentXp < amount)
			return false;

		currentXp -= amount;
		return true;
	}

	public void RegisterHealthUpgrade()
    {
		healthUpgradeCount++;
    }

    public void RegisterAttackUpgrade()
    {
        attackUpgradeCount++;
    }
	/// <summary>
	/// Recibe da�o a trav�s del contrato com�n de combate.
	/// </summary>
	/// <param name="damage">Da�o a recibir</param>
	/// <returns>Devuelve false si el impacto no se ha aplicado.</returns>
	public bool ReceiveDamage(CombatDamage damage)
    {
        if (currentHealth <= 0f || damage.Amount <= 0f)
            return false;

        // Evita da�o entre miembros del mismo equipo. Los objetos neutrales pueden recibir da�o de cualquiera
        if (team != CombatTeam.Neutral && team == damage.SourceTeam)
        {
            return false;
        }

        if (isParrying && parryTimer > 0f && damage.IsParryable && IsAttackInFront(damage))
        {
            ResolveParry(damage);
            return false;
        }

        Collider2D collider = damage.Source.GetComponent<Collider2D>();

        if (collider != null)
        {
            lastDamageCollider = collider;
            lastCombatDamage = damage;
        }

        if (!canTakeDamage)
        {
            OnDamageBlocked?.Invoke(damage);
            return false;
        }

        lastDamagePosition = damage.HitPoint;
        TakeDamage(damage.Amount);
        AddKnockback(damage.HitPoint);
        return true;
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"{gameObject.name} took {amount} damage. Current Health: {currentHealth}/{maxHealth}");

        OnDamageTaken?.Invoke();

        if (currentHealth <= 0f)
        {
            OnDeath?.Invoke();
        }
    }

    /// <summary>
    /// Comprueba si el ataque recibido procede del lado hacia el que mira el jugador.
    /// No tiene l�mite de distancia: �nicamente comprueba la direcci�n.
    /// </summary>
    /// <param name="incomingDamage">Informaci�n del ataque recibido.</param>
    /// <returns>True si el ataque procede de delante del jugador.</returns>
    private bool IsAttackInFront(CombatDamage incomingDamage)
    {
        // Utiliza el punto de impacto del ataque para determinar su procedencia.
        Vector2 attackPosition = incomingDamage.HitPoint;

        Vector2 directionToAttack = attackPosition - (Vector2)transform.position;

        // Si el punto de impacto coincide pr�cticamente con la posici�n
        // del jugador, utiliza la posici�n del atacante como referencia.
        if (directionToAttack.sqrMagnitude < 0.001f &&
            incomingDamage.Source != null)
        {
            directionToAttack =
                (Vector2)incomingDamage.Source.position - (Vector2)transform.position;
        }

        // Si no se puede determinar la procedencia del ataque,
        // no se considera v�lido para realizar un parry.
        if (directionToAttack.sqrMagnitude < 0.001f)
            return false;

        // El producto escalar permite comprobar si el ataque procede
        // del mismo semiplano hacia el que mira el jugador.
        // Un resultado positivo indica que est� delante.
        Vector2 facing = Vector2.right * facingDirection;

        return Vector2.Dot(facing, directionToAttack.normalized) > 0f;
    }

    /// <summary>
    /// Intenta iniciar una ventana de parry.
    /// El parry solo puede bloquear ataques parryables que lleguen desde delante.
    /// </summary>
    /// <returns>True si el parry se ha iniciado correctamente.</returns>
    public bool BeginParry()
    {
        // Comprueba que el cooldown ha terminado y que el jugador sigue vivo.
        if (!CanParry || currentHealth <= 0f)
            return false;

        isParrying = true;

        // Guarda la direcci�n inicial para impedir que el jugador
        // pueda girar y cubrir ambos lados con el mismo parry.
        parryFacingDirection = facingDirection;

        // Inicia la ventana temporal y su cooldown.
        parryTimer = parryWindow;
        parryCooldownTimer = parryCooldown;

        return true;
    }

    /// <summary>
    /// Cancela la ventana de parry sin modificar su cooldown.
    /// </summary>
    public void EndParry()
    {
        isParrying = false;
        parryTimer = 0f;
    }

    /// <summary>
    /// Resuelve las consecuencias de un parry exitoso seg�n el tipo de ataque.
    /// </summary>
    /// <param name="incomingDamage">Informaci�n del ataque neutralizado.</param>
    private void ResolveParry(CombatDamage incomingDamage)
    {
        // Consume inmediatamente la ventana para impedir que un mismo parry
        // neutralice varios ataques.
        EndParry();

        // Notifica que el parry se ha realizado correctamente
        OnParrySuccess?.Invoke();

        // Parry contra proyectiles:
        // intenta localizar un componente que implemente IReflectableProjectile
        // y le solicita cambiar de propietario y equipo.
        if (incomingDamage.AttackType == CombatAttackType.Projectile &&
            incomingDamage.Source != null)
        {
            IReflectableProjectile projectile =
                FindInterface<IReflectableProjectile>(incomingDamage.Source);

            if (projectile != null)
            {
                // Calcula la direcci�n de reflexi�n utilizando la posici�n
                // del proyectil respecto al jugador.
                Vector2 direction = incomingDamage.Source.position - transform.position;

                // Si ambas posiciones coinciden, utiliza una direcci�n alternativa
                // para evitar enviar un vector nulo al proyectil.
                direction = direction.sqrMagnitude > 0.001f
                    ? direction.normalized
                    : Vector2.right * -facingDirection;

                projectile.Reflect(transform, CombatTeam.Player, direction);
            }
        }
        // Parry contra ataques cuerpo a cuerpo:
        // intenta localizar un componente que implemente IStunnable
        // y le aplica la duraci�n de aturdimiento configurada.
        else if (incomingDamage.AttackType == CombatAttackType.Melee &&
                 incomingDamage.Source != null)
        {
            IStunnable stunnable = FindInterface<IStunnable>(incomingDamage.Source);

            if (stunnable != null)
                stunnable.ApplyStun();
        }
    }

    /// <summary>
    /// Actualiza la direcci�n hacia la que mira el jugador.
    /// Si cambia de direcci�n durante un parry, lo cancela inmediatamente.
    /// </summary>
    /// <param name="direction">Direcci�n del jugador: 1 para derecha y -1 para izquierda.</param>
    public void SetFacingDirection(int direction)
    {
        if (direction == 0)
            return;

        // Normaliza cualquier valor recibido a 1 o -1
        int newDirection = direction > 0 ? 1 : -1;

        // No es necesario hacer nada si la direcci�n no ha cambiado
        if (newDirection == facingDirection)
            return;

        facingDirection = newDirection;

        // El parry solo protege hacia la direcci�n en la que comenz�
        // Cambiar de direcci�n cancela la ventana para evitar cubrir ambos lados
        if (isParrying && facingDirection != parryFacingDirection)
            EndParry();
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

    // Restablece la estamina al m�ximo al reaparecer
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

    /// <summary>
    /// Busca un componente que implemente una interfaz concreta
    /// en el objeto atacante recibido o en alguno de sus padres.
    /// </summary>
    /// <typeparam name="T">Interfaz que se desea encontrar.</typeparam>
    /// <param name="source">Transform del objeto atacante.</param>
    /// <returns>El componente encontrado o null si no existe.</returns>
    private static T FindInterface<T>(Transform source)
        where T : class
    {
        MonoBehaviour[] behaviours = source.GetComponentsInParent<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is T component)
                return component;
        }

        return null;
    }

    public bool GetCanTakeDamage()
    {
        return canTakeDamage;
    }

    public void SetCanTakeDamage(bool value)
    {
        canTakeDamage = value;
    }

    public void AddKnockback(Vector3 damageSourcePosition)
    {
        Vector3 direction = ((damageSourcePosition + -Vector3.up * 0.5f) - transform.position).normalized;

        rb.AddForce(-direction * knockbackForce, ForceMode2D.Impulse);
    }

    public void ApplyStun()
    {
        OnStunApplied?.Invoke();
    }

    private void CheckIfStillColliding()
    {
        if (lastDamageCollider == null || ownerCollider == null)
        {
            lastDamageCollider = null;
            return;
        }

        if (!lastDamageCollider.gameObject.activeSelf || !lastDamageCollider.enabled)
        {
            lastDamageCollider = null;
            return;
        }

        ColliderDistance2D distance = ownerCollider.Distance(lastDamageCollider);

        if (distance.isOverlapped)
        {
            Collider2D hazardToApply = lastDamageCollider;
            CombatDamage damageToApply = lastCombatDamage;

            lastDamageCollider = null;
            lastCombatDamage = default;

            ReceiveDamage(damageToApply);
        }
        else
        {
            lastDamageCollider = null;
            lastCombatDamage = default;
        }
    }
}