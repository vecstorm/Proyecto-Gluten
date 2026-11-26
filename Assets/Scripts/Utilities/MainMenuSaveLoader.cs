using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuSaveLoader : MonoBehaviour
{
    [SerializeField] private string fallbackGameplayScene = "EnemyTestMechanics";

    public void ContinueGame()
    {
        GameSave.LoadFromDisk();

        if (GameSave.data == null || !GameSave.data.hasSave)
        {
            Debug.LogWarning("No hay partida guardada para cargar.");
            return;
        }

        string targetScene = !string.IsNullOrEmpty(GameSave.data.sceneName)
            ? GameSave.data.sceneName
            : fallbackGameplayScene;

        SceneManager.LoadScene(targetScene);
    }
}