
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{ 
    // ID que identifica esta zona del mapa
    [SerializeField] private int Map_ID;

    // Canvas del mapa
    [SerializeField] private GameObject MapCanva;

    private MapZoneImage[] Map_ZoneImages;

    // Esto guarda todas las zonas que haya (Evitando duplicados)
    private HashSet<int> unlockedZones = new HashSet<int>();

    // Unica instancia del MapManager en todo el juego
    public static MapManager Instance;

    private void Awake()
    {
        // Si ya hay un MapManager activo, este se destruye pero le pasa su ID
        if (Instance != null && Instance != this)
        {
            // El Canvas de esta escena sobra, el de la zona anterior sigue vivo
            if (MapCanva != null && MapCanva != Instance.MapCanva && MapCanva.scene.IsValid())
            {
                Destroy(MapCanva);
            }

            //Instance.SetZone(Map_ID);

            //Destroy(gameObject);

            return;
        }

        Instance = this;

        // Mantenemos el MapManager entre las escenas
        DontDestroyOnLoad(gameObject);

        // Si el canvas es asignado un prefab y no una instancia, lo creamos
        if (!MapCanva.scene.IsValid())
        {
            MapCanva = Instantiate(MapCanva);
        }

        // Sera una raiz principal para poder mantenerlo entre las escenas
        if (MapCanva.transform.parent != null)
        {
            MapCanva.transform.SetParent(null, false);
        }

        // Para que no destruya el canva
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
        // Buscamos al jugador de ESTA MISMA ESCENA DONDE ESTA(con cada escena lo pasa)
        GameObject player = GameObject.FindWithTag("Player");

        // Aunque el jugador no este en la Escena ocultamos el mapa IGUALMENTE
        if (player == null && Instance != null)
            Instance.MapCanva.SetActive(false);

        // El MapManager de las demás zonas solo avisan si el jugador esta en esa zona o no
        if (Instance != this)
        {
            if (player != null)
                Instance.SetZone(Map_ID);

            return;
        }

        // Buscamos automaticamente las imagenes del Canvas
        CollectImages();

        // AL iniciar todas las imagenes estan inactivas
        foreach (MapZoneImage zoneImage in Map_ZoneImages)
            if (zoneImage != null) zoneImage.gameObject.SetActive(false);

        // Mostramos la imagen de la zona actual
        //SetZone(Map_ID);

        // Si empezamos en una escena con jugador, desbloqueamos su zona
        if (player != null)
            SetZone(Map_ID);
    }

    // Buscamos las imagenes del mapa que hay dentro del Canvas por su ID
    private void CollectImages()
    {
        Map_ZoneImages = MapCanva.GetComponentsInChildren<MapZoneImage>(true);
    }

    // Mostramos en el mapa la zona indicada, dejando las desbloqueadas
    public void SetZone(int zone)
    {
        // El mapa estará invisible, en caso de la función de "HabilityMap" COMENTAR ESTO
        MapCanva.SetActive(false);

        // Si todavia no tenemos las imagenes, las buscamos
        if (Map_ZoneImages == null)
        {
            CollectImages();
        }

        // Si la zona no estaba desbloqueada, la desbloqueamos
        // HashSet ya ignora los duplicados
        unlockedZones.Add(zone);

        // Mostramos las imagenes de las zonas desbloqueadas y ocultamos las demas
        foreach (MapZoneImage zoneImage in Map_ZoneImages)
        {
            if (zoneImage != null)
            {
                zoneImage.gameObject.SetActive(unlockedZones.Contains(zoneImage.ZoneID));
            }
        }

        // Avisamos si esa zona no tiene imagen en el mapa
        if (!Map_ZoneImages.Any(zoneImage => zoneImage != null && zoneImage.ZoneID == zone))
        {
            Debug.LogWarning($"No hay imagen para la zona {zone}", this);
        }
    }

    // OPCIONAL: Es de manera más simple para poder llamar a la función y solo poner su input
    //public void HabilityMap()
    //{
    //    MapCanva.SetActive(!MapCanva.activeSelf);
    //}
}
