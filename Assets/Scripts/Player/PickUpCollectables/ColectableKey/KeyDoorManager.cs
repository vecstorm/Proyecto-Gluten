using System.Collections.Generic;
using UnityEngine;

public class KeyDoorManager : MonoBehaviour
{

    public HashSet<int> keys = new HashSet<int>();

    private void Start()
    {
        if (GameSave.data == null || !GameSave.data.hasSave)
        {
            GameSave.LoadFromDisk();
        }

        if (GameSave.data != null && GameSave.data.hasSave)
        {
            LoadKeysFromSave(GameSave.data);
        }
    }

    public void DumpKeysToSave(SaveData data)
    {
        if (data == null) return;
        data.keyIds = new List<int>(keys);
    }

    public void LoadKeysFromSave(SaveData data)
    {
        keys.Clear();
        if (data == null || data.keyIds == null) return;

        foreach (int id in data.keyIds)
        {
            keys.Add(id);
        }
    }

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
