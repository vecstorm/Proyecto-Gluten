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
        Debug.Log("entra colicion");

            if (player != null)
            {
                player.AddXp(xpValue);
                Destroy(gameObject);
            }
        }
    }
}
