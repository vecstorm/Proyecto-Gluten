using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState { Idle, Walk, Run, Jump, Airborne, DoubleJump, Dash, WallGrab, WallSlide, WallJump, Hook, Crouch, Attack, Defend, Parry, Hurt, Dead }

    [SerializeField] private PlayerState currentState = PlayerState.Idle;
    public PlayerState CurrentState => currentState;


    private void Start()
    {
        EnterState(currentState);
    }

    private void Update()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                Idle();
                break;

            case PlayerState.Walk:
                Walk();
                break;

            case PlayerState.Run:
                Run();
                break;

            case PlayerState.Jump:
                Jump();
                break;

            case PlayerState.Airborne:
                Airborne();
                break;

            case PlayerState.DoubleJump:
                DoubleJump();
                break;

            case PlayerState.Dash:
                Dash();
                break;

            case PlayerState.WallGrab:
                WallGrab();
                break;

            case PlayerState.WallSlide:
                WallSlide();
                break;

            case PlayerState.WallJump:
                WallJump();
                break;

            case PlayerState.Hook:
                Hook();
                break;

            case PlayerState.Crouch:
                Crouch();
                break;

            case PlayerState.Attack:
                Attack();
                break;

            case PlayerState.Defend:
                Defend();
                break;

            case PlayerState.Parry:
                Parry();
                break;

            case PlayerState.Hurt:
                Hurt();
                break;

            case PlayerState.Dead:
                Dead();
                break;




        }
    }

    public bool ChangeState(PlayerState newState)
    {
        if (newState == currentState) //Si el nuevo estado es el mismo al estado actual, no hace nada
            return false;

        if (!IsTransitionAllowed(currentState, newState)) //SI la transicion no esta permitida, avisa
        {
            Debug.LogWarning($"Transición no permitida: {currentState} -> {newState}", this);
            return false;
        }

        ExitState(currentState); //Limpia acciones del estado anterior
        currentState = newState; //Actualiza el estado
        EnterState(currentState); //Entra en el nuevo estado
        return true;

    }

    //Metodo que comprueba 
    public bool IsTransitionAllowed(PlayerState from, PlayerState to)
    {
        if (from == to) //Para evitar que el personaje cambie a un estado el cual ya esta
            return false;


        //Desde cualquier estado excepto Hurt y Dead, el jugador puede pasar a Hurt o Dead
        ///Ejemplo: no puede pasar de dead a hurt porque en esta condicion no esta permitido
        if (from != PlayerState.Hurt && from != PlayerState.Dead && (to == PlayerState.Hurt || to == PlayerState.Dead))
            return true;

        //Aqui esta la lista de estados a los cuales desde uno inicial se puede pasar a otro.
        switch (from)
        {
            case PlayerState.Idle:
                return IsOneOf(to,
                    PlayerState.Walk, PlayerState.Run, PlayerState.Jump,
                    PlayerState.Dash, PlayerState.Crouch, PlayerState.Attack,
                    PlayerState.Defend, PlayerState.Hook);

            case PlayerState.Walk:
                return IsOneOf(to,
                    PlayerState.Idle, PlayerState.Run, PlayerState.Jump,
                    PlayerState.Dash, PlayerState.Crouch, PlayerState.Attack,
                    PlayerState.Defend, PlayerState.Hook);

            case PlayerState.Run:
                return IsOneOf(to,
                    PlayerState.Idle, PlayerState.Walk, PlayerState.Jump,
                    PlayerState.Dash, PlayerState.Crouch, PlayerState.Attack,
                    PlayerState.Defend, PlayerState.Hook);

            case PlayerState.Jump:
                return IsOneOf(to,
                    PlayerState.Airborne, PlayerState.DoubleJump,
                    PlayerState.Dash, PlayerState.WallGrab, PlayerState.Hook,
                    PlayerState.Attack);

            case PlayerState.Airborne:
                return IsOneOf(to,
                    PlayerState.Idle, PlayerState.Walk, PlayerState.Run,
                    PlayerState.DoubleJump, PlayerState.Dash,
                    PlayerState.WallGrab, PlayerState.WallSlide,
                    PlayerState.Hook, PlayerState.Attack);

            case PlayerState.DoubleJump:
                return IsOneOf(to,
                    PlayerState.Airborne, PlayerState.Dash,
                    PlayerState.WallGrab, PlayerState.Hook,
                    PlayerState.Attack);

            case PlayerState.Dash:
                return IsOneOf(to,
                    PlayerState.Idle, PlayerState.Walk, PlayerState.Run,
                    PlayerState.Airborne, PlayerState.WallGrab,
                    PlayerState.Hook);

            case PlayerState.WallGrab:
                return IsOneOf(to,
                    PlayerState.WallSlide, PlayerState.WallJump,
                    PlayerState.Airborne, PlayerState.Hook, PlayerState.Dash);

            case PlayerState.WallSlide:
                return IsOneOf(to,
                    PlayerState.WallJump, PlayerState.WallGrab,
                    PlayerState.Airborne, PlayerState.Hook, PlayerState.Dash);

            case PlayerState.WallJump:
                return IsOneOf(to,
                    PlayerState.Airborne, PlayerState.Dash,
                    PlayerState.DoubleJump, PlayerState.WallGrab,
                    PlayerState.Hook, PlayerState.Attack);

            case PlayerState.Hook:
                return IsOneOf(to,
                    PlayerState.Idle, PlayerState.Walk, PlayerState.Run,
                    PlayerState.Airborne, PlayerState.WallGrab,
                    PlayerState.Dash, PlayerState.Attack);

            case PlayerState.Crouch:
                return IsOneOf(to,
                    PlayerState.Idle, PlayerState.Walk, PlayerState.Attack,
                    PlayerState.Hook, PlayerState.Dash);

            case PlayerState.Attack:
                return IsOneOf(to,
                    PlayerState.Idle, PlayerState.Walk, PlayerState.Run,
                    PlayerState.Airborne, PlayerState.Crouch,
                    PlayerState.Defend, PlayerState.Parry,
                    PlayerState.Hook, PlayerState.Dash);

            case PlayerState.Defend:
                return IsOneOf(to,
                    PlayerState.Idle, PlayerState.Walk, PlayerState.Parry);

            case PlayerState.Parry:
                return IsOneOf(to,
                    PlayerState.Defend, PlayerState.Idle,
                    PlayerState.Walk, PlayerState.Attack);

            case PlayerState.Hurt:
                return IsOneOf(to,
                    PlayerState.Idle, PlayerState.Walk, PlayerState.Run,
                    PlayerState.Airborne, PlayerState.WallGrab,
                    PlayerState.WallSlide, PlayerState.Dead);

            case PlayerState.Dead:
                //Se permite volver a idle despues de reaparecer en un checkpoint
                return to == PlayerState.Idle;

            default:
                return false;
        }
    }

    //Metodo que comprueba si un estado esta en la lista de estados permitidos
    private bool IsOneOf(PlayerState value, params PlayerState[] options)
    {
        foreach (PlayerState option in options) //Devuelve true siempre que los estados que se escojan esten dentro de la lista
        {
            if (value == option)
                return true;
        }

        return false; //Sino devuelve false
    }

    private void EnterState(PlayerState state)
    {
        //Preparar el estado al entrar: por ejemplo, iniciar un temporizador o activar una animación.
    }

    private void ExitState(PlayerState state)
    {
        // Limpiar el estado al salir: por ejemplo, cancelar un temporizador o detener una acción.
    }

    //METER TODA LA LOGIDA DENTRO DE LOS METODOS DE ABAJO

    private void Dead()
    {
        throw new NotImplementedException();
    }

    private void Hurt()
    {
        throw new NotImplementedException();
    }

    private void Parry()
    {
        throw new NotImplementedException();
    }

    private void Defend()
    {
        throw new NotImplementedException();
    }

    private void Attack()
    {
        throw new NotImplementedException();
    }

    private void Crouch()
    {
        throw new NotImplementedException();
    }

    private void Hook()
    {
        throw new NotImplementedException();
    }

    private void WallJump()
    {
        throw new NotImplementedException();
    }

    private void WallSlide()
    {
        throw new NotImplementedException();
    }

    private void WallGrab()
    {
        throw new NotImplementedException();
    }

    private void Dash()
    {
        throw new NotImplementedException();
    }

    private void DoubleJump()
    {
        throw new NotImplementedException();
    }

    private void Airborne()
    {
        throw new NotImplementedException();
    }

    private void Jump()
    {
        throw new NotImplementedException();
    }

    private void Run()
    {
        throw new NotImplementedException();
    }

    private void Walk()
    {
        throw new NotImplementedException();
    }

    private void Idle()
    {
        throw new NotImplementedException();
    }

    
}
