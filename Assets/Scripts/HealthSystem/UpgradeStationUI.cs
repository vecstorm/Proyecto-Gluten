using UnityEngine;

public class UpgradeStationUI : MonoBehaviour
{
    private bool playerNearby = false;

    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Has interactuado con la estación");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            Debug.Log("Estás cerca de la estación");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            Debug.Log("Te has alejado de la estación");
        }
    }
}