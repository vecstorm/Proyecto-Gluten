using UnityEngine;
// Añadimos esta línea para usar el nuevo Input System
using UnityEngine.InputSystem;

public class TesterStats : MonoBehaviour
{
    [SerializeField] private CharacterStats characterStats;

    private void Update()
    {
        if (characterStats == null)
            characterStats = GetComponent<CharacterStats>();

        if (characterStats == null || Keyboard.current == null) return;

        // Pulsar 'T' para infligir 10 de daño
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            characterStats.TakeDamage(10f);
            Debug.Log($"Daño recibido. Vida actual: {characterStats.CurrentHealth}");
        }

        // Pulsar 'H' para curar 10 de vida
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            characterStats.Heal(10f);
            Debug.Log($"Curación aplicada. Vida actual: {characterStats.CurrentHealth}");
        }

        if (Keyboard.current.fKey.wasPressedThisFrame) {
            characterStats.FullHeal();
            Debug.Log($"Te has curado a full loco {characterStats.CurrentHealth}");
        }

        // Pulsar 'Espacio' para consumir 20 de estamina
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (characterStats.UseStamina(20f))
            {
                Debug.Log($"Estamina usada. Estamina actual: {characterStats.CurrentStamina}");
            }
            else
            {
                Debug.Log("¡No hay suficiente estamina!");
            }
        }
    }
}