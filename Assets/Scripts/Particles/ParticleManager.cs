using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.ParticleSystem;

public static class ParticleManager
{
    private static Dictionary<string, ParticleSystem> particleDictionary = new Dictionary<string, ParticleSystem>();

    private static List<ParticleSystem> persistentParticlesUsed = new List<ParticleSystem>();
    private static List<ParticleSystem> persistentParticles = new List<ParticleSystem>();

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
        if (CheckDictionary(name))
        {
            Object.Instantiate(particleDictionary[name], transform);
        }
    }

    public static void SpawnParticles(string name, Transform transform, Vector3 direction)
    {
        if (CheckDictionary(name))
        {
            Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction);

            ParticleSystem particle = Object.Instantiate(particleDictionary[name], transform);
            particle.transform.localRotation = rotation;
        }
    }

    public static void SpawnParticles(string name, Transform transform, Vector3 position, Vector3 direction)
    {
        if (CheckDictionary(name))
        {
            Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction);

            ParticleSystem particle = Object.Instantiate(particleDictionary[name], transform);
            particle.transform.position = position;
            particle.transform.localRotation = rotation;
        }
    }

    public static void SpawnParticles(string name, Vector3 position, Quaternion rotation)
    {
        if (CheckDictionary(name))
        {
            Object.Instantiate(particleDictionary[name], position, rotation);
        }
    }

    public static void SpawnParticles(string name, Vector3 position, Vector3 direction)
    {
        if (CheckDictionary(name))
        {
            Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction);

            Object.Instantiate(particleDictionary[name], position, rotation);
        }
    }

    public static void SpawnParticlesOnSurface(string name, Transform transform, Vector3 surfaceDirection, LayerMask layerMask, bool isPersistent = false)
    {
        if (CheckDictionary(name))
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, surfaceDirection, 2f, layerMask);

            if (hit.collider)
            {
                if (persistentParticlesUsed.Contains(particleDictionary[name]) && isPersistent)
                {
                    return;
                }

                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                ParticleSystem particle = Object.Instantiate(particleDictionary[name], transform);
                particle.gameObject.transform.position = hit.point;
                particle.gameObject.transform.localRotation = rotation;

                if (isPersistent)
                {
                    persistentParticlesUsed.Add(particleDictionary[name]);
                    persistentParticles.Add(particle);
                }
            }
        }
    }

    public static void SpawnParticlesOnSurface(string name, Transform transform, Vector3 surfaceDirection, Vector3 particleDirection, LayerMask layerMask, bool isPersistent = false)
    {
        if (CheckDictionary(name))
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, surfaceDirection, 2f, layerMask);

            if (hit.collider)
            {
                if (persistentParticlesUsed.Contains(particleDictionary[name]) && isPersistent)
                {
                    return;
                }

                Quaternion rotation = Quaternion.FromToRotation(Vector3.right, particleDirection);

                ParticleSystem particle = Object.Instantiate(particleDictionary[name], transform);
                particle.gameObject.transform.position = hit.point;
                particle.gameObject.transform.localRotation = rotation;

                if (isPersistent)
                {
                    persistentParticlesUsed.Add(particleDictionary[name]);
                    persistentParticles.Add(particle);
                }
            }
        }
    }

    public static void SpawnParticlesOnSurface(string name, Vector3 transformPosition, Vector3 surfaceDirection, LayerMask layerMask, bool isPersistent = false)
    {
        if (CheckDictionary(name))
        {
            RaycastHit2D hit = Physics2D.Raycast(transformPosition, surfaceDirection, 2f, layerMask);

            if (hit.collider)
            {
                if (persistentParticlesUsed.Contains(particleDictionary[name]) && isPersistent)
                {
                    return;
                }

                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                ParticleSystem particle = Object.Instantiate(particleDictionary[name], hit.point, rotation);

                if (isPersistent)
                {
                    persistentParticlesUsed.Add(particleDictionary[name]);
                    persistentParticles.Add(particle);
                }
            }
        }
    }

    public static void SpawnParticlesOnSurface(string name, Transform transform, Vector3 surfaceDirection, Vector3 particleDirection, LayerMask layerMask)
    {
        if (CheckDictionary(name))
        {
            Quaternion rotation = Quaternion.FromToRotation(Vector3.right, particleDirection);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, surfaceDirection, 2f, layerMask);

            if (hit.collider)
            {
                Object.Instantiate(particleDictionary[name], hit.point, rotation);
            }
        }
    }

    public static void StopAllPersistentParticles()
    {
        foreach (ParticleSystem particle in persistentParticles)
        {
            if (particle == null) continue;

            particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        persistentParticlesUsed.Clear();
        persistentParticles.Clear();
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
}
