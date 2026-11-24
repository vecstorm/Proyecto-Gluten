using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShowTutorialUI : MonoBehaviour
{
    public TextMeshProUGUI tutorialUI;

    public GameObject panelTutorial;

    [Header("Frases por acción (usa {0} para la tecla/botón)")]
    [TextArea]
    private string tutorialMovimiento = "Pulsa {0} para moverte";
    [TextArea]
    private string tutorialSalto = "Pulsa {0} para saltar";
    [TextArea]
    private string tutorialDash = "Pulsa {0} para hacer dash";
    [TextArea]
    private string tutorialHook = "Pulsa {0} para usar el gancho";
    [TextArea]
    private string tutorialAttack = "Pulsa {0} para atacar";
    [TextArea]
    private string tutorialParry = "Pulsa {0} para usar el parry y stunear enemigos";
    [TextArea]
    private string tutorialSavePoint = "Pulsa {0} para acceder al punto de guardado. Ahi podras curarte y mejorar";
    [TextArea]
    private string tutorialRangedAttack = "Pulsa {0} para atacar a distancia";
    [TextArea]
    private string tutorialMap = "Pulsa {0} para mostrar el mapa";
    [TextArea]
    private string tutorialPause = "Pulsa {0} para abrir el menu de pausa";

    private GlutenActionControls glutenActionControls;

    public enum MyEnum
    {
        Jump,
        Move,
        Dash,
        Hook,
        Attack,
        Parry,
        Action,
        RangedAttack,
        Map,
        Pause
    }

    private const string jumpPath = "Player/Jump";
    private const string movePath = "Player/Move";
    private const string dashPath = "Player/Dash";
    private const string hookPath = "Player/Hook";
    private const string attackPath = "Player/Attack";
    private const string parryPath = "Player/Parry";
    private const string interactPath = "Player/Action";
    private const string rangedPath = "Player/RangedAttack";
    private const string mapPath = "Player/Map";
    private const string pausePath = "Player/Pause";


    public MyEnum accionSeleccionada;

    private string GetActionPath(MyEnum action)
    {
        return action switch
        {
            MyEnum.Jump => jumpPath,
            MyEnum.Move => movePath,
            MyEnum.Dash => dashPath,
            MyEnum.Hook => hookPath,
            MyEnum.Attack => attackPath,
            MyEnum.Parry => parryPath,
            MyEnum.Action => interactPath,
            MyEnum.RangedAttack => rangedPath,
            MyEnum.Map => mapPath,
            MyEnum.Pause => pausePath,
            _ => ""
        };
    }

    private string GetSentenceTemplate(MyEnum action)
    {
        return action switch
        {
            MyEnum.Jump => tutorialSalto,
            MyEnum.Move => tutorialMovimiento,
            MyEnum.Dash => tutorialDash,
            MyEnum.Hook => tutorialHook,
            MyEnum.Attack => tutorialAttack,
            MyEnum.Parry => tutorialParry,
            MyEnum.Action => tutorialSavePoint,
            MyEnum.RangedAttack => tutorialRangedAttack,
            MyEnum.Map => tutorialMap,
            MyEnum.Pause => tutorialPause,
            _ => "Pulsa {0}"
        };
    }

    void Awake()
    {
        glutenActionControls = new GlutenActionControls();
        if (panelTutorial != null)
        {
            panelTutorial.SetActive(false);
        }
    }

    void OnEnable()
    {
        glutenActionControls?.Enable();
    }

    void OnDisable()
    {
        glutenActionControls?.Disable();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player == null || tutorialUI == null) return;
        Debug.Log(accionSeleccionada);
        var action = glutenActionControls.FindAction(GetActionPath(accionSeleccionada));
        Debug.Log(action);
        if (action == null) return;

        string grupoActual = ObtenerDispositivoActivo();
        string buttonText = action.GetBindingDisplayString(InputBinding.MaskByGroup(grupoActual));

        if (string.IsNullOrEmpty(buttonText))
        {
            buttonText = "No botón asignado";
        }

        tutorialUI.text = string.Format(GetSentenceTemplate(accionSeleccionada), buttonText);
        panelTutorial.SetActive(true);
        Debug.Log(buttonText);
    }

    private string ObtenerDispositivoActivo()
    {
        if (Gamepad.current != null)
        {
            double tiempoMando = Gamepad.current.lastUpdateTime;
            double tiempoTeclado = Keyboard.current != null ? Keyboard.current.lastUpdateTime : 0;
            double tiempoRaton = Mouse.current != null ? Mouse.current.lastUpdateTime : 0;

            if (tiempoMando > tiempoTeclado && tiempoMando > tiempoRaton)
            {
                return "Gamepad"; // Pon aquí el nombre exacto de tu esquema de mando
            }
        }

        return "Keyboard&Mouse";
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null && panelTutorial != null)
        {
            panelTutorial.SetActive(false);
        }
    }
}