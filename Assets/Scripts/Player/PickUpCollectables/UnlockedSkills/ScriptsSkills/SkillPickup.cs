using UnityEngine;

public class SkillPickup : MonoBehaviour
{


    [SerializeField] private SkillData skillToUnlock;

    private bool collected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player"))
        {
            return;
        }

        SkillUnlockManager manager = FindAnyObjectByType<SkillUnlockManager>();

        if (manager == null) 
        {
            Debug.Log("no se ha encontrado un SkillUnlockManager en la escena");
            return;
        }

        if (manager.UnlockSkill(skillToUnlock))
        {
            collected = true;
            Debug.Log("Habilidad desbloqueada: " + skillToUnlock.skillName);
            DestroyPickeable();
        }

    }

    public void DestroyPickeable()
    {
        //Animaciones sonidos y particulas
        Destroy(gameObject);
    }

    private void Start()
    {
        if (GameSave.data != null && GameSave.data.hasSave) { 
            Debug.Log("SkillPickup Start: " + skillToUnlock.skillName);
        Debug.Log("SkillPickup Start: " + skillToUnlock.skillId);
        Debug.Log("SkillPickup Start: " + GameSave.data.unlockedSkillIds.Count);
        Debug.Log("SkillPickup Start: " + string.Join(", ", GameSave.data.unlockedSkillIds));
        if (skillToUnlock != null &&
            !string.IsNullOrEmpty(skillToUnlock.skillId) &&
            GameSave.data != null &&
            GameSave.data.unlockedSkillIds != null &&
            GameSave.data.unlockedSkillIds.Contains(skillToUnlock.skillId))
        {
            Debug.Log("Habilidad ya desbloqueada: " + skillToUnlock.skillName);
            Destroy(gameObject);
        }
        }
    }
}
