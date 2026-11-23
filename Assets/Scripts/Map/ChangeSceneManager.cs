using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneManager : MonoBehaviour
{
    [SerializeField] private Canvas fadeCanvas;

    [SerializeField] private List<string> sceneNames = new List<string>();
    [SerializeField] private GameObject player;
    private SceneExitsManager exitsManager;
    private int destinationExitId;

    public static ChangeSceneManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    public void ChangeScene(int sceneIndex, int destinationExitId)
    {
        this.destinationExitId = destinationExitId;
        //fade out
        SceneManager.LoadScene(sceneNames[sceneIndex]);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        exitsManager = FindFirstObjectByType<SceneExitsManager>();

        player.transform.position = exitsManager.GetSceneExitPosition(destinationExitId).position;

        //fade in
    }




    public List<string> GetSceneNames()
    {
        return sceneNames;
    }
    

}
