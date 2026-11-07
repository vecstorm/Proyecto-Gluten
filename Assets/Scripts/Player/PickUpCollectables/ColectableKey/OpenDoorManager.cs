using UnityEngine;

public class OpenDoorManager : MonoBehaviour
{
    [SerializeField] public KeyData idDoor;

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
        Debug.Log("Puerta abierta");

        Destroy(gameObject);
    }
}
