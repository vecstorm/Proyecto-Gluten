using UnityEngine;

[CreateAssetMenu(fileName = "Xp", menuName = "Scriptable Objects/Xp")]
public class XpData : ScriptableObject
{
    [SerializeField] public int xp;

    [SerializeField] public GameObject xpPickUp;
}
