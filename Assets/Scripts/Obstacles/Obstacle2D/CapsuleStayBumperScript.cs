using UnityEngine;

public class CapsuleStayBumperScript : MonoBehaviour
{
    [SerializeField] private float bounceForce = 15f;

    private Collider2D bumperCollider;

    private void Awake()
    {
        bumperCollider = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Comprobamos si hemos chocado con un jugador
        if (!collision.gameObject.TryGetComponent<PlayerController>(out PlayerController player))
            return;

        Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();

        if (playerRb == null) return;

        // Obtenemos el centro del jugador y el punto mas cercano del bumper
        Vector2 playerCenter = playerRb.worldCenterOfMass;
        Vector2 closestPoint = bumperCollider.ClosestPoint(playerCenter);

        // Calculamos la dirección desde el bumper hacia el jugador
        Vector2 direction = playerCenter - closestPoint;

        // Comprobamos que exista una dirección válida
        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        // Eliminamos la velocidad para que se vea el impulso
        playerRb.linearVelocity = Vector2.zero;

        // Aplicamos el impulso
        playerRb.AddForce(direction * bounceForce, ForceMode2D.Impulse);
    }
}