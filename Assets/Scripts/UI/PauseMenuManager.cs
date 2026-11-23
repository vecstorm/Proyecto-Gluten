
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuPauseManager : MonoBehaviour
{
    public static MenuPauseManager Instance;

    [SerializeField] private GameObject pauseMenu;

    private bool isPaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (pauseMenu == null) return;


        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void ReturnToGameMenu()
    {
        SceneManager.LoadScene("SceneMenus");
    }

    public void SetCanvasActive(GameObject canva)
    {
        canva.SetActive(true);
    }

    public void SetCanvasNotActive(GameObject canva)
    {
        canva.SetActive(false);
    }

}
