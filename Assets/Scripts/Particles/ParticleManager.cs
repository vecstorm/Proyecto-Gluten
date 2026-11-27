using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ParticleManager
{
    private static Dictionary<string, ParticleSystem> particleDictionary = new Dictionary<string, ParticleSystem>();

    // Registra en el diccionario todos los prefab de particulas que haya en la carpeta Resources/Particles
    static ParticleManager()
    {
        List<ParticleSystem> particleResources = Resources.LoadAll<ParticleSystem>("Particles").ToList();

        foreach (ParticleSystem particle in particleResources)
        {
            particleDictionary.Add(particle.name, particle);
        }
    }

    // Comprueba si existe alguna particula en el diccionario con el nombre establecido
    private static bool CheckDictionary(string name)
    {
        if (particleDictionary.ContainsKey(name))
        {
            return true;
        }
        else
        {
            Debug.LogError("No existen particulas con el identificador " + name);
            return false;
        }
    }

    /// <summary>
    /// Instancia un prefab de particulas como hijo de un padre específico.
    /// </summary>
    /// <param name="name">Nombre del prefab de particulas</param>
    /// <param name="transform">Padre de las particulas a instanciar</param>
    public static void SpawnParticles(string name, Transform transform)
    {
        if (!CheckDictionary(name)) return;

        Object.Instantiate(particleDictionary[name], transform);
    }

    /// <summary>
    /// Instancia un prefab de particulas como hijo de un padre específico, rotado en torno una dirección.
    /// </summary>
    /// <param name="name">Nombre del prefab de particulas</param>
    /// <param name="transform">Padre de las particulas a instanciar</param>
    /// <param name="direction">Dirección en la que rotar el prefab</param>
    public static void SpawnParticles(string name, Transform transform, Vector3 direction)
    {
        if (!CheckDictionary(name)) return;

        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction);

        ParticleSystem particle = Object.Instantiate(particleDictionary[name], transform);
        particle.transform.localRotation = rotation;
    }

    /// <summary>
    /// Instancia un prefab de particulas como hijo de un padre específico, rotado en torno una dirección
    /// y colocado en una posición específica.
    /// </summary>
    /// <param name="name">Nombre del prefab de particulas</param>
    /// <param name="transform">Padre de las particulas a instanciar</param>
    /// <param name="position">Posición donde instanciar el prefab</param>
    /// <param name="direction">Dirección en la que rotar el prefab</param>
    public static void SpawnParticles(string name, Transform transform, Vector3 position, Vector3 direction)
    {
        if (!CheckDictionary(name)) return;

        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction);

        ParticleSystem particle = Object.Instantiate(particleDictionary[name], transform);
        particle.transform.position = position;
        particle.transform.localRotation = rotation;
    }

    /// <summary>
    /// Instancia un prefab de particulas con una rotación y posición especificos.
    /// </summary>
    /// <param name="name">Nombre del prefab de particulas</param>
    /// <param name="position">Posición donde instanciar el prefab</param>
    /// <param name="rotation">Rotación que tendrá el prefab</param>
    public static void SpawnParticles(string name, Vector3 position, Quaternion rotation)
    {
        if (!CheckDictionary(name)) return;

        Object.Instantiate(particleDictionary[name], position, rotation);
    }

    /// <summary>
    /// Instancia un prefab de particulas con una posición especifica y rotado en torno una dirección.
    /// </summary>
    /// <param name="name">Nombre del prefab de particulas</param>
    /// <param name="position">Posición donde instanciar el prefab</param>
    /// <param name="direction">Dirección en la que rotar el prefab</param>
    public static void SpawnParticles(string name, Vector3 position, Vector3 direction)
    {
        if (!CheckDictionary(name)) return;

        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction);

        Object.Instantiate(particleDictionary[name], position, rotation);
    }

    /// <summary>
    /// Instancia un prefab de particulas con una posición especifica y flipeado en torno una dirección.
    /// </summary>
    /// <param name="name">Nombre del prefab de particulas</param>
    /// <param name="transform">Padre de las particulas a instanciar</param>
    /// <param name="facingDirection">Dirección en la que flipear el prefab</param>
    public static void SpawnParticles(string name, Transform transform, int facingDirection)
    {
        if (!CheckDictionary(name)) return;

        ParticleSystem particle = Object.Instantiate(particleDictionary[name], transform);

        ParticleSystemRenderer renderer = particle.GetComponent<ParticleSystemRenderer>();

        if (renderer != null)
        {
            renderer.flip = new Vector3(facingDirection < 0 ? 1f : 0f, 0f, 0f);
        }
    }

    /// <summary>
    /// Instancia un prefab de particulas con una posición especifica y flipeado en torno una dirección.
    /// </summary>
    /// <param name="name">Nombre del prefab de particulas</param>
    /// <param name="position">Posición donde instanciar el prefab</param>
    /// <param name="direction">Dirección en la que rotar el prefab</param>
    /// <param name="facingDirection">Dirección en la que flipear el prefab</param>
    public static void SpawnParticles(string name, Vector3 position, Vector3 direction, int facingDirection)
    {
        if (!CheckDictionary(name)) return;

        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction);

        ParticleSystem particle = Object.Instantiate(particleDictionary[name], position, rotation);

        ParticleSystemRenderer renderer = particle.GetComponent<ParticleSystemRenderer>();

        if (renderer != null)
        {
            renderer.flip = new Vector3(facingDirection < 0 ? 1f : 0f, 0f, 0f);
        }
    }

    /// <summary>
    /// Crea un raycast en dirección a una superficie e instancia un prefab de particulas como hijo de un padre específico 
    /// posicionado en el punto donde el raycast haya colisionado con la superficie.
    /// </summary>
    /// <param name="name">Nombre del prefab de particulas</param>
    /// <param name="transform">Padre de las particulas a instanciar</param>
    /// <param name="surfaceDirection">Dirección donde apuntara el raycast</param>
    /// <param name="layerMask">Layers con los que podra colisionar el raycast</param>
    /// <param name="rayDistance">Distancia de detección del raycast</param>
    public static void SpawnParticlesOnSurface(string name, Transform transform, Vector3 surfaceDirection, LayerMask layerMask, float rayDistance = 2f)
    {
        if (!CheckDictionary(name)) return;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, surfaceDirection, rayDistance, layerMask);

        if (hit.collider)
        {
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            ParticleSystem particle = Object.Instantiate(particleDictionary[name], transform);
            particle.gameObject.transform.position = hit.point;
            particle.gameObject.transform.localRotation = rotation;
        }
    }

    /// <summary>
    /// Crea un raycast en dirección a una superficie e instancia un prefab de particulas como hijo de un padre específico 
    /// posicionado en el punto donde el raycast haya colisionado con la superficie y rotado en torno una dirección.
    /// </summary>
    /// <param name="name">Nombre del prefab de particulas</param>
    /// <param name="transform">Padre de las particulas a instanciar</param>
    /// <param name="surfaceDirection">Dirección donde apuntara el raycast</param>
    /// <param name="particleDirection">Dirección en la que rotar el prefab</param>
    /// <param name="layerMask">Layers con los que podra colisionar el raycast</param>
    /// <param name="rayDistance">Distancia de detección del raycast</param>
    public static void SpawnParticlesOnSurface(string name, Transform transform, Vector3 surfaceDirection, Vector3 particleDirection, LayerMask layerMask, float rayDistance = 2f)
    {
        if (!CheckDictionary(name)) return;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, surfaceDirection, rayDistance, layerMask);

        if (hit.collider)
        {
            Quaternion rotation = Quaternion.FromToRotation(Vector3.right, particleDirection);

            ParticleSystem particle = Object.Instantiate(particleDictionary[name], transform);
            particle.gameObject.transform.position = hit.point;
            particle.gameObject.transform.localRotation = rotation;
        }
    }

    /// <summary>
    /// Crea un raycast en dirección a una superficie e instancia un prefab de particulas
    /// posicionado en el punto donde el raycast haya colisionado con la superficie y rotado en torno una dirección.
    /// </summary>
    /// <param name="name">Nombre del prefab de particulas</param>
    /// <param name="position">Origen del raycast</param>
    /// <param name="surfaceDirection">Dirección donde apuntara el raycast</param>
    /// <param name="layerMask">Layers con los que podra colisionar el raycast</param>
    /// <param name="rayDistance">Distancia de detección del raycast</param>
    public static void SpawnParticlesOnSurface(string name, Vector3 position, Vector3 surfaceDirection, LayerMask layerMask, float rayDistance = 2f)
    {
        if (CheckDictionary(name))
        {
            RaycastHit2D hit = Physics2D.Raycast(position, surfaceDirection, rayDistance, layerMask);

            if (hit.collider)
            {
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                ParticleSystem particle = Object.Instantiate(particleDictionary[name], hit.point, rotation);
            }
        }
    }

    /// <summary>
    /// Crea un raycast en dirección a una superficie e instancia un prefab de particulas
    /// posicionado en el punto donde el raycast haya colisionado con la superficie y rotado en torno una dirección.
    /// </summary>
    /// <param name="name">Nombre del prefab de particulas</param>
    /// <param name="position">Origen del raycast</param>
    /// <param name="surfaceDirection">Dirección donde apuntara el raycast</param>
    /// <param name="particleDirection">Dirección en la que rotar el prefab</param>
    /// <param name="layerMask">Layers con los que podra colisionar el raycast</param>
    /// <param name="rayDistance">Distancia de detección del raycast</param>
    public static void SpawnParticlesOnSurface(string name, Vector3 position, Vector3 surfaceDirection, Vector3 particleDirection, LayerMask layerMask, float rayDistance = 2f)
    {
        if (CheckDictionary(name))
        {
            RaycastHit2D hit = Physics2D.Raycast(position, surfaceDirection, rayDistance, layerMask);

            if (hit.collider)
            {
                Quaternion rotation = Quaternion.FromToRotation(Vector3.right, particleDirection);
                ParticleSystem particle = Object.Instantiate(particleDictionary[name], hit.point, rotation);
            }
        }
    }

    /// <summary>
    /// Termina la emision de todas las particulas en la escena.
    /// </summary>
    public static void StopAllParticles()
    {
        foreach (ParticleSystem particle in Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.InstanceID))
        {
            particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    /// <summary>
    /// Destruye los gameobjects de todas las particulas en la escena.
    /// </summary>
    public static void DeleteAllParticles()
    {
        foreach (ParticleSystem particle in Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.InstanceID))
        {
            Object.Destroy(particle.gameObject);
        }
    }

    /// <summary>
    /// Devuelve un sistema de particulas específico.
    /// </summary>
    /// <param name="name">Nombre del prefab de particulas</param>
    public static ParticleSystem GetParticles(string name)
    {
        if (!CheckDictionary(name)) return null;
        
        return particleDictionary[name];
    }
}
