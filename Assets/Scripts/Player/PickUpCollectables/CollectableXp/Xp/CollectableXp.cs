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
            PlayerXp player = collision.GetComponent<PlayerXp>();

            if (player != null)
            {
                player.AddXp(xpValue);
                Destroy(gameObject);
            }
        }
    }
}
