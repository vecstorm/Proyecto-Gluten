using UnityEngine;

[CreateAssetMenu(fileName = "KeyData", menuName = "Scriptable Objects/KeyData")]
public class KeyData : ScriptableObject
{
    [SerializeField] public int Id;

    [SerializeField] public GameObject key;
}
