using UnityEngine;

public class KeyPickUp : MonoBehaviour
{
    [SerializeField] KeyData key;

    private void Start()
    {
        if (key == null || GameSave.data == null || !GameSave.data.hasSave) return;

        if (GameSave.data.keyIds.Contains(key.Id)) // Cambia key.id por el nombre del campo int de tu KeyData
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision != CompareTag("Player")) return;

        KeyDoorManager keyManager = collision.GetComponent<KeyDoorManager>();

        if (keyManager == null ) return;

        keyManager.SaveKey(key);

        Destroy(gameObject);
    }
}
