using UnityEngine;

public class InfiniteRotation : MonoBehaviour
{
    [SerializeField] private float speed = 6f;

    void Update()
    {
        transform.Rotate(0, 0, speed * Time.deltaTime);
    }
}
