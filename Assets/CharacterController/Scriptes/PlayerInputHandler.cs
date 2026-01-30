using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{

    //Gestion des Input:
    public static PlayerInputHandler Instance;

    //Stock les valeurs dans des vecteur 2 :
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }

    //Stock dans une bool :
    public bool JumpPressed { get; private set; }

    //Stock la touche interact :
    public bool InteractPressed { get; private set; }

    private PlayerInputActions inputActions;

    private void Awake()
    {
        Instance = this;
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();

        //Définie les valeurs des vecteurs :

        inputActions.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

        inputActions.Player.Look.performed += ctx => LookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => LookInput = Vector2.zero;

        //Définie les valeurs des bool (Pour savoir si la touche est appuyer)

        inputActions.Player.Jump.performed += ctx => JumpPressed = true;
        inputActions.Player.Jump.canceled += ctx => JumpPressed = false;

        inputActions.Player.Interact.performed += ctx => InteractPressed = true;
        inputActions.Player.Interact.canceled += ctx => InteractPressed = false;


    }

    private void OnDisable()
    {
        inputActions.Disable();
    }
}
