using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Creamos un Rigidbody para controlar las físicas del jugador
    private Rigidbody rb;

    // Creamos la velocidad de movimiento del jugador
    [SerializeField] private float speed = 5f;

    // Guardamos la dirección de movimiento introducida por el jugador
    private Vector2 inputVector;

    // Creamos la fuerza del salto
    [SerializeField] private float jumpForce = 6f;

    // Creamos la variable del sonido del salto
    private AudioSource jumpSound;

    private void Awake()
    {
        // Buscamos el Rigidbody del jugador
        rb = GetComponent<Rigidbody>();

        // Buscamos el sonido del salto
        jumpSound = GetComponent<AudioSource>();
    }

    private void Update()
    {
        float h = 0f;
        float v = 0f;

        // Comprobamos las teclas de movimiento
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                h -= 1f;

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                h += 1f;

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                v -= 1f;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                v += 1f;
        }

        // Guardamos la dirección de movimiento
        inputVector = new Vector2(h, v).normalized;

        // Comprobamos si el jugador pulsa la tecla de salto
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        // Calculamos la dirección del movimiento
        Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);

        // Aplicamos la velocidad sin modificar la velocidad vertical
        Vector3 targetVelocity = moveDirection * speed;

        rb.linearVelocity = new Vector3(
            targetVelocity.x,
            rb.linearVelocity.y,
            targetVelocity.z
        );

        // Giramos al jugador hacia la dirección del movimiento
        if (moveDirection != Vector3.zero)
        {
            transform.LookAt(transform.position + moveDirection);
        }

        // Evitamos que el jugador gire por las físicas
        rb.angularVelocity = Vector3.zero;
    }

    private void Jump()
    {
        // Comprobamos si el jugador está tocando el suelo
        if (!Physics.Raycast(transform.position, Vector3.down, 1.1f))
            return;

        // Aplicamos la fuerza del salto
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        // Reproducimos el sonido del salto
        if (jumpSound != null)
        {
            jumpSound.Play();
        }
    }
}