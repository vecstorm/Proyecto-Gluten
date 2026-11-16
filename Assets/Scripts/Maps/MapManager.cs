
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Linq;

public class MapManager : MonoBehaviour
{ 
    // ID que identifica esta zona del mapa
    [SerializeField] private int Map_ID;

    // Canvas del mapa
    [SerializeField] private GameObject MapCanva;

    private GameObject[] Map_Images;

    private bool Map_Opened = false;

    private void Awake()
    {
        // Mantener el MapManager entre escenas
        DontDestroyOnLoad(gameObject);

        // Mantener el Canvas entre escenas
        DontDestroyOnLoad(MapCanva);
    }

    private void Start()
    {
        // Buscar automaticamente las imagenes del Canvas
        Map_Images = MapCanva.GetComponentsInChildren<Image>(true).Select(img => img.gameObject).Where(obj => obj != MapCanva).ToArray();

        // Ocultar todas las imagenes inicialmente
        foreach (GameObject image in Map_Images)
        {
            image.SetActive(false);
        }

        // Ocultar el Canvas al iniciar
        MapCanva.SetActive(false);
    }

    private void Update()
    {
        // Abrir y cerrar el mapa con M
        if (Keyboard.current != null &&
            Keyboard.current.mKey.wasPressedThisFrame)
        {
            Map_Opened = !Map_Opened;

            MapCanva.SetActive(Map_Opened);
        }
    }
}
