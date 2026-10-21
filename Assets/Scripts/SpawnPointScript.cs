using Unity.Netcode;
using UnityEngine;

public class PlayerRespawnScript : NetworkBehaviour
{
    private Rigidbody rb;
    private static Vector3 RespawnPaintPlayer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        rb = GetComponent<Rigidbody>();

        // Aqui solo iniciara el servidor de la reaparición
        if (IsServer)
        {
            GameObject spawnCube = GameObject.FindWithTag("SpawnCube");
            if (spawnCube != null)
            {
                RespawnPaintPlayer = spawnCube.transform.position;
            }
            else
            {
                // En caso que no lo encunetre entonces respaunea en este lugar especifico
                RespawnPaintPlayer = new Vector3(0f, 1f, 0f); 
            }
        }
    }
}