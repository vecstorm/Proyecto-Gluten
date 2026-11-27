using UnityEngine;
using UnityEngine.InputSystem; // <-- 1. Importante: Asegúrate de tener este using

public class CheckpointInteract : MonoBehaviour
{

    [Header("Efectos Visuales / Opcionales")]
    [SerializeField] private GameObject indicadorUI;

    private bool jugadorEnRango = false;

    private void Start()
    {
        if (indicadorUI != null)
            indicadorUI.SetActive(false);
    }

    public void ActivarCheckpoint()
    {
        // Buscamos al jugador y curamos usando tu Singleton / CharacterStats
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            CharacterStats stats = player.GetComponent<CharacterStats>();
            RespawnPointScript respawn = player.GetComponent<RespawnPointScript>();
            KeyDoorManager keyDoorManager = player.GetComponent<KeyDoorManager>();
            if (stats != null)
            {
                // Asegúrate de tener este método en tu CharacterStats o ajusta la lógica aquí
                stats.FullHeal();
                stats.SaveStats(); // Guarda el estado del jugador
                
                if (respawn != null)
                {
                    respawn.respawnPointPlayer = transform.position; // Actualiza el punto de respawn
                    GameSave.data.playerPosition = transform.position; // Guarda el punto de respawn en GameSave
                }
                if (keyDoorManager != null)
                {
                    keyDoorManager.DumpKeysToSave(GameSave.data); // Guarda las llaves en GameSave
                }
                SkillUnlockManager skillManager = FindFirstObjectByType<SkillUnlockManager>();

                if (skillManager != null)
                {
                    skillManager.DumpSkillsToSave(GameSave.data);
                }

                GameSave.Save();
            }
        }

        EnemyManager.ResetEnemies();

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