
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] public GameObject mainMenuCanvas;
    [SerializeField] public GameObject optionCanvas;
    [SerializeField] public GameObject CreditMenu;

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
        Time.timeScale = 1f;
        SceneManager.LoadScene("Tutorial1");
    }

    public void ChangeSceneToMenus()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SceneMenus");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
