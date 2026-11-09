using System.Collections.Generic;
using UnityEngine;

public static class EnemyManager
{
    private static List<EnemyAI> enemiesOnMap = new List<EnemyAI>();

    public static List<EnemyAI> EnemiesOnMap => enemiesOnMap;

    public static void ResetEnemies()
    {
        Debug.Log("Entered in ResetEnemies");

        int enemiesResetedCount = 0;

        foreach (EnemyAI enemy in enemiesOnMap)
        {
            enemiesResetedCount++;
            Debug.Log( "Reseted " + enemiesResetedCount + " enemies.");

            enemy.gameObject.SetActive(true);
            enemy.ResetEnemy();
        }
    }
}
