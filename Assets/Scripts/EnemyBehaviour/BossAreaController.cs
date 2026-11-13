using UnityEngine;

public class BossAreaController : MonoBehaviour
{
    private BossAI ownedBoss;
    private GameObject areaDoor;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (ownedBoss != null)
            {
                if (ownedBoss.IsDead())
                {
                    return;
                }

                Debug.Log("Player entered the boss area!");
                EnableDoors();
                ownedBoss.BossActive(true);
                ownedBoss.SetTargetTransform(collision.transform);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (ownedBoss != null)
            {
                if (!ownedBoss.IsDead())
                {
                    return;
                }

                Debug.Log("Player exited the boss area!");
                DisableDoors();
                ownedBoss.BossActive(false);
                ownedBoss.SetTargetTransform(null);
            }
        }
    }

    public void DisableDoors()
    {
        areaDoor.SetActive(false);
    }

    public void EnableDoors()
    {
        areaDoor.SetActive(true);
    }

    public void SetAreaDoor(GameObject areaDoor)
    {
        this.areaDoor = areaDoor;
    }

    public void SetOwnedBoss(BossAI ownedBoss)
    {
        this.ownedBoss = ownedBoss;
    }
}
