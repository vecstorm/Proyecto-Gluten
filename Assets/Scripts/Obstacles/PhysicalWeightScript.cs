using UnityEngine;

public class PhysicalWeightScript : MonoBehaviour
{
    private Rigidbody rb;
    private Transform player;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    void Update()
    {
        if (!rb.isKinematic) return;

        if (player != null)
        {
            // Detecta si el jugador está a la derecha o izquierda del centro
            float direction = player.position.x < transform.position.x ? 1f : -1f;

            // Rota suavemente hacia ese lado en el eje Z
            transform.Rotate(0, 0, direction * 40f * Time.deltaTime);

            // Si se inclina más de 30 grados, pierde el equilibrio y cae al suelo
            float zAngle = transform.eulerAngles.z;
            if (zAngle > 30f && zAngle < 330f)
            {
                rb.isKinematic = false; // ¡Se cae por el peso y la gravedad!
            }
        }
        else
        {
            // Si el jugador se baja, vuelve suavemente a la posición horizontal
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, 0), 30f * Time.deltaTime);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Player")) player = col.transform;
    }

    void OnCollisionExit(Collision col)
    {
        if (col.gameObject.CompareTag("Player")) player = null;
    }
}