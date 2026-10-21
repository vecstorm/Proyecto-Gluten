using Unity.Netcode;
using UnityEngine;

public class StayBumperScript : NetworkBehaviour
{
    [SerializeField] private float bounceForce = 15f;

    private Collider bumperCollider;

    private void Awake()
    {
        bumperCollider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        // Comprobamos si hemos chocado con un jugador
        if (!collision.gameObject.TryGetComponent<PlayerController>(out PlayerController player))
            return;

        Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();

        if (playerRb == null) return;

        // Lo que hacemos es que el jugador una vez que colisione con el objeto
        // Entonces tendra el impulso hacia ateras
        Vector3 playerCenter = playerRb.worldCenterOfMass;
        Vector3 closestPoint = bumperCollider.ClosestPoint(playerCenter);

        // Calculamos la dirección desde el bumper hacia el jugador
        Vector3 direction = playerCenter - closestPoint;

        // Si choca el player ira impulsado a la direcion contraria
        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        // Eliminamos la velocidad para que se vea el impulso
        playerRb.linearVelocity = Vector3.zero;

        // Aplicamos el impulso
        playerRb.AddForce(direction * bounceForce, ForceMode.Impulse);
    }
}