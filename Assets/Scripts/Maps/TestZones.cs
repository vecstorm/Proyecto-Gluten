using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class TestZones : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // Tecla 1 -> Pasillo_1
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            SceneManager.LoadScene("Pasillo_1");

        // Tecla 2 -> Pasillo_2
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            SceneManager.LoadScene("Pasillo_2");
    }
}