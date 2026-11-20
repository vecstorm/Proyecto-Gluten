using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public static class ParticleManager
{
    private static Dictionary<string, ParticleSystem> particleDictionary = new Dictionary<string, ParticleSystem>();

    static ParticleManager()
    {
        List<ParticleSystem> particleResources = Resources.LoadAll<ParticleSystem>("Particles").ToList();

        foreach (ParticleSystem particle in particleResources)
        {
            particleDictionary.Add(particle.name, particle);
        }
    }

    public static void SpawnParticles(string name, Transform transform)
    {
        if (particleDictionary.ContainsKey(name))
        {
            Object.Instantiate(particleDictionary[name], transform);
        }
        else
        {
            Debug.LogError("No existen particulas con el identificador " + name);
        }
    }

    public static void SpawnParticles(string name, Vector3 position, Quaternion rotation)
    {
        if (particleDictionary.ContainsKey(name))
        {
            Object.Instantiate(particleDictionary[name], position, rotation);
        }
        else
        {
            Debug.LogError("No existen particulas con el identificador " + name);
        }
    }

    public static void SpawnParticles(string name, Vector3 position, Vector3 direction)
    {
        if (particleDictionary.ContainsKey(name))
        {
            Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction);

            Object.Instantiate(particleDictionary[name], position, rotation);
        }
        else
        {
            Debug.LogError("No existen particulas con el identificador " + name);
        }
    }

    public static void SpawnParticlesOnSurface(string name, Transform transform, Vector3 surfaceDirection, LayerMask layerMask)
    {
        if (particleDictionary.ContainsKey(name))
        {
            //Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, surfaceDirection, 5f, layerMask);

            if (hit.collider)
            {
                Object.Instantiate(particleDictionary[name], hit.point, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogError("No existen particulas con el identificador " + name);
        }
    }

    public static void SpawnParticlesOnSurface(string name, Transform transform, Vector3 surfaceDirection, Vector3 particleDirection, LayerMask layerMask)
    {
        if (particleDictionary.ContainsKey(name))
        {
            Quaternion rotation = Quaternion.FromToRotation(Vector3.right, particleDirection);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, surfaceDirection, 5f, layerMask);

            if (hit.collider)
            {
                Object.Instantiate(particleDictionary[name], hit.point, rotation);
            }
        }
        else
        {
            Debug.LogError("No existen particulas con el identificador " + name);
        }
    }

    public static void StopAllParticles()
    {
        foreach (ParticleSystem particle in Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.InstanceID))
        {
            particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    public static void DeleteAllParticles()
    {
        foreach (ParticleSystem particle in Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.InstanceID))
        {
            Object.Destroy(particle.gameObject);
        }
    }
}
