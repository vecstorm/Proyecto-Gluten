
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeSceneManager : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeTime;

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

        fadeImage.color = new Color(
            fadeImage.color.r,
            fadeImage.color.g,
            fadeImage.color.b,
            0f
        );
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void ChangeScene(int sceneIndex, int destinationExitId)
    {
        this.destinationExitId = destinationExitId;

        StartCoroutine(FadeCoroutine(sceneIndex));
    }

    private IEnumerator FadeCoroutine(int sceneIndex)
    {
        // Fade In: aparece la imagen
        yield return fadeImage.DOFade(1f, fadeTime)
            .SetEase(Ease.Linear)
            .WaitForCompletion();

        Debug.Log("Vamos a la escena " + sceneNames[sceneIndex]);

        // Cambiar de escena
        SceneManager.LoadScene(sceneNames[sceneIndex]);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Escena " + scene.name + " Cargada");

        exitsManager = FindFirstObjectByType<SceneExitsManager>();

        player.transform.position =
            exitsManager.GetSceneExitPosition(destinationExitId).position;

        // Fade Out: desaparece la imagen
        fadeImage.DOFade(0f, fadeTime * 2);
    }

    public List<string> GetSceneNames()
    {
        return sceneNames;
    }
}
