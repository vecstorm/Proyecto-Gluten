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
            Destroy(gameObject);
        }

    }
}
