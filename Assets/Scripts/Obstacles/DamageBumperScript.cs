using UnityEngine;

public class DamageBumperScript : MonoBehaviour
{
    [SerializeField] private float damageAmount = 1f;
    [SerializeField] private float bounceForce = 15f;

    private Collider bumperCollider;

    private void Awake()
    {
        bumperCollider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Lo que hacemos es comprobar si hemos colisionado con el jugador
        if (!collision.gameObject.TryGetComponent<PlayerController>(out PlayerController player))
            return;

        // Si es correto entonces aplicaremos el daño
        CharacterStats characterStats = collision.gameObject.GetComponent<CharacterStats>();
        if (characterStats != null)
        {
            characterStats.TakeDamage(damageAmount);
            Debug.Log("El jugador recibe " + damageAmount + " de daño");
        }

        Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();

        if (playerRb == null) return;

        // Obtenemos el centro del player y el punto mas cercano del bumper
        Vector3 playerCenter = playerRb.worldCenterOfMass;
        Vector3 closestPoint = bumperCollider.ClosestPoint(playerCenter);

        // Calculamos la dirección desde el bumper hacia el jugador
        Vector3 direction = playerCenter - closestPoint;

        // Comprobamos queeste kla direccion valida
        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        // Eliminamos la velocidad para que se vea el impulso
        playerRb.linearVelocity = Vector3.zero;

        // Aplicamos el impulso
        playerRb.AddForce(direction * bounceForce, ForceMode.Impulse);
    }
}