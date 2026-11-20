using System.Collections.Generic;
using UnityEngine;

public class KeyDoorManager : MonoBehaviour
{

    public HashSet<int> keys = new HashSet<int>(); //Coleccion de llaves que tiene el personaje

    public void SaveKey(KeyData key)
    {
        if (key == null) return;

        if (keys.Add(key.Id))
        {
            Debug.Log("llave obtenida" + key.Id);
        }
        
    }

    public bool HasKey(KeyData key)
    {
        Debug.Log("no hay llave");
        if (key == null) return false;

        return keys.Contains(key.Id);
    }
}
