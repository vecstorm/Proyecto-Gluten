using UnityEngine;

public class RotateScript : MonoBehaviour
{
    public float rotationSpeed = 90f; 

    private void Update()
    {
        // Rota el objeto en el eje X de forma constante
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}