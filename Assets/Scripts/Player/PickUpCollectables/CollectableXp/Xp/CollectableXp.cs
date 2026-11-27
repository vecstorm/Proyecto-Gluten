    using UnityEngine;

public class CollectableXp : MonoBehaviour
{
    private int xpValue = 1;
    public void SetValueXp (int value)
    {
        xpValue = value;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CharacterStats player = collision.GetComponent<CharacterStats>();

            if (player != null)
            {
                player.AddXp(xpValue);
                AudioController.Instance.PlayPlayerInteraction(PlayerInteractionSFX.CollectExperience);
                ParticleManager.SpawnParticles("PickObject", transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }
}
