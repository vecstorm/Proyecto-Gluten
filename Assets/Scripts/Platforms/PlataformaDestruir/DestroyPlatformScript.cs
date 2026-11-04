using UnityEngine;

public class DestroyPlatformScript : MonoBehaviour
{
    [Header("Tiempos")]
    [Tooltip("Segundos que tarda en desaparecer después de que el jugador la toque")]
    [SerializeField] private float disappearDelay = 3f;

    [Tooltip("Segundos que tarda en volver a aparecer")]
    [SerializeField] private float reappearDelay = 3f;

    // Collider y renderer de la plataforma
    private BoxCollider2D col;
    private Renderer platformRenderer;

    // Temporizador del estado actual
    private float timer;

    // true = esperando para desaparecer
    private bool isDisappearing = false;

    void Start()
    {
        col = GetComponent<BoxCollider2D>();
        platformRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        // Si el jugador la tocó, esperamos 3 segundos y desaparece
        if (isDisappearing)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                SetVisible(false);
                isDisappearing = false;
                timer = reappearDelay;
            }
        }
        // Mientras está desaparecida, esperamos otros 3 segundos y reaparece
        else if (col != null && !col.enabled)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                SetVisible(true);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Solo el jugador y solo si la plataforma está visible
        if (collision.gameObject.CompareTag("Player") && !isDisappearing && col != null && col.enabled)
        {
            timer = disappearDelay;
            isDisappearing = true;
        }
    }

    private void SetVisible(bool visible)
    {
        if (col != null)
            col.enabled = visible;

        // Renderer funciona tanto para SpriteRenderer como para MeshRenderer
        if (platformRenderer != null)
            platformRenderer.enabled = visible;
    }
}
