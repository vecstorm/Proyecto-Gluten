using UnityEngine;
using System.Linq;

public class MapManager : MonoBehaviour
{
    [Header("Configuración MapCanva")]
    [SerializeField] private GameObject MapCanva;

    // Array de escenas, lo creamos para poner el ID más alto  de las imagenes
    private bool[] unlockedZones;

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

        // Creamos el array segun las imagenes que haya
        CollectImages();
        CreateZoneArray();
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

    private void CreateZoneArray()
    {
        // Guardaremos el ID más alto de las escenas
        int maxID = 0;

        // Comprobamos la lista de las imagenes no esten vacias
        if (mapZoneImages != null)
        {
            // Recorremos todas las imágenes del mapa
            foreach (var img in mapZoneImages)
            {
                // Comprobamos que la imagen exista y que su ID sea mayor al que tenemos guardado
                if (img != null && img.ZoneID > maxID)
                    maxID = img.ZoneID;
            }
        }

        // Creamos un array de booleanos según el ID más alto de las zonas
        unlockedZones = new bool[maxID + 1];
    }

    // Metodo que recibira el ID de la escena actual y marca su booleano como true
    public void UnlockZone(int zoneID)
    {
        // Verificamos que el ID esté dentro del tamaño del array booleano
        if (unlockedZones != null && zoneID >= 0 && zoneID < unlockedZones.Length)
        {
            unlockedZones[zoneID] = true; // Se queda en true de forma permanente
            Debug.Log($"Zona {zoneID} desbloqueada en el array de booleanos");
        }
        else
        {
            Debug.LogWarning($"El ID de zona {zoneID} está fuera del rango del array booleano");
        }

        // Actualizamos el mapa
        UpdateMapVisuals();
    }

    // Función que recorre todas las imagenes del mapa y enciende las que estan en true
    private void UpdateMapVisuals()
    {
        if (mapZoneImages == null)
        {
            CollectImages();
        }

        if (mapZoneImages == null) return;

        // Si el array de la escena no existe, no se muestra nada
        if (unlockedZones == null) return;

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