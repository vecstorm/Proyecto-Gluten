using UnityEngine;

public class RectangleRotateScript : MonoBehaviour
{
    public float rotationSpeed = 90f;

    private void Update()
    {
        // Rota el objeto de forma constante en el eje Z (plano 2D)
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}