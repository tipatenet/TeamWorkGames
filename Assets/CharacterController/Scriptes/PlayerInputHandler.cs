using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{

    //Gestion des Input:
    public static PlayerInputHandler Instance;

    //Stock les valeurs dans des vecteur 2 :
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public Vector2 ScrollInventory { get; private set; }
    public Vector2 RotateItemInspact { get; private set; }

    //Stock dans une bool :
    public bool JumpPressed { get; private set; }

    //Stock la touche interact :
    public bool InteractPressed { get; private set; }

    //Drop Item touche :
    public bool DropItemPressed { get; private set; }

    //Inspact item touch :
    public bool InspactItem { get; private set; }


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

        inputActions.Player.Scroll.performed += ctx => ScrollInventory = ctx.ReadValue<Vector2>();
        inputActions.Player.Scroll.canceled += ctx => ScrollInventory = Vector2.zero;

        inputActions.Player.RotateItem.performed += ctx => RotateItemInspact = ctx.ReadValue<Vector2>();
        inputActions.Player.RotateItem.canceled += ctx => RotateItemInspact = Vector2.zero;

        //Définie les valeurs des bool (Pour savoir si la touche est appuyer)

        inputActions.Player.Jump.performed += ctx => JumpPressed = true;
        inputActions.Player.Jump.canceled += ctx => JumpPressed = false;

        inputActions.Player.Interact.performed += ctx => InteractPressed = true;
        inputActions.Player.Interact.canceled += ctx => InteractPressed = false;

        inputActions.Player.DropItem.performed += ctx => DropItemPressed = true;
        inputActions.Player.DropItem.canceled += ctx => DropItemPressed = false;

        inputActions.Player.Inspact.performed += ctx => InspactItem = true;
        inputActions.Player.Inspact.canceled += ctx => InspactItem = false;



        //Définis Axis


    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    //Attention à compléter à chaque fois que on rajoute un input look les input sauf pour ceux qui doive pas etre look
    public void LockGameplayInputs(bool locked)
    {
        if (locked)
        {
            inputActions.Player.Move.Disable();
            inputActions.Player.Look.Disable();
            inputActions.Player.Jump.Disable();
            inputActions.Player.Scroll.Disable();
            inputActions.Player.Interact.Disable();
            inputActions.Player.DropItem.Disable();
            inputActions.Player.RotateItem.Enable();
        }
        else
        {
            inputActions.Player.Move.Enable();
            inputActions.Player.Look.Enable();
            inputActions.Player.Jump.Enable();
            inputActions.Player.Scroll.Enable();
            inputActions.Player.Interact.Enable();
            inputActions.Player.DropItem.Enable();
            inputActions.Player.RotateItem.Disable();
        }
    }
}
