using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager instance;

    private Dictionary<string, ParticleSystem> particleDictionary = new Dictionary<string, ParticleSystem>();

    [Header("Particle Prefabs")]
    [SerializeField] private List<ParticleSystem> particles;

    private void Awake()
    {
        instance = this;

        foreach (ParticleSystem particle in particles)
        {
            particleDictionary.Add(particle.name, particle);
        }
    }

    public void SpawnParticles(string name, Transform transform)
    {
        if (particleDictionary.ContainsKey(name))
        {
            Instantiate(particleDictionary[name], transform);
        }
        else
        {
            Debug.LogError("No existen particulas con el identificador " + name);
        }
    }

    public void SpawnParticles(string name, Vector3 position, Quaternion rotation)
    {
        if (particleDictionary.ContainsKey(name))
        {
            Instantiate(particleDictionary[name], position, rotation);
        }
        else
        {
            Debug.LogError("No existen particulas con el identificador " + name);
        }
    }

    public void StopAllParticles()
    {
        foreach (ParticleSystem particle in FindObjectsByType<ParticleSystem>(FindObjectsSortMode.InstanceID))
        {
            particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    public void DeleteAllParticles()
    {
        foreach (ParticleSystem particle in FindObjectsByType<ParticleSystem>(FindObjectsSortMode.InstanceID))
        {
            Destroy(particle.gameObject);
        }
    }
}
