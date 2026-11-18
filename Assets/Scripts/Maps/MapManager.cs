using UnityEngine;
using System.Linq;

public class MapManager : MonoBehaviour
{
    [Header("Configuración MapCanva")]
    [SerializeField] private GameObject MapCanva;

    [Header("Pon el X de imagenes del mapa que habran 0-X")]
    [SerializeField] private bool[] unlockedZones;

    private MapZoneImage[] mapZoneImages;

    // Creamos una instancia para el MapManger nuestro
    public static MapManager Instance;

    private void Awake()
    {
        // Si ya existe un MapManager antes entonces lo que hara es destruir el duplicado
        if (Instance != null && Instance != this)
        {
            if (MapCanva != null && MapCanva != Instance.MapCanva && MapCanva.scene.IsValid())
            {
                Destroy(MapCanva);
            }
            Destroy(gameObject);
            return;
        }

        // Si es el primer MapManager entonces se convierte en la instancia oficial
        Instance = this;

        // El MapManager NO SE DESTRUYE al cambiar de escena
        DontDestroyOnLoad(gameObject);

        // Configuramos y protegemos el MapCanva para que tampoco se destruya
        if (MapCanva != null)
        {
            if (!MapCanva.scene.IsValid())
            {
                MapCanva = Instantiate(MapCanva);
            }

            if (MapCanva.transform.parent != null)
            {
                MapCanva.transform.SetParent(null, false);
            }

            // Hacemos que el mapa no se destruya entre escenas
            DontDestroyOnLoad(MapCanva);

            RectTransform rect = MapCanva.GetComponent<RectTransform>();
            if (rect != null && rect.localScale == Vector3.zero)
            {
                rect.localScale = Vector3.one;
            }
        }
    }

    private void Start()
    {
        CollectImages();
        UpdateMapVisuals();

        // Nos aseguramos de que el mapa empiece cerrado al arrancar el juego
        if (MapCanva != null)
        {
            MapCanva.SetActive(false);
        }
    }

    // Buscamos todas las imágenes de mapa en el Canvas
    private void CollectImages()
    {
        if (MapCanva != null)
        {
            mapZoneImages = MapCanva.GetComponentsInChildren<MapZoneImage>(true);
        }
    }

    // Este es un metodo que recibe la ID de la escena actual y marca su booleano como true
    public void UnlockZone(int zoneID)
    {
        // Verificamos que el ID esté dentro del tamaño del array booleano
        if (unlockedZones != null && zoneID >= 0 && zoneID < unlockedZones.Length)
        {
            unlockedZones[zoneID] = true; // Se queda en true de forma permanente
            Debug.Log($"Zona {zoneID} desbloqueada en el array de booleanos.");
        }
        else
        {
            Debug.LogWarning($"El ID de zona {zoneID} está fuera del rango del array booleano.");
        }

        // Actualizamos los gráficos del mapa inmediatamente
        UpdateMapVisuals();
    }

    // Recorre todas las imágenes del mapa y enciende las que tengan su booleano en TRUE
    private void UpdateMapVisuals()
    {
        if (mapZoneImages == null)
        {
            CollectImages();
        }

        if (mapZoneImages == null) return;

        foreach (var zoneImage in mapZoneImages)
        {
            if (zoneImage != null)
            {
                // Si el ID de la imagen coincide con un índice en true del array, se activa; si no, se oculta
                if (zoneImage.ZoneID >= 0 && zoneImage.ZoneID < unlockedZones.Length)
                {
                    bool isUnlocked = unlockedZones[zoneImage.ZoneID];
                    zoneImage.gameObject.SetActive(isUnlocked);
                }
                else
                {
                    zoneImage.gameObject.SetActive(false);
                }
            }
        }
    }

    // OPCIONAL: Esta función sirve para abrir o cerrar el mapa
    public void HabilityMap()
    {
        if (MapCanva != null)
        {
            MapCanva.SetActive(!MapCanva.activeSelf);
        }
    }
}