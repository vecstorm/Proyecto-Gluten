
using System.Collections;
using UnityEngine;
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
    }

    private void Start()
    {
        PrepareImage(ControlPanel, panelColor);
        PrepareImage(ImageDefeat, ImageDefeat.color);
    }

    private void OnEnable()
    {
        SearchPlayer();
    }

    private void Update()
    {
        if (playerStats == null)
            SearchPlayer();
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.OnDeath -= StartSequence;
    }

    private void SearchPlayer()
    {
        CharacterStats newPlayer = GameObject.FindGameObjectWithTag("Player")?.GetComponent<CharacterStats>();

        if (newPlayer == null || newPlayer == playerStats) return;

        if (playerStats != null)
            playerStats.OnDeath -= StartSequence;

        playerStats = newPlayer;
        playerStats.OnDeath += StartSequence;
    }

    private void PrepareImage(Image img, Color color)
    {
        color.a = 0f;
        img.color = color;
        img.raycastTarget = false;
    }

    private void StartSequence()
    {
        StopAllCoroutines();
        StartCoroutine(ShowSequence());
    }

    private IEnumerator ShowSequence()
    {
        yield return ShowElement(ControlPanel, panelStart, 0f, 1f);
        yield return ShowElement(ImageDefeat, imageStart, 0f, 1f);

        yield return ShowElement(ImageDefeat, imageHide, 1f, 0f);
        yield return ShowElement(ControlPanel, panelHide, 1f, 0f);
    }

    private IEnumerator ShowElement(Image img, float delay, float startAlpha, float endAlpha)
    {
        yield return new WaitForSeconds(delay);
        yield return Fade(img, startAlpha, endAlpha);
    }

    private IEnumerator Fade(Image img, float startAlpha, float endAlpha)
    {
        Color color = img.color;

        for (float time = 0; time < fadeTime; time += Time.deltaTime)
        {
            color.a = Mathf.Lerp(startAlpha, endAlpha, time / fadeTime);
            img.color = color;
            yield return null;
        }

        color.a = endAlpha;
        img.color = color;
    }
}
