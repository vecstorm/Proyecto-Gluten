using UnityEngine;

public class KeyPickUp : MonoBehaviour
{
    [SerializeField] KeyData key;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision != CompareTag("Player")) return;

        KeyDoorManager keyManager = collision.GetComponent<KeyDoorManager>();

        if (keyManager == null ) return;

        AudioController.Instance.PlayPlayerInteraction(PlayerInteractionSFX.CollectKey);

        keyManager.SaveKey(key);

        ParticleManager.SpawnParticles("PickObject", transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
