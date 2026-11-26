using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenDoorManager : MonoBehaviour
{
    [SerializeField] public KeyData idDoor;
    [SerializeField] public KeyData salida;
    [SerializeField] private string fallbackDoorId = "Door_01";

    private string GetDoorKey()
    {
        if (idDoor != null)
        {
            return $"Door_{idDoor.Id}";
        }

        return fallbackDoorId;
    }

    private void Start()
    {
        if (GameSave.IsUnlocked(GetDoorKey()))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Mira player");
        if (!collision.CompareTag("Player")) return;

        KeyDoorManager keyManager = collision.GetComponent<KeyDoorManager>();

        Debug.Log("Casi tiene llave");

        if (keyManager == null) return;
        if (keyManager.HasKey(idDoor))
        {
            Debug.Log("tiene llave");
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        GameSave.MarkUnlocked(GetDoorKey());

        if (idDoor == salida)
        {
            SceneManager.LoadScene("SceneMenus");
        }

        Debug.Log("Puerta abierta");

        Destroy(gameObject);
    }
}