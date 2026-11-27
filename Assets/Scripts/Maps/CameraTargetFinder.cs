using Unity.Cinemachine;
using UnityEngine;

public class CameraTargetFinder : MonoBehaviour
{
    private CinemachineCamera camera;

    private void Awake()
    {
        camera = GetComponent<CinemachineCamera>();
    }

    private void OnEnable()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if(player != null)
        {
            camera.Target.TrackingTarget = player.transform;
        }
    }
}
