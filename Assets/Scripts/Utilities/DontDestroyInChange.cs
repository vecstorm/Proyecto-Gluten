using UnityEngine;

public class DontDestroyInChange : MonoBehaviour
{
    [Tooltip("Identificador único para evitar conflictos si tienes varios objetos persistentes.")]
    [SerializeField] private string uniqueID;

    private static System.Collections.Generic.Dictionary<string, GameObject> instances = new System.Collections.Generic.Dictionary<string, GameObject>();

    private void Awake()
    {
        // Si no se asignó un ID, usamos el nombre del objeto base del prefab
        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = gameObject.name;
        }

        // Comprobamos si ya existe una instancia previa de este prefab en memoria
        if (instances.ContainsKey(uniqueID) && instances[uniqueID] != null)
        {
            // Si ya existe, destruimos este nuevo duplicado para mantener el original
            Destroy(gameObject);
            return;
        }

        // Si es el primero, lo guardamos en el diccionario y evitamos que se destruya
        instances[uniqueID] = gameObject;
        DontDestroyOnLoad(gameObject);
    }
}
