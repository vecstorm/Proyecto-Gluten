using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShowTutorialUI : MonoBehaviour
{
    public TextMeshProUGUI tutorialUI;
    [TextArea]
    public string tutorialMovimiento = "Pulsa {0} para moverte";

    private GlutenActionControls glutenActionControls;

    public enum MyEnum
    {
        Jump,
        Move,
        Dash
    }

    private string jumpText = "Player/Jump";
    private string dashText = "Player/Dash";
    private string moveText = "Player/Move";

    public MyEnum accionSeleccionada;

    public string GetActionPath(MyEnum action)
    {
        return action switch
        {
            MyEnum.Jump => jumpText,
            MyEnum.Dash => dashText,
            MyEnum.Move => moveText,
            _ => ""
        };
    }

    void Start()
    {
        glutenActionControls = new GlutenActionControls();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player)
        {
            tutorialUI.gameObject.SetActive(true);

            var action = glutenActionControls.FindAction(GetActionPath(accionSeleccionada));

            // 1. Detectamos si lo último que tocó el jugador fue el mando o el teclado
            string grupoActual = ObtenerDispositivoActivo();

            // 2. Pedimos las teclas o botones de ese dispositivo
            string buttonText = action.GetBindingDisplayString(InputBinding.MaskByGroup(grupoActual));

            if (string.IsNullOrEmpty(buttonText))
            {
                buttonText = "No botón asignado";
            }

            tutorialUI.text = string.Format(tutorialMovimiento, buttonText);
            Debug.Log(buttonText);
        }
    }

    private string ObtenerDispositivoActivo()
    {
        // Si hay un mando conectado...
        if (Gamepad.current != null)
        {
            // ...y no hay teclado, o el mando se usó más recientemente que el teclado y ratón
            double tiempoMando = Gamepad.current.lastUpdateTime;
            double tiempoTeclado = Keyboard.current != null ? Keyboard.current.lastUpdateTime : 0;
            double tiempoRaton = Mouse.current != null ? Mouse.current.lastUpdateTime : 0;

            if (tiempoMando > tiempoTeclado && tiempoMando > tiempoRaton)
            {
                return "Controller"; // Asegúrate de que el grupo en tu InputActions se llame "Gamepad"
            }
        }

        return "Keyboard&Mouse"; // Por defecto devuelve teclado y ratón
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player)
        {
            tutorialUI.gameObject.SetActive(false);
        }
    }
}
