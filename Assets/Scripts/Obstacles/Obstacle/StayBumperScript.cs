
using UnityEngine;

public class StayBumperScript : MonoBehaviour
{
    [SerializeField] private float bounceForce = 15f;

    private Collider bumperCollider;

    private void Awake()
    {
        bumperCollider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Comprobamos si hemos chocado con un jugador
        if (!collision.gameObject.TryGetComponent<PlayerController>(out PlayerController player))
            return;

        Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();

        if (playerRb == null) return;

        // Obtenemos el centro del jugador y el punto mas cercano del bumper
        Vector3 playerCenter = playerRb.worldCenterOfMass;
        Vector3 closestPoint = bumperCollider.ClosestPoint(playerCenter);

        // Calculamos la dirección desde el bumper hacia el jugador
        Vector3 direction = playerCenter - closestPoint;

        // Comprobamos que exista una dirección válida
        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        // Eliminamos la velocidad para que se vea el impulso
        playerRb.linearVelocity = Vector3.zero;

        // Aplicamos el impulso
        playerRb.AddForce(direction * bounceForce, ForceMode.Impulse);
    }
}
