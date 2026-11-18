using UnityEngine;

public class ZoneID : MonoBehaviour
{
    [SerializeField] private int SceneID;

    private void Start()
    {
        // Lo que hacemos es que si la instancia del MapManager no es nulo entonces pasamos el ID de la escena
        if (MapManager.Instance != null)
        {
            // Lo pasamos a la zona para desbloquear
            MapManager.Instance.SetZone(SceneID);
        }
    }
}