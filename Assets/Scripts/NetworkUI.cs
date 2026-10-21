using Unity.Netcode;
using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    void OnGUI()
    {
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUI.Button(new Rect(10, 10, 100, 30), "Host"))
                NetworkManager.Singleton.StartHost();

            if (GUI.Button(new Rect(10, 50, 100, 30), "Client"))
                NetworkManager.Singleton.StartClient();
        }
    }
}