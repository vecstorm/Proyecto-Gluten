using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] public GameObject mainMenuCanvas;
    [SerializeField] public GameObject optionCanvas;
    [SerializeField] public GameObject pauseMenu;
    [SerializeField] public GameObject CreditMenu;

    private bool isPaused = false;

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
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

    public void SetCanvasActive(GameObject canva)
    {
        canva.SetActive(true);
    }

    public void SetCanvasNotActive(GameObject canva)
    {
        canva.SetActive(false);
    }

    public void ChangeSceneToGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
