using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class SaveData
{
    public bool hasSave;
    public string sceneName;
    public Vector3 playerPosition;

    public float currentHealth;
    public float maxHealth;
    public int experience;
    public int bonusHealthPoints;
    public int healthUpgradeCost;
    public int bonusAttackPoints;
    public int attackUpgradeCost;
    public float basicAttackDamage;

    public List<int> keyIds = new List<int>();
    public List<string> unlockedIds = new List<string>();
    public List<string> unlockedSkillIds = new List<string>();
}

public static class GameSave
{
    public static SaveData data = new SaveData();
    public static bool isLoadingSave = false;

    private static string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

    public static void Save()
    {
        data.hasSave = true;
        data.sceneName = SceneManager.GetActiveScene().name;
        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
    }

    public static void DeleteSave()
    {
        if (System.IO.File.Exists(SavePath))
        {
            System.IO.File.Delete(SavePath);
        }

        data = new SaveData();
        data.hasSave = false;
    }

    public static void LoadFromDisk()
    {
        if (File.Exists(SavePath))
            data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath)) ?? new SaveData();
        else
            data = new SaveData();
    }

    public static void MarkUnlocked(string id)
    {
        if (!string.IsNullOrEmpty(id) && !data.unlockedIds.Contains(id))
            data.unlockedIds.Add(id);
    }

    public static bool IsUnlocked(string id) => data.unlockedIds.Contains(id);
}