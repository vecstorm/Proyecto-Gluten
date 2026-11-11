using UnityEngine;
using UnityEngine.InputSystem; // <-- 1. Importante: Asegúrate de tener este using

public class CheckpointInteract : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private KeyCode teclaInteraccion = KeyCode.E; // (Opcional, si prefieres usar el nuevo sistema, puedes cambiarlo por Key.E)
    [SerializeField] private bool yaActivado = false;

    [Header("Efectos Visuales / Opcionales")]
    [SerializeField] private GameObject indicadorUI;

    private bool jugadorEnRango = false;

    private void Start()
    {
        if (indicadorUI != null)
            indicadorUI.SetActive(false);
    }

    private void Update()
    {
        
        if (jugadorEnRango && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ActivarCheckpoint();
        }
    }

    private void ActivarCheckpoint()
    {
        // Buscamos al jugador y curamos usando tu Singleton / CharacterStats
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            CharacterStats stats = player.GetComponent<CharacterStats>();
            RespawnPointScript respawn = player.GetComponent<RespawnPointScript>();
            if (stats != null)
            {
                // Asegúrate de tener este método en tu CharacterStats o ajusta la lógica aquí
                stats.FullHeal();
                if (respawn != null)
                {
                    respawn.respawnPointPlayer = transform.position; // Actualiza el punto de respawn
                }
            }
        }

        // 2. Ordenamos al GameManager central que resetee a los enemigos
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RespawnAllEnemies();
        }

        Debug.Log("Checkpoint activado en: " + transform.position);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorEnRango = true;
            if (indicadorUI != null)
                indicadorUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorEnRango = false;
            if (indicadorUI != null)
                indicadorUI.SetActive(false);
        }
    }
}