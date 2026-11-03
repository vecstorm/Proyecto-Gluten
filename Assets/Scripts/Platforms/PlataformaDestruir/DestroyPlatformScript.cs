using UnityEngine;

public class DestroyPlatformScript : MonoBehaviour
{
    private BoxCollider2D col;
    private float timer;

    void Start()
    {
        col = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        // Si el temporizador está activo, restamos tiempo hasta reactivar el colisionador
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                col.enabled = true;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Comprobamos si el objeto que está encima es el jugador y pulsa la tecla S
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                col.enabled = false; // Desactiva el cubo al instante
                timer = 0.3f;        // Tiempo que tardará en volver a activarse
            }
        }
    }
}