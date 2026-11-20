using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
public class FinalBossHand : MonoBehaviour
{
    [SerializeField] private Transform originPoint;
    [SerializeField] private float returningSpeed = 20f;
    [SerializeField] private float offsetSpeed = 3f;
    
    private float movementOffset;
    private float incomingOffset;

    private FinalBossEnemyAI ownerBoss;

    private bool isBusy = false;

    private Rigidbody2D rb;

    private bool armCollided = false;
    private bool onOrigin = false;
    private bool colorFlashing = false;

    private CapsuleCollider2D damageCollider;

    private Vector3 spawnRotation;

    private float safeTime = 0.3f;
    private float timer;

    public bool ArmCollided => armCollided;

    public bool OnOrigin => onOrigin;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        spawnRotation = transform.rotation.eulerAngles;

        damageCollider = GetComponent<CapsuleCollider2D>();
    }

    private void FixedUpdate()
    {
        if (!isBusy)
        {
            movementOffset = Mathf.Lerp(movementOffset, incomingOffset, offsetSpeed * Time.fixedDeltaTime);

            Vector3 targetPosition = originPoint.position + Vector3.right * movementOffset;

            rb.MovePosition(Vector3.MoveTowards(transform.position, targetPosition, returningSpeed * Time.fixedDeltaTime));

            if (Vector3.Distance(transform.position, originPoint.position) <= 0.01f)
            {
                onOrigin = true;
            }
        }

        timer -= Time.fixedDeltaTime;

        if (timer <= 0)
        {
            timer = 0;
        }
    }

    // Da valor al boss dueño del brazo
    public void SetOwnerBoss(FinalBossEnemyAI boss)
    {
        ownerBoss = boss;
    }

    // Logica de retorno al punto de origen del brazo
    public void ReturnToOrigin()
    {
        armCollided = false;
        onOrigin = false;
        isBusy = false;
    }

    // Resetea la rotacion del brazo
    public void ResetRotation()
    {
        transform.rotation = Quaternion.Euler(spawnRotation);
    }

    // Logica de reseteo del brazo
    public void ResetHand()
    {
        rb.linearVelocity = Vector3.zero;

        ReturnToOrigin();
        ResetRotation();
    }

    // Da valor al safe time del brazo
    public void SetSafeTime(float safeTime)
    {
        this.safeTime = safeTime;
    }

    // Mueve el brazo hacia una posicion especifica
    public void MoveToPosition(Vector3 targetPosition, float speed)
    {
        isBusy = true;
        rb.MovePosition(Vector3.MoveTowards(transform.position, targetPosition, speed * Time.fixedDeltaTime));
    }

    // Mueve el brazo en una direccion especifica
    public void MoveToDirection(Vector3 targetDirection, float speed)
    {
        isBusy = true;
        rb.linearVelocity = targetDirection * speed;
    }

    // Mueve el brazo hasta que haya colisionado con algo
    public void MoveTillCollision(Vector3 targetDirection, float speed)
    {
        if (armCollided)
        {
            Debug.Log("COLLIDED AND STOPPED");
            return;
        }

        isBusy = true;

        rb.linearVelocity = targetDirection * speed;
    }

    public void BeginSafeTime()
    {
        timer = safeTime;
    }

    // Activa o desactiva el collider del brazo
    public void SetColliderActive(bool value)
    {
        damageCollider.enabled = value;
    }

    // Da valor al offset de movimiento del brazo
    public void SetMovementOffset(float offset)
    {
        incomingOffset = offset;
    }

    // Si colisiona con el player, llama a la funcion que lo daña desde el script del jefe final
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            if (ownerBoss != null && isBusy && timer <= 0)
            {
                ownerBoss.ArmHitPlayer(transform, collision);
            }
        }
    }

    // Detecta si el brazo ha colisionado contra algo
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Ground") && isBusy)
        {
            ContactPoint2D contact = collision.GetContact(0);

            if (Mathf.Abs(contact.normal.y) > 0.5f)
            {
                Debug.Log("Arm: " + gameObject.name + " has collided with: " + collision.gameObject.name);
                armCollided = true;
            }
        }
    }
}
