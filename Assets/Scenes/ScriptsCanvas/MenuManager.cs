using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] public GameObject mainMenuCanvas;
    [SerializeField] public GameObject optionCanvas;
    [SerializeField] public GameObject pauseMenu;

    public void SetCanvasActive(GameObject canva)
    {
        canva.SetActive(true);
        canva.SetActive(true);
        canva.SetActive(true);
    }
    public void SetCanvasNotActive(GameObject canva)
    {
        canva.SetActive(false);
        canva.SetActive(false);
        canva.SetActive(false);
    }

    public void ChangeSceneToGame()
    {
        //SceneManager.LoadScene(PlayGameScene);
    }
}
