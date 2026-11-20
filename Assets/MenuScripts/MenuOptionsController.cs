using UnityEngine;
using UnityEngine.UI;

public class MenuOptionsController : MonoBehaviour
{
    [SerializeField] Button generalOptionsButton;
    [SerializeField] Button rebindingOptionsButton;

    [SerializeField] GameObject CanvasGeneral;
    [SerializeField] GameObject CanvasRebinding;


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
}
