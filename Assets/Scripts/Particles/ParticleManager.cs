using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
