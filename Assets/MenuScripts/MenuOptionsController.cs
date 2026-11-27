using UnityEngine;
using UnityEngine.UI;

public class MenuOptionsController : MonoBehaviour
{
    [SerializeField] Button generalOptionsButton;
    [SerializeField] Button rebindingOptionsButton;

    [SerializeField] GameObject CanvasGeneral;
    [SerializeField] GameObject CanvasRebinding;
    [SerializeField] GameObject keyboardRebinding;
    [SerializeField] GameObject controllerRebinding;


    public void SetPanelGeneralOptionsActive()
    {
        CanvasGeneral.SetActive(true);
        CanvasRebinding.SetActive(false);
    }

    public void SetPanelRebindingOptionsActive()
    {
        CanvasGeneral.SetActive(false);
        CanvasRebinding.SetActive(true);
    }

    public void SetPanelKeyboardRebindingActive()
    {
        keyboardRebinding.SetActive(true);
        controllerRebinding.SetActive(false);
    }

    public void SetPanelControllerRebindingActive()
    {
        keyboardRebinding.SetActive(false);
        controllerRebinding.SetActive(true);
    }
}
