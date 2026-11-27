using NUnit.Framework;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class SceneExitsManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> sceneExits = new List<GameObject>();

    public Transform GetSceneExitPosition(int sceneExitId)
    {
        foreach (GameObject sceneExit in sceneExits)
        {
            if(sceneExit.GetComponent<SceneExit>().GetsceneExitId() == sceneExitId)
            {
                return sceneExit.transform;
            }
        }
        return null;
    }
}
