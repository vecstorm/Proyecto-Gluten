
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MapManager : MonoBehaviour
{ 
    // ID que identifica esta zona del mapa
    [SerializeField] private int Map_ID;

    // Canvas del mapa
    [SerializeField] private GameObject MapCanva;

    private GameObject[] Map_Images;

    // Unica instancia del MapManager en todo el juego
    public static MapManager Instance;

    private void Awake()
    {
        // Si ya hay un MapManager activo, este se destruye pero le pasa su zona
        if (Instance != null && Instance != this)
        {
            // El Canvas de esta escena sobra, el de la zona anterior sigue vivo
            if (MapCanva != null && MapCanva != Instance.MapCanva && MapCanva.scene.IsValid())
            {
                Destroy(MapCanva);
            }

            Instance.SetZone(Map_ID);

            Destroy(gameObject);

            return;
        }

        Instance = this;

        // Mantener el MapManager entre escenas
        DontDestroyOnLoad(gameObject);

        // Si el Canvas asignado es el prefab y no una instancia de la escena, lo creamos
        if (!MapCanva.scene.IsValid())
        {
            MapCanva = Instantiate(MapCanva);
        }

        // Para poder mantener el Canvas entre escenas tiene que ser un objeto raiz
        if (MapCanva.transform.parent != null)
        {
            MapCanva.transform.SetParent(null, false);
        }

        // Mantener el Canvas entre escenas
        DontDestroyOnLoad(MapCanva);

        // Si el Canvas tiene escala 0 no se ve, lo arreglamos
        RectTransform rect = MapCanva.GetComponent<RectTransform>();

        if (rect != null && rect.localScale == Vector3.zero)
        {
            rect.localScale = Vector3.one;
        }
    }

    private void OnDestroy()
    {
        // Si se destruye el MapManager que estaba activo, el siguiente vuelve a ser el unico
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        // Buscar automaticamente las imagenes del Canvas
        CollectImages();

        // Mostrar la imagen de la zona actual
        SetZone(Map_ID);
    }

    // Buscar las imagenes de las zonas que hay dentro del Canvas
    private void CollectImages()
    {
        Map_Images = MapCanva.GetComponentsInChildren<Image>(true).Select(img => img.gameObject).Where(obj => obj != MapCanva).ToArray();
    }

    // Muestra en el mapa la imagen de la zona indicada y oculta las demas
    public void SetZone(int zone)
    {
        // El mapa esta siempre visible
        MapCanva.SetActive(true);

        // Si todavia no hemos buscado las imagenes, las buscamos
        if (Map_Images == null)
        {
            CollectImages();
        }

        // Ocultar todas las imagenes
        foreach (GameObject image in Map_Images)
        {
            if (image != null)
            {
                image.SetActive(false);
            }
        }

        // Buscar la imagen de la zona por su nombre (Zona1, Zona2...)
        foreach (GameObject image in Map_Images)
        {
            if (image != null && image.name == "Zona" + zone)
            {
                image.SetActive(true);

                return;
            }
        }

        Debug.LogWarning($"No hay imagen para la zona {zone}", this);
    }
}
