using UnityEngine;

public class SceneExit : MonoBehaviour
{
    [SerializeField] private int sceneExitId; 
    [SerializeField] private int destinationIndex;
    [SerializeField] private int destinationSceneExitId;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        ChangeSceneManager.Instance.ChangeScene(destinationIndex, destinationSceneExitId);
    }

    public int GetsceneExitId() => sceneExitId;
}
