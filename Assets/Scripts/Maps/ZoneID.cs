using UnityEngine;

public class ZoneID : MonoBehaviour
{
    [SerializeField] private int SceneID;

    private void Start()
    {
        // Al iniciar la escena, enviamos nuestro ID al MapManager para que desbloquee la zona correspondiente
        if (MapManager.Instance != null)
        {
            MapManager.Instance.UnlockZone(SceneID);
        }
        else
        {
            Debug.LogWarning($"No se encontró el MapManager en la escena para la zona ID: {SceneID}");
        }
    }
}