using UnityEngine;

public class XpDropEnemy : MonoBehaviour
{

    [SerializeField] private XpData xp;


    public void DropXp()
    {
        if (xp == null) return;


        GameObject xpObject = Instantiate(xp.xpPickUp, transform.position, Quaternion.identity);

        CollectableXp collectableXp = xpObject.GetComponent<CollectableXp>();

        if (collectableXp != null)
        {
            collectableXp.SetValueXp(xp.xp);
        }

    }
}
