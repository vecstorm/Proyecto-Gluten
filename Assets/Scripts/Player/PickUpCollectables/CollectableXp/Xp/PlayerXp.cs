using UnityEngine;

public class PlayerXp : MonoBehaviour
{

    public int currentXp = 0;


    public void AddXp(int x)
    {
        currentXp += x;

        Debug.Log(currentXp);
    }
}
