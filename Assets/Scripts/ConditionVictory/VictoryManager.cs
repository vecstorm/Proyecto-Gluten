using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryNabager : MonoBehaviour
{
    private static VictoryNabager instance;

    [Header("UI References")]
    [SerializeField] private Image ControlPanel;
    [SerializeField] private Image ImageDefeat;

    [Header("Panel Color")]
    [SerializeField] private Color panelColor = new Color(0.357f, 0.188f, 0.188f, 1f);

    [Header("Panel Timings")]
    [SerializeField] private float panelStart = 0f;
    [SerializeField] private float panelHide = 2f;

    [Header("Image Timings")]
    [SerializeField] private float imageStart = 0.5f;
    [SerializeField] private float imageHide = 2f;

    [Header("Fade Duration")]
    [SerializeField] private float fadeTime = 1f;

    private CharacterStats playerStats;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        PrepareImage(ControlPanel, panelColor);
        if (ImageDefeat != null)
            PrepareImage(ImageDefeat, ImageDefeat.color);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SearchPlayer();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribePlayer();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SearchPlayer();
    }

    private void SearchPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        CharacterStats newPlayer = playerObj != null ? playerObj.GetComponent<CharacterStats>() : null;

        if (newPlayer == playerStats)
            return;

        UnsubscribePlayer();
        playerStats = newPlayer;

        if (playerStats != null)
            playerStats.OnDeath += StartSequence;
    }

    private void UnsubscribePlayer()
    {
        if (playerStats != null)
            playerStats.OnDeath -= StartSequence;
    }

    private void PrepareImage(Image img, Color color)
    {
        if (img == null)
            return;

        color.a = 0f;
        img.color = color;
        img.raycastTarget = false;
    }

    private void StartSequence()
    {
        StopAllCoroutines();
        StartCoroutine(ShowSequence());
    }

    // Esta es una función que al principio y al final hace el timepo que se muestra
    // Y desaparece el panel y la imagen
    private IEnumerator ShowSequence()
    {
        yield return ShowElement(ControlPanel, panelStart, 0f, 1f);
        yield return ShowElement(ImageDefeat, imageStart, 0f, 1f);

        yield return ShowElement(ImageDefeat, imageHide, 1f, 0f);
        yield return ShowElement(ControlPanel, panelHide, 1f, 0f);
    }

    // Está función muestra a partir de los elementos(panel/imagen) se espera un tiempo para que los otros se muestren
    private IEnumerator ShowElement(Image img, float delay, float startAlpha, float endAlpha)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        yield return Fade(img, startAlpha, endAlpha);
    }

    // El tiempo estumado que durada el panel y la imagen antes de desaparecer
    private IEnumerator Fade(Image img, float startAlpha, float endAlpha)
    {
        if (img == null)
            yield break;

        Color color = img.color;

        if (fadeTime <= 0f)
        {
            color.a = endAlpha;
            img.color = color;
            yield break;
        }

        for (float time = 0f; time < fadeTime; time += Time.deltaTime)
        {
            color.a = Mathf.Lerp(startAlpha, endAlpha, time / fadeTime);
            img.color = color;
            yield return null;
        }

        color.a = endAlpha;
        img.color = color;
    }
}