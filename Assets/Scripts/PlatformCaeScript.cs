using Unity.Netcode;
using UnityEngine;

public class PlataformaFragil : NetworkBehaviour
{
    // Segundos desde que el jugador la pisa hasta que se rompe
    [SerializeField] private float tiempoRotura = 2f;

    // Segundos desde que se rompe hasta que reaparece
    [SerializeField] private float tiempoReaparicion = 6f;

    // Variable sincronizada: true = plataforma rota.
    // Todos la pueden leer (Everyone) y solo el servidor la puede escribir (Server).
    private NetworkVariable<bool> rota = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    // Referencias a los componentes
    private Rigidbody rb;
    private Collider col;
    private Renderer rend;

    // Será true cuando alguien ya la ha pisado
    private bool pisada;

    // Cuenta atrás en segundos (solo la usa el servidor)
    private float temporizador;

    private void Awake()
    {
        //Asignación de referencias a los componentes
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Kinematic: la plataforma se queda fija, la gravedad y los choques no la mueven
        rb.isKinematic = true;

        // Nos suscribimos: cuando "rota" cambie, se ejecuta AlCambiarRota
        rota.OnValueChanged += AlCambiarRota;

        // También aplicamos el valor actual al aparecer, por si el cliente entra cuando ya está rota
        Aplicar(rota.Value);
    }

    public override void OnNetworkDespawn()
    {
        // Nos desuscribimos al terminar
        rota.OnValueChanged -= AlCambiarRota;
    }

    // Se ejecuta en TODAS las máquinas (servidor y clientes) cuando cambia "rota"
    private void AlCambiarRota(bool valorAnterior, bool valorNuevo)
    {
        Aplicar(valorNuevo);
    }

    // Apaga o enciende el collider (lo que se pisa) y el renderer (lo que se ve)
    private void Aplicar(bool estaRota)
    {
        col.enabled = !estaRota;
        rend.enabled = !estaRota;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // REGLA DE ORO DE NETCODE: solo el SERVIDOR decide lo que pasa
        if (!IsServer) return;

        // Si ya está pisada, no hacemos nada más
        if (pisada) return;

        // ¿Lo que nos ha tocado es un jugador?
        PlayerController jugador = collision.collider.GetComponentInParent<PlayerController>();
        if (jugador == null) return;

        // Empieza la cuenta atrás hasta la rotura
        pisada = true;
        temporizador = tiempoRotura;
    }

    private void FixedUpdate()
    {
        // REGLA DE ORO DE NETCODE: solo el SERVIDOR calcula la lógica
        if (!IsServer) return;

        // Si nadie la ha pisado, no hay nada que contar
        if (!pisada) return;

        // Restamos el tiempo que ha pasado desde el último FixedUpdate
        temporizador -= Time.fixedDeltaTime;

        // Si todavía queda tiempo, seguimos esperando
        if (temporizador > 0f) return;

        if (!rota.Value)
        {
            // Se acabó la espera: se rompe y empieza la cuenta atrás de la reaparición
            rota.Value = true;
            temporizador = tiempoReaparicion;
        }
        else
        {
            // Se acabó la reaparición: vuelve y se puede pisar de nuevo
            rota.Value = false;
            pisada = false;
        }
    }
}