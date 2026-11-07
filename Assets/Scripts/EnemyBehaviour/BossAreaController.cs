using UnityEngine;

public class BossAreaController : MonoBehaviour
{
    [SerializeField] private GameObject ownedBoss;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            MiniBossEnemyAI bossAI = ownedBoss.GetComponent<MiniBossEnemyAI>();

            if (bossAI != null)
            {
                Debug.Log("Player entered the boss area!");
                bossAI.BossActive(true);
            }
        }
    }
}
