using System.Collections.Generic;
using UnityEngine;

public class SkillUnlockManager : MonoBehaviour
{
    //hacemos una colección de las skills. 
    //Con HashSet no se duplican y no importa el orden
    public HashSet<string> unlockSkills = new HashSet<string>();

    [SerializeField] public SkillData[] allSkills;

    /// <summary>
    /// MEtodo para verificar si la colección contiene la habilidad
    /// </summary>
    /// <param name="skillID"></param>
    /// <returns></returns>
    public bool IsSkillUnlocked(string skillID)
    {
        return unlockSkills.Contains(skillID);
    }
    
    /// <summary>
    /// Metodo para desbloquear una habilidad
    /// </summary>
    /// <param name="skill"></param>
    /// <returns></returns>
    public bool UnlockSkill(SkillData skill)
    {

        if (skill == null || string.IsNullOrEmpty(skill.skillId))
        {
            return false;
        }

        return unlockSkills.Add(skill.skillId);

    }

    public bool UnlockSkillById(string skillID)
    {
        foreach (SkillData skill in allSkills)
        {
            if(skill !=null && skill.skillId == skillID){
                return UnlockSkill(skill);
            }
        }
        Debug.Log("No se esncuentra la habilidad con ID:" + skillID);
        return false;
    }

    public SkillData GetSkillByID(string skillID)
    {
        foreach(SkillData skill in allSkills)
        {
            if (skill != null && skill.skillId == skillID)
            {
                return skill;
            }
        }

        return null;
    }

    public void LogUnlockedSkills()
    {
        foreach(string skillID in unlockSkills) 
        {
            Debug.Log($"Unlocked skills: {skillID}");
        }
    }

    [ContextMenu("Pruebas/Desbloquear Dash")]
    private void TestUnlockDash()
    {
        bool unlocked = UnlockSkillById("DASH");

        Debug.Log(unlocked
            ? "Dash desbloqueado correctamente."
            : "El Dash ya estaba desbloqueado o no se encontró.");
    }

    [ContextMenu("Pruebas/Mostrar habilidades desbloqueadas")]
    private void TestLogUnlockedSkills()
    {
        LogUnlockedSkills();
    }
}
