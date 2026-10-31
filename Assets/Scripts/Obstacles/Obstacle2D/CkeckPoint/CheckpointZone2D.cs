
using UnityEngine;

public class CheckpointZone2D : MonoBehaviour
{
    public static Vector2 lastSafePosition;
    public static bool hasSafePoint = false;

    private static float maxPlayerXPosition = float.MinValue;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player != null)
        {
            if (transform.position.x > maxPlayerXPosition)
            {
                maxPlayerXPosition = transform.position.x;
                lastSafePosition = transform.position;
                hasSafePoint = true;

                Debug.Log("Advanced checkpoint saved at: " + lastSafePosition);
            }
            else
            {
                lastSafePosition = transform.position;
                hasSafePoint = true;
                Debug.Log("Checkpoint updated at: " + lastSafePosition);
            }
        }
    }
}
